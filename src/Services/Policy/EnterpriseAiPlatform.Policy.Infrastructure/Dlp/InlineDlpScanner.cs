using System.Text.RegularExpressions;
using EnterpriseAiPlatform.Policy.Application.Abstractions;
using EnterpriseAiPlatform.Policy.Domain.Entities;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Policy.Infrastructure.Dlp;

public sealed class InlineDlpScanner : IDlpScanner
{
    private static readonly List<(DlpRule Rule, Regex CompiledRegex)> BuiltInRules = new()
    {
        // 1. AWS Access Key ID
        (new DlpRule(Guid.NewGuid(), "AWS Access Key", DlpCategory.SecretKey, @"\b(AKIA[0-9A-Z]{16})\b", "[REDACTED_AWS_KEY]", DlpAction.Mask),
         new Regex(@"\b(AKIA[0-9A-Z]{16})\b", RegexOptions.Compiled)),

        // 2. GitHub Personal Access Token
        (new DlpRule(Guid.NewGuid(), "GitHub Personal Access Token", DlpCategory.SecretKey, @"\b(ghp_[a-zA-Z0-9]{36})\b", "[REDACTED_GITHUB_TOKEN]", DlpAction.Block),
         new Regex(@"\b(ghp_[a-zA-Z0-9]{36})\b", RegexOptions.Compiled)),

        // 3. RSA Private Key Header
        (new DlpRule(Guid.NewGuid(), "RSA Private Key", DlpCategory.SecretKey, @"-----BEGIN RSA PRIVATE KEY-----[\s\S]*?-----END RSA PRIVATE KEY-----", "[REDACTED_RSA_PRIVATE_KEY]", DlpAction.Block),
         new Regex(@"-----BEGIN RSA PRIVATE KEY-----[\s\S]*?-----END RSA PRIVATE KEY-----", RegexOptions.Compiled)),

        // 4. DB Connection String Password
        (new DlpRule(Guid.NewGuid(), "Connection String Password", DlpCategory.ConnectionInfo, @"(?i)(password|pwd)\s*=\s*['""]?([^'"";\s]+)['""]?", "$1=[REDACTED_PASSWORD]", DlpAction.Mask),
         new Regex(@"(?i)(password|pwd)\s*=\s*['""]?([^'"";\s]+)['""]?", RegexOptions.Compiled)),

        // 5. US Social Security Number (SSN)
        (new DlpRule(Guid.NewGuid(), "US SSN", DlpCategory.Pii, @"\b\d{3}-\d{2}-\d{4}\b", "[REDACTED_SSN]", DlpAction.Mask),
         new Regex(@"\b\d{3}-\d{2}-\d{4}\b", RegexOptions.Compiled)),

        // 6. Credit Card Number (16 digits with optional dashes/spaces)
        (new DlpRule(Guid.NewGuid(), "Credit Card Number", DlpCategory.Pii, @"\b(?:\d[ -]*?){13,16}\b", "[REDACTED_CREDIT_CARD]", DlpAction.Mask),
         new Regex(@"\b(?:\d[ -]*?){13,16}\b", RegexOptions.Compiled)),

        // 7. Email Address
        (new DlpRule(Guid.NewGuid(), "Email Address", DlpCategory.Pii, @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b", "[REDACTED_EMAIL]", DlpAction.Mask),
         new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b", RegexOptions.Compiled))
    };

    public Task<Result<DlpScanResult>> ScanAndRedactAsync(
        TenantId tenantId,
        string textPayload,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(textPayload))
        {
            return Task.FromResult(Result<DlpScanResult>.Success(new DlpScanResult(textPayload ?? string.Empty, textPayload ?? string.Empty, new List<DlpMatch>(), false, null)));
        }

        string sanitizedText = textPayload;
        var matches = new List<DlpMatch>();
        bool shouldBlock = false;
        string? blockReason = null;

        foreach (var (rule, regex) in BuiltInRules)
        {
            if (!rule.IsEnabled) continue;

            var regexMatches = regex.Matches(sanitizedText);
            foreach (Match match in regexMatches)
            {
                if (!match.Success) continue;

                matches.Add(new DlpMatch(
                    rule.Name,
                    rule.Category,
                    match.Value,
                    match.Index,
                    match.Length,
                    rule.ReplacementToken
                ));

                if (rule.Action == DlpAction.Block)
                {
                    shouldBlock = true;
                    blockReason = $"Payload contains critical secret ({rule.Name}) violation.";
                }

                if (rule.Action == DlpAction.Mask)
                {
                    sanitizedText = regex.Replace(sanitizedText, rule.ReplacementToken);
                }
            }
        }

        var result = new DlpScanResult(textPayload, sanitizedText, matches.AsReadOnly(), shouldBlock, blockReason);
        return Task.FromResult(Result<DlpScanResult>.Success(result));
    }

    public Task<Result<IReadOnlyList<DlpRule>>> GetActiveRulesAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var rules = BuiltInRules.Select(r => r.Rule).ToList().AsReadOnly();
        return Task.FromResult(Result<IReadOnlyList<DlpRule>>.Success(rules));
    }
}
