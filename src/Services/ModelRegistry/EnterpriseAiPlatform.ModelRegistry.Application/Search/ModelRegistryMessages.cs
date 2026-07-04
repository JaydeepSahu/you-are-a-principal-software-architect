using EnterpriseAiPlatform.ModelRegistry.Contracts.Requests;
using EnterpriseAiPlatform.ModelRegistry.Contracts.Responses;
using EnterpriseAiPlatform.ModelRegistry.Domain;
using EnterpriseAiPlatform.Application.Abstractions;

namespace EnterpriseAiPlatform.ModelRegistry.Application.Search;

public sealed record CreateModelRegistryCommand(ModelRegistryWriteRequest Request) : ICommand<ModelRegistryResponse>;

public sealed record UpdateModelRegistryCommand(ModelRegistryEntryId ModelId, ModelRegistryWriteRequest Request) : ICommand<ModelRegistryResponse>;

public sealed record DeleteModelRegistryCommand(ModelRegistryEntryId ModelId) : ICommand;

public sealed record GetModelRegistryQuery(ModelRegistryEntryId ModelId) : IQuery<ModelRegistryResponse>;

public sealed record ListModelRegistryQuery(
    ModelProvider? Provider = null,
    ModelHealthStatus? Health = null,
    bool? Available = null,
    string? Search = null,
    int Skip = 0,
    int Take = 25) : IQuery<ModelRegistryPageResponse>;
