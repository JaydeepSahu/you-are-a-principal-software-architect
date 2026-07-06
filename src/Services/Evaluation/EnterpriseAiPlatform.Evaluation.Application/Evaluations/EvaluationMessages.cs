using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Evaluation.Contracts.Requests;
using EnterpriseAiPlatform.Evaluation.Contracts.Responses;

namespace EnterpriseAiPlatform.Evaluation.Application.Evaluations;

public sealed record CreateEvaluationCommand(CreateEvaluationRequest Request) : ICommand<CreateEvaluationResponse>;

public sealed record GetEvaluationQuery(Guid EvaluationId) : IQuery<EvaluationResponse>;

public sealed record ListEvaluationsQuery(ListEvaluationsRequest Request) : IQuery<ListEvaluationsResponse>;

public sealed record GetEvaluationStatsQuery(
    string? TargetId = null,
    string? TargetType = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null) : IQuery<EvaluationStatsResponse>;
