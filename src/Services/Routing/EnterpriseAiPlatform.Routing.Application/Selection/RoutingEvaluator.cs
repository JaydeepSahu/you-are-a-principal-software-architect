using EnterpriseAiPlatform.Routing.Application.Abstractions;
using EnterpriseAiPlatform.Routing.Contracts.Requests;
using EnterpriseAiPlatform.Routing.Domain;

namespace EnterpriseAiPlatform.Routing.Application.Selection;

public interface IRoutingEvaluator
{
    RoutingEvaluationResult Evaluate(RoutingConfiguration configuration, RouteEvaluationRequest request);
}

public sealed class RoutingEvaluator : IRoutingEvaluator
{
    public RoutingEvaluationResult Evaluate(RoutingConfiguration configuration, RouteEvaluationRequest request)
    {
        var prompt = request.Prompt.Trim();
        var systemPrompt = request.SystemPrompt?.Trim();
        var text = string.Join('\n', new[] { systemPrompt, prompt }.Where(value => !string.IsNullOrWhiteSpace(value)));
        var category = Classify(text, request.TaskType);
        var complexityScore = EstimateComplexityScore(text, request.RequiredCapabilities, request.Metadata);
        var complexity = ToComplexity(complexityScore);
        var estimatedInputTokens = EstimateInputTokens(text, request.Metadata);
        var estimatedOutputTokens = EstimateOutputTokens(complexity, request.MaxTokens);
        var targetMode = request.RequestedMode ?? configuration.Mode;
        var scopeProfile = ResolveScopeProfile(configuration, request.Department, request.Repository);
        var candidates = FilterCandidates(configuration, request, scopeProfile, estimatedInputTokens + estimatedOutputTokens);
        var matchedRule = MatchRule(configuration, request, complexity, estimatedInputTokens, estimatedOutputTokens);

        if (matchedRule?.SelectedModelKey is { } selectedKeyFromRule)
        {
            var selectedFromRule = candidates.FirstOrDefault(candidate => candidate.ModelKey.Equals(selectedKeyFromRule, StringComparison.OrdinalIgnoreCase))
                ?? configuration.GetModel(selectedKeyFromRule);
            if (selectedFromRule is not null && IsAllowedByScope(selectedFromRule, scopeProfile))
            {
                return BuildDecision(configuration, request, targetMode, category, complexity, complexityScore, estimatedInputTokens, estimatedOutputTokens, candidates, selectedFromRule, matchedRule.Name, scopeProfile, matchedRule.MaximumEstimatedCostUsd);
            }
        }

        var selected = SelectCandidate(configuration, request, targetMode, category, complexity, complexityScore, estimatedInputTokens, estimatedOutputTokens, candidates, scopeProfile, matchedRule);
        return BuildDecision(configuration, request, targetMode, category, complexity, complexityScore, estimatedInputTokens, estimatedOutputTokens, candidates, selected, matchedRule?.Name, scopeProfile, scopeProfile?.MaximumEstimatedCostUsd ?? request.MaxCostUsd);
    }

    private static RoutingEvaluationResult BuildDecision(
        RoutingConfiguration configuration,
        RouteEvaluationRequest request,
        RoutingMode mode,
        RoutingRequestCategory category,
        RoutingRequestComplexity complexity,
        int complexityScore,
        int estimatedInputTokens,
        int estimatedOutputTokens,
        List<RoutingModelProfile> candidates,
        RoutingModelProfile selected,
        string? matchedRule,
        RoutingScopeProfile? scopeProfile,
        decimal? budgetLimit)
    {
        var estimatedCost = EstimateCost(selected, estimatedInputTokens, estimatedOutputTokens);
        var alternates = candidates
            .Where(candidate => candidate.ModelKey != selected.ModelKey)
            .Select(candidate => new RoutingModelOption(
                candidate.ModelKey,
                candidate.Provider,
                candidate.ProviderModelName,
                candidate.DisplayName,
                ScoreCandidate(configuration, request, mode, candidate, estimatedInputTokens, estimatedOutputTokens, scopeProfile, budgetLimit, false),
                EstimateCost(candidate, estimatedInputTokens, estimatedOutputTokens),
                estimatedInputTokens + estimatedOutputTokens,
                candidate.P50Ms,
                candidate.AvailabilityPercent))
            .OrderByDescending(option => option.Score)
            .Take(configuration.Defaults.MaxAlternates)
            .ToList();

        return new RoutingEvaluationResult(
            mode,
            category,
            complexity,
            complexityScore,
            estimatedInputTokens,
            estimatedOutputTokens,
            estimatedCost,
            selected.ModelKey,
            selected.Provider,
            selected.ProviderModelName,
            selected.DisplayName,
            BuildReason(mode, category, matchedRule, scopeProfile, selected),
            matchedRule,
            request.Department,
            request.Repository,
            budgetLimit.HasValue && estimatedCost > budgetLimit.Value,
            budgetLimit.HasValue ? budgetLimit - estimatedCost : null,
            alternates.Select(option => new RoutingModelCandidate(
                option.ModelKey,
                option.Provider,
                option.ProviderModelName,
                option.DisplayName,
                option.Score,
                option.EstimatedCostUsd,
                option.EstimatedTokens,
                option.EstimatedLatencyMs,
                option.AvailabilityPercent)).ToList(),
            DateTimeOffset.UtcNow);
    }

