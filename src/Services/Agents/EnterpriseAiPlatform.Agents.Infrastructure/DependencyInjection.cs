using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Agents.Application.Abstractions;
using EnterpriseAiPlatform.Agents.Infrastructure.Approvals;
using EnterpriseAiPlatform.Agents.Infrastructure.Execution;
using EnterpriseAiPlatform.Agents.Infrastructure.Memory;
using EnterpriseAiPlatform.Agents.Infrastructure.Planning;
using EnterpriseAiPlatform.Agents.Infrastructure.Streaming;
using EnterpriseAiPlatform.Agents.Infrastructure.Tools;
using EnterpriseAiPlatform.Infrastructure.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.Agents.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAgentInfrastructure(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContextAccessor, HttpContextRequestContextAccessor>();
        services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<IRequestContextAccessor>().Current);
        services.AddSingleton<IToolRegistry, ToolRegistry>();
        services.AddSingleton<IAgentMemoryStore, InMemoryAgentMemoryStore>();
        services.AddSingleton<IApprovalManager, ApprovalManager>();
        services.AddSingleton<IAgentStreamBroadcaster, AgentStreamBroadcaster>();
        services.AddScoped<IAgentPlanner, AgentPlanner>();
        services.AddScoped<IAgentExecutor, AgentExecutor>();

        return services;
    }
}

