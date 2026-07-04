using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.ModelRegistry.Application.Abstractions;
using EnterpriseAiPlatform.ModelRegistry.Contracts.Requests;
using EnterpriseAiPlatform.ModelRegistry.Contracts.Responses;
using EnterpriseAiPlatform.ModelRegistry.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.ModelRegistry.Application.Search;

public sealed class CreateModelRegistryHandler(
    IModelRegistryRepository repository,
    IRequestContextAccessor requestContext)
    : ICommandHandler<CreateModelRegistryCommand, ModelRegistryResponse>
{
    public async Task<Result<ModelRegistryResponse>> Handle(
        CreateModelRegistryCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = requestContext.Current.TenantId;
        var existing = await repository.GetByProviderAndNameAsync(
            tenantId,
            command.Request.Provider,
            command.Request.ProviderModelName,
            cancellationToken);

        if (existing is not null)
        {
            return Result.Failure<ModelRegistryResponse>(ModelRegistryErrors.Conflict(
                $"Model '{command.Request.ProviderModelName}' already exists for provider '{command.Request.Provider}'."));
        }

        var entry = new ModelRegistryEntry(
            ModelRegistryEntryId.New(),
            tenantId,
            command.Request.Provider,
            command.Request.ProviderModelName,
            command.Request.DisplayName,
            command.Request.Description,
            ModelRegistryMappings.MapCapabilities(command.Request.Capabilities),
            ModelRegistryMappings.MapPricing(command.Request.Pricing),
            ModelRegistryMappings.MapLatency(command.Request.Latency),
            command.Request.ContextSize,
            ModelRegistryMappings.MapAvailability(command.Request.Availability),
            ModelRegistryMappings.MapHealth(command.Request.Health),
            command.Request.Configuration,
            DateTimeOffset.UtcNow);

        await repository.UpsertAsync(entry, cancellationToken);
        return Result.Success(ModelRegistryMappings.Map(entry));
    }
}

public sealed class UpdateModelRegistryHandler(
    IModelRegistryRepository repository,
    IRequestContextAccessor requestContext)
    : ICommandHandler<UpdateModelRegistryCommand, ModelRegistryResponse>
{
    public async Task<Result<ModelRegistryResponse>> Handle(
        UpdateModelRegistryCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = requestContext.Current.TenantId;
        var entry = await repository.GetByIdAsync(command.ModelId, tenantId, cancellationToken);
        if (entry is null)
        {
            return Result.Failure<ModelRegistryResponse>(ModelRegistryErrors.NotFound(command.ModelId.Value));
        }

        var duplicate = await repository.GetByProviderAndNameAsync(
            tenantId,
            command.Request.Provider,
            command.Request.ProviderModelName,
            cancellationToken);
        if (duplicate is not null && duplicate.Id != entry.Id)
        {
            return Result.Failure<ModelRegistryResponse>(ModelRegistryErrors.Conflict(
                $"Model '{command.Request.ProviderModelName}' already exists for provider '{command.Request.Provider}'."));
        }

        entry.Update(
            command.Request.Provider,
            command.Request.ProviderModelName,
            command.Request.DisplayName,
            command.Request.Description,
            ModelRegistryMappings.MapCapabilities(command.Request.Capabilities),
            ModelRegistryMappings.MapPricing(command.Request.Pricing),
            ModelRegistryMappings.MapLatency(command.Request.Latency),
            command.Request.ContextSize,
            ModelRegistryMappings.MapAvailability(command.Request.Availability),
            ModelRegistryMappings.MapHealth(command.Request.Health),
            command.Request.Configuration,
            DateTimeOffset.UtcNow);

        await repository.UpsertAsync(entry, cancellationToken);
        return Result.Success(ModelRegistryMappings.Map(entry));
    }
}

public sealed class DeleteModelRegistryHandler(
    IModelRegistryRepository repository,
    IRequestContextAccessor requestContext)
    : ICommandHandler<DeleteModelRegistryCommand>
{
    public async Task<Result> Handle(DeleteModelRegistryCommand command, CancellationToken cancellationToken)
    {
        var deleted = await repository.DeleteAsync(command.ModelId, requestContext.Current.TenantId, cancellationToken);
        return deleted
            ? Result.Success()
            : Result.Failure(ModelRegistryErrors.NotFound(command.ModelId.Value));
    }
}