    private static RoutingModelProfile SelectCandidate(
        RoutingConfiguration configuration,
        RouteEvaluationRequest request,
        RoutingMode mode,
        RoutingRequestCategory category,
        RoutingRequestComplexity complexity,
        int complexityScore,
        int estimatedInputTokens,
        int estimatedOutputTokens,
        IReadOnlyList<RoutingModelProfile> candidates,
        RoutingScopeProfile? scopeProfile,
        RoutingRule? matchedRule)
    {
        var targetBudget = scopeProfile?.MaximumEstimatedCostUsd ?? request.MaxCostUsd;
        var ranked = candidates
            .Select(candidate => new
            {
                Candidate = candidate,
                Score = ScoreCandidate(configuration, request, mode, candidate, estimatedInputTokens, estimatedOutputTokens, scopeProfile, targetBudget, true),
                Cost = EstimateCost(candidate, estimatedInputTokens, estimatedOutputTokens),
            })
            .Where(item => !targetBudget.HasValue || item.Cost <= targetBudget.Value || mode == RoutingMode.Ml)
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Cost)
            .ThenByDescending(item => item.Candidate.AvailabilityPercent)
            .ToList();

        if (ranked.Count > 0)
        {
            return ranked[0].Candidate;
        }

        if (configuration.Defaults.FallbackModelKey is not null)
        {
            var fallback = configuration.GetModel(configuration.Defaults.FallbackModelKey);
            if (fallback is not null)
            {
                return fallback;
            }
        }

