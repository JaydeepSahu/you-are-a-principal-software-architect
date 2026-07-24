using System.Diagnostics.CodeAnalysis;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EnterpriseAiPlatform.BackgroundWorkers.Infrastructure;

[SuppressMessage("Reliability", "CA1848:Use the LoggerMessage delegates", Justification = "Background worker lifecycle logging.")]
public sealed class MeteringAggregationWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<MeteringAggregationWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("MeteringAggregationWorker started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                using var scope = scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                await mediator.Send(new Contracts.MeteringAggregationCommand(
                    Guid.NewGuid(),
                    DateTimeOffset.UtcNow.AddMinutes(-5),
                    DateTimeOffset.UtcNow), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "MeteringAggregationWorker encountered an error.");
            }
        }
        logger.LogInformation("MeteringAggregationWorker stopped.");
    }
}

[SuppressMessage("Reliability", "CA1848:Use the LoggerMessage delegates", Justification = "Background worker lifecycle logging.")]
public sealed class PolicyCleanupWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<PolicyCleanupWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PolicyCleanupWorker started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                using var scope = scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                await mediator.Send(new Contracts.CleanupExpiredPoliciesCommand(Guid.NewGuid()), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "PolicyCleanupWorker encountered an error.");
            }
        }
        logger.LogInformation("PolicyCleanupWorker stopped.");
    }
}

[SuppressMessage("Reliability", "CA1848:Use the LoggerMessage delegates", Justification = "Background worker lifecycle logging.")]
public sealed class AuditFlushWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<AuditFlushWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("AuditFlushWorker started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                using var scope = scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                await mediator.Send(new Contracts.FlushAuditBufferCommand(Guid.NewGuid(), 100), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AuditFlushWorker encountered an error.");
            }
        }
        logger.LogInformation("AuditFlushWorker stopped.");
    }
}

public static class DependencyInjection
{
    public static IServiceCollection AddBackgroundWorkersInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddHostedService<MeteringAggregationWorker>();
        services.AddHostedService<PolicyCleanupWorker>();
        services.AddHostedService<AuditFlushWorker>();
        return services;
    }
}
