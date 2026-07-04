using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Routing.Contracts.Requests;
using EnterpriseAiPlatform.Routing.Contracts.Responses;

namespace EnterpriseAiPlatform.Routing.Application.Selection;

public sealed record UpsertRoutingConfigurationCommand(RoutingConfigurationRequest Request) : ICommand<RoutingConfigurationResponse>;

public sealed record GetRoutingConfigurationQuery : IQuery<RoutingConfigurationResponse>;

public sealed record DeleteRoutingConfigurationCommand : ICommand;

public sealed record EvaluateRouteCommand(RouteEvaluationRequest Request) : ICommand<RouteEvaluationResponse>;

public sealed record RoutingModeQuery(int Take = 25) : IQuery<IReadOnlyList<string>>;