        throw new InvalidOperationException("No viable routing model was found.");
    }

    private static double ScoreCandidate(
        RoutingConfiguration configuration,
        RouteEvaluationRequest request,
        RoutingMode mode,
        RoutingModelProfile candidate,
        int estimatedInputTokens,
        int estimatedOutputTokens,
        RoutingScopeProfile? scopeProfile,
        decimal? targetBudget,
        bool applyRules)
    {
        var capabilityScore = ScoreCapabilities(candidate, request.RequiredCapabilities);
        var costScore = ScoreCost(candidate, estimatedInputTokens, estimatedOutputTokens, targetBudget);
        var latencyScore = ScoreLatency(candidate, request.Metadata);
        var availabilityScore = candidate.AvailabilityPercent / 100d;
        var healthScore = candidate.Health switch
        {
            RoutingHealthStatus.Healthy => 1d,
            RoutingHealthStatus.Degraded => 0.6d,
            RoutingHealthStatus.Maintenance => 0.1d,
            RoutingHealthStatus.Unavailable => 0d,
            _ => 0.4d,
        };
        var contextScore = candidate.ContextSize <= 0 ? 0d : Math.Min(1d, (candidate.ContextSize - estimatedInputTokens) / (double)candidate.ContextSize);
        var budgetScore = targetBudget is null ? 1d : Math.Max(0d, 1d - ((double)EstimateCost(candidate, estimatedInputTokens, estimatedOutputTokens) / Math.Max((double)targetBudget.Value, 0.01d)));
        var ruleScore = applyRules ? ScopeBoost(candidate, scopeProfile, request) : 0d;

        var total =
            capabilityScore * configuration.Weights.CapabilityWeight
            + costScore * configuration.Weights.CostWeight
            + latencyScore * configuration.Weights.LatencyWeight
            + availabilityScore * configuration.Weights.AvailabilityWeight
            + healthScore * configuration.Weights.HealthWeight
            + contextScore * configuration.Weights.ContextHeadroomWeight
            + budgetScore * configuration.Weights.BudgetWeight
            + ruleScore * configuration.Weights.RuleWeight;

        if (mode == RoutingMode.Budget)
        {
            total += budgetScore * 0.35d;
        }

        if (mode == RoutingMode.Department || mode == RoutingMode.Repository)
        {
            total += ruleScore * 0.2d;
        }

        return total;
    }

    private static RoutingRequestCategory Classify(string text, string? taskType)
    {
        if (!string.IsNullOrWhiteSpace(taskType))
        {
            var normalizedTask = taskType.Trim().ToLowerInvariant();
            if (normalizedTask.Contains("code") || normalizedTask.Contains("refactor") || normalizedTask.Contains("bug"))
            {
                return RoutingRequestCategory.Code;
            }

            if (normalizedTask.Contains("summary"))
            {
                return RoutingRequestCategory.Summarization;
            }

            if (normalizedTask.Contains("plan") || normalizedTask.Contains("design"))
            {
                return RoutingRequestCategory.Planning;
            }
        }

        var lower = text.ToLowerInvariant();
        if (lower.Contains("summarize") || lower.Contains("summary") || lower.Contains("brief"))
        {
            return RoutingRequestCategory.Summarization;
        }

        if (lower.Contains("refactor") || lower.Contains("compile") || lower.Contains("class ") || lower.Contains("function ") || lower.Contains("method "))
        {
            return RoutingRequestCategory.Code;
        }

        if (lower.Contains("extract") || lower.Contains("parse") || lower.Contains("schema"))
        {
            return RoutingRequestCategory.Extraction;
        }

        if (lower.Contains("plan") || lower.Contains("architecture") || lower.Contains("roadmap"))
        {
            return RoutingRequestCategory.Planning;
        }

        if (lower.Contains("analyze") || lower.Contains("compare") || lower.Contains("evaluate"))
        {
            return RoutingRequestCategory.Analysis;
        }

        if (lower.Contains("reason") || lower.Contains("debug") || lower.Contains("troubleshoot"))
        {
            return RoutingRequestCategory.Reasoning;
        }

        if (EstimateInputTokens(text, null) > 6000)
        {
            return RoutingRequestCategory.LongContext;
        }

        return RoutingRequestCategory.Chat;
    }

    private static int EstimateComplexityScore(string text, IReadOnlyList<string>? requiredCapabilities, IReadOnlyDictionary<string, string>? metadata)
    {
        var score = Math.Clamp(EstimateInputTokens(text, metadata) / 60, 0, 100);
        if (text.Contains("plan", StringComparison.OrdinalIgnoreCase)
            || text.Contains("design", StringComparison.OrdinalIgnoreCase)
            || text.Contains("architecture", StringComparison.OrdinalIgnoreCase)
            || text.Contains("roadmap", StringComparison.OrdinalIgnoreCase))
        {
            score += 12;
        }
        if (text.Contains("critical", StringComparison.OrdinalIgnoreCase)
            || text.Contains("migration", StringComparison.OrdinalIgnoreCase)
            || text.Contains("rollback", StringComparison.OrdinalIgnoreCase))
        {
            score += 8;
        }
        if (text.Contains("```", StringComparison.Ordinal))
        {
            score += 10;
        }
        if (text.Contains('?'))
        {
            score += 5;
        }
        if (text.Contains("error", StringComparison.OrdinalIgnoreCase) || text.Contains("exception", StringComparison.OrdinalIgnoreCase))
        {
            score += 8;
        }
        if (requiredCapabilities is { Count: > 0 })
        {
            score += Math.Min(20, requiredCapabilities.Count * 4);
        }

        return Math.Clamp(score, 0, 100);
    }

    private static RoutingRequestComplexity ToComplexity(int score)
        => score switch
        {
            < 15 => RoutingRequestComplexity.Trivial,
            < 35 => RoutingRequestComplexity.Simple,
            < 60 => RoutingRequestComplexity.Moderate,
            < 85 => RoutingRequestComplexity.Complex,
            _ => RoutingRequestComplexity.VeryComplex,
        };

    private static int EstimateInputTokens(string text, IReadOnlyDictionary<string, string>? metadata)
    {
        var approximate = (int)Math.Ceiling(text.Length / 4.0d);
        if (metadata is not null)
        {
            approximate += metadata.Sum(item => (item.Key.Length + item.Value.Length) / 8);
        }

        return Math.Max(1, approximate);
    }

    private static int EstimateOutputTokens(RoutingRequestComplexity complexity, int? maxTokens)
    {
        if (maxTokens is { } explicitMaxTokens)
        {
            return Math.Max(16, explicitMaxTokens);
        }

        return complexity switch
        {
            RoutingRequestComplexity.Trivial => 128,
            RoutingRequestComplexity.Simple => 256,
            RoutingRequestComplexity.Moderate => 512,
            RoutingRequestComplexity.Complex => 768,
            RoutingRequestComplexity.VeryComplex => 1024,
            _ => 256,
        };
    }

    private static decimal EstimateCost(RoutingModelProfile candidate, int inputTokens, int outputTokens)
    {
        var inputCost = ((decimal)inputTokens / 1000m) * candidate.InputTokenCostPer1K;
        var outputCost = ((decimal)outputTokens / 1000m) * candidate.OutputTokenCostPer1K;
        return decimal.Round(inputCost + outputCost, 6, MidpointRounding.AwayFromZero);
    }

    private static double ScoreCapabilities(RoutingModelProfile candidate, IReadOnlyList<string>? requestedCapabilities)
    {
        if (requestedCapabilities is null || requestedCapabilities.Count == 0)
        {
            return 1d;
        }

        var matches = requestedCapabilities.Count(capability => candidate.Capabilities.Any(item => item.Equals(capability, StringComparison.OrdinalIgnoreCase)));
        return matches / (double)requestedCapabilities.Count;
    }

    private static double ScoreCost(RoutingModelProfile candidate, int inputTokens, int outputTokens, decimal? budget)
    {
        var estimated = EstimateCost(candidate, inputTokens, outputTokens);
        if (budget is null)
        {
            return 1d / (1d + (double)estimated);
        }

        var ratio = (double)estimated / Math.Max(0.01d, (double)budget.Value);
        return Math.Clamp(1d - ratio, 0d, 1d);
    }

    private static double ScoreLatency(RoutingModelProfile candidate, IReadOnlyDictionary<string, string>? metadata)
    {
        var expected = candidate.P50Ms;
        if (metadata is not null && metadata.TryGetValue("latency_preference", out var preference) && preference.Equals("low", StringComparison.OrdinalIgnoreCase))
        {
            expected = candidate.P50Ms;
        }

        return 1d / (1d + expected / 1000d);
    }

    private static RoutingRule? MatchRule(
        RoutingConfiguration configuration,
        RouteEvaluationRequest request,
        RoutingRequestComplexity complexity,
        int inputTokens,
        int outputTokens)
    {
        foreach (var rule in configuration.Rules.Where(rule => rule.Enabled).OrderByDescending(rule => rule.Priority))
        {
            if (rule.RequiredDepartments.Count > 0 && !string.IsNullOrWhiteSpace(request.Department) && !rule.RequiredDepartments.Contains(request.Department, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            if (rule.RequiredRepositories.Count > 0 && !string.IsNullOrWhiteSpace(request.Repository) && !rule.RequiredRepositories.Contains(request.Repository, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            if (rule.MinimumComplexity.HasValue && complexity < rule.MinimumComplexity.Value)
            {
                continue;
            }

            if (rule.MaximumEstimatedTokens.HasValue && inputTokens + outputTokens > rule.MaximumEstimatedTokens.Value)
            {
                continue;
            }

            if (rule.MaximumEstimatedCostUsd.HasValue && request.MaxCostUsd.HasValue && request.MaxCostUsd.Value > rule.MaximumEstimatedCostUsd.Value)
            {
                continue;
            }

            if (rule.IncludeKeywords.Count > 0)
            {
                var lower = request.Prompt.ToLowerInvariant();
                if (!rule.IncludeKeywords.Any(keyword => lower.Contains(keyword.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }
            }

            if (rule.ExcludeKeywords.Count > 0)
            {
                var lower = request.Prompt.ToLowerInvariant();
                if (rule.ExcludeKeywords.Any(keyword => lower.Contains(keyword.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }
            }

            if (rule.RequiredCapabilities.Count > 0)
            {
                var anyModelMatches = configuration.Models.Any(model => rule.RequiredCapabilities.All(capability => model.Capabilities.Any(item => item.Equals(capability, StringComparison.OrdinalIgnoreCase))));
                if (!anyModelMatches)
                {
                    continue;
                }
            }

            return rule;
        }

        return null;
    }

    private static List<RoutingModelProfile> FilterCandidates(
        RoutingConfiguration configuration,
        RouteEvaluationRequest request,
        RoutingScopeProfile? scopeProfile,
        int totalEstimatedTokens)
    {
        var hasCapabilityFilter = request.RequiredCapabilities is { Count: > 0 };
        var candidates = configuration.Models
            .Where(model => model.Enabled)
            .Where(model => model.Health != RoutingHealthStatus.Unavailable && model.AvailabilityPercent >= configuration.Defaults.MinimumAvailabilityPercent)
            .Where(model => model.ContextSize >= totalEstimatedTokens)
            .Where(model => !hasCapabilityFilter || request.RequiredCapabilities!.All(capability => model.Capabilities.Any(item => item.Equals(capability, StringComparison.OrdinalIgnoreCase))))
            .Where(model => IsAllowedByScope(model, scopeProfile))
            .ToList();

        if (candidates.Count == 0 && hasCapabilityFilter)
        {
            candidates = configuration.Models
                .Where(model => model.Enabled)
                .Where(model => model.Health != RoutingHealthStatus.Unavailable && model.AvailabilityPercent >= configuration.Defaults.MinimumAvailabilityPercent)
                .Where(model => model.ContextSize >= totalEstimatedTokens)
                .Where(model => IsAllowedByScope(model, scopeProfile))
                .ToList();
        }

        return candidates.Count <= configuration.Defaults.MaxCandidates
            ? candidates
            : candidates
                .OrderByDescending(model => model.AvailabilityPercent)
                .ThenBy(model => model.P95Ms)
                .Take(configuration.Defaults.MaxCandidates)
                .ToList();
    }

    private static bool IsAllowedByScope(RoutingModelProfile candidate, RoutingScopeProfile? scopeProfile)
    {
        if (scopeProfile is null || !scopeProfile.Enabled)
        {
            return true;
        }

        if (scopeProfile.AllowedModelKeys.Count > 0 && !scopeProfile.AllowedModelKeys.Contains(candidate.ModelKey, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        if (scopeProfile.AllowedProviders.Count > 0 && !scopeProfile.AllowedProviders.Contains(candidate.Provider))
        {
            return false;
        }

        return true;
    }

    private static RoutingScopeProfile? ResolveScopeProfile(RoutingConfiguration configuration, string? department, string? repository)
    {
        if (!string.IsNullOrWhiteSpace(repository) && configuration.Repositories.TryGetValue(repository.Trim(), out var repositoryProfile) && repositoryProfile.Enabled)
        {
            return repositoryProfile;
        }

        if (!string.IsNullOrWhiteSpace(department) && configuration.Departments.TryGetValue(department.Trim(), out var departmentProfile) && departmentProfile.Enabled)
        {
            return departmentProfile;
        }

        return null;
    }

    private static string BuildReason(RoutingMode mode, RoutingRequestCategory category, string? matchedRule, RoutingScopeProfile? scopeProfile, RoutingModelProfile selected)
    {
        var reasonParts = new List<string> { $"{mode} routing", $"{category} workload", $"selected {selected.ModelKey}" };
        if (matchedRule is not null)
        {
            reasonParts.Add($"rule:{matchedRule}");
        }

        if (scopeProfile is not null)
        {
            reasonParts.Add($"scope:{scopeProfile.Name}");
        }

        return string.Join("; ", reasonParts);
    }

    private static double ScopeBoost(RoutingModelProfile candidate, RoutingScopeProfile? scopeProfile, RouteEvaluationRequest request)
    {
        if (scopeProfile is null)
        {
            return 0.5d;
        }

        var score = 0.5d;
        if (scopeProfile.PreferredModelKey is not null && candidate.ModelKey.Equals(scopeProfile.PreferredModelKey, StringComparison.OrdinalIgnoreCase))
        {
            score += 0.3d;
        }

        if (scopeProfile.AllowedModelKeys.Count > 0 && scopeProfile.AllowedModelKeys.Contains(candidate.ModelKey, StringComparer.OrdinalIgnoreCase))
        {
            score += 0.1d;
        }

        if (scopeProfile.AllowedProviders.Count > 0 && scopeProfile.AllowedProviders.Contains(candidate.Provider))
        {
            score += 0.1d;
        }

        if (!string.IsNullOrWhiteSpace(request.Department) && scopeProfile.Name.Equals(request.Department, StringComparison.OrdinalIgnoreCase))
        {
            score += 0.05d;
        }

        if (!string.IsNullOrWhiteSpace(request.Repository) && scopeProfile.Name.Equals(request.Repository, StringComparison.OrdinalIgnoreCase))
        {
            score += 0.05d;
        }

        return Math.Clamp(score, 0d, 1d);
    }
}