public sealed class GetModelRegistryHandler(
    IModelRegistryRepository repository,
    IRequestContextAccessor requestContext)
    : IQueryHandler<GetModelRegistryQuery, ModelRegistryResponse>
{
    public async Task<Result<ModelRegistryResponse>> Handle(
        GetModelRegistryQuery query,
        CancellationToken cancellationToken)
    {
        var entry = await repository.GetByIdAsync(query.ModelId, requestContext.Current.TenantId, cancellationToken);
        return entry is null
            ? Result.Failure<ModelRegistryResponse>(ModelRegistryErrors.NotFound(query.ModelId.Value))
            : Result.Success(ModelRegistryMappings.Map(entry));
    }
}

public sealed class ListModelRegistryHandler(
    IModelRegistryRepository repository,
    IRequestContextAccessor requestContext)
    : IQueryHandler<ListModelRegistryQuery, ModelRegistryPageResponse>
{
    public async Task<Result<ModelRegistryPageResponse>> Handle(
        ListModelRegistryQuery query,
        CancellationToken cancellationToken)
    {
        var models = await repository.GetByTenantAsync(requestContext.Current.TenantId, cancellationToken);
        var filtered = models.Where(model =>
        {
            if (query.Provider.HasValue && model.Provider != query.Provider.Value)
            {
                return false;
            }

            if (query.Health.HasValue && model.Health.Status != query.Health.Value)
            {
                return false;
            }

            if (query.Available.HasValue && model.Availability.IsAvailable != query.Available.Value)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(query.Search))
            {
                return true;
            }

            var search = query.Search.Trim();
            return model.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || model.ProviderModelName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || (model.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || model.Configuration.Any(item => item.Key.Contains(search, StringComparison.OrdinalIgnoreCase) || item.Value.Contains(search, StringComparison.OrdinalIgnoreCase));
        })
        .OrderByDescending(model => model.UpdatedAtUtc)
        .ToList();

        var take = Math.Min(query.Take, 100);
        var page = filtered.Skip(query.Skip).Take(take).Select(ModelRegistryMappings.Map).ToList();
        return Result.Success(new ModelRegistryPageResponse(query.Skip, take, filtered.Count, page));
    }
}

internal static class ModelRegistryMappings
{
    internal static ModelRegistryResponse Map(ModelRegistryEntry entry)
    {
        return new ModelRegistryResponse(
            entry.Id.Value,
            entry.TenantId.Value,
            entry.Provider,
            entry.ProviderModelName,
            entry.DisplayName,
            entry.Description,
            entry.Capabilities.Select(capability => new ModelCapabilityResponse(
                capability.Name,
                capability.Description,
                capability.Enabled)).ToList(),
            new ModelPricingResponse(
                entry.Pricing.InputTokenCostPer1K,
                entry.Pricing.OutputTokenCostPer1K,
                entry.Pricing.CachedInputTokenCostPer1K,
                entry.Pricing.Currency),
            new ModelLatencyResponse(
                entry.Latency.P50Ms,
                entry.Latency.P95Ms,
                entry.Latency.P99Ms,
                entry.Latency.MeasuredAtUtc),
            entry.ContextSize,
            new ModelAvailabilityResponse(
                entry.Availability.IsAvailable,
                entry.Availability.AvailabilityPercent,
                entry.Availability.Region,
                entry.Availability.LastCheckedUtc),
            new ModelHealthResponse(
                entry.Health.Status,
                entry.Health.Message,
                entry.Health.CheckedAtUtc),
            new Dictionary<string, string>(entry.Configuration, StringComparer.OrdinalIgnoreCase),
            entry.Version,
            entry.CreatedAtUtc,
            entry.UpdatedAtUtc);
    }

    internal static IReadOnlyList<ModelCapability> MapCapabilities(IReadOnlyList<ModelCapabilityRequest> requests)
        => requests.Select(request => new ModelCapability(request.Name, request.Description, request.Enabled)).ToList();

    internal static ModelPricing MapPricing(ModelPricingRequest request)
        => new(request.InputTokenCostPer1K, request.OutputTokenCostPer1K, request.Currency, request.CachedInputTokenCostPer1K);

    internal static ModelLatencyProfile MapLatency(ModelLatencyRequest request)
        => new(request.P50Ms, request.P95Ms, request.P99Ms, request.MeasuredAtUtc);

    internal static ModelAvailabilityProfile MapAvailability(ModelAvailabilityRequest request)
        => new(request.IsAvailable, request.AvailabilityPercent, request.Region, request.LastCheckedUtc);

    internal static ModelHealthProfile MapHealth(ModelHealthRequest request)
        => new(request.Status, request.Message, request.CheckedAtUtc);
}
