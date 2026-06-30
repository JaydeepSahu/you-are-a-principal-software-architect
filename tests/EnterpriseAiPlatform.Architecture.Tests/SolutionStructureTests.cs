namespace EnterpriseAiPlatform.Architecture.Tests;

public sealed class SolutionStructureTests
{
    [Fact]
    public void SolutionContainsRequiredTopLevelProjects()
    {
        string root = FindSolutionRoot();

        string[] expectedProjects =
        [
            "src/BuildingBlocks/EnterpriseAiPlatform.SharedKernel/EnterpriseAiPlatform.SharedKernel.csproj",
            "src/BuildingBlocks/EnterpriseAiPlatform.Application.Abstractions/EnterpriseAiPlatform.Application.Abstractions.csproj",
            "src/BuildingBlocks/EnterpriseAiPlatform.Infrastructure.Abstractions/EnterpriseAiPlatform.Infrastructure.Abstractions.csproj",
            "src/BuildingBlocks/EnterpriseAiPlatform.Contracts/EnterpriseAiPlatform.Contracts.csproj",
            "src/BuildingBlocks/EnterpriseAiPlatform.ServiceDefaults/EnterpriseAiPlatform.ServiceDefaults.csproj",
            "src/Gateways/EnterpriseAiPlatform.AiGateway.Api/EnterpriseAiPlatform.AiGateway.Api.csproj",
            "src/Services/Identity/EnterpriseAiPlatform.Identity.Api/EnterpriseAiPlatform.Identity.Api.csproj",
            "src/Services/Policy/EnterpriseAiPlatform.Policy.Api/EnterpriseAiPlatform.Policy.Api.csproj",
            "src/Services/ModelRegistry/EnterpriseAiPlatform.ModelRegistry.Api/EnterpriseAiPlatform.ModelRegistry.Api.csproj",
            "src/Services/Routing/EnterpriseAiPlatform.Routing.Api/EnterpriseAiPlatform.Routing.Api.csproj",
            "src/Services/ProviderAdapters/EnterpriseAiPlatform.ProviderAdapters.Host/EnterpriseAiPlatform.ProviderAdapters.Host.csproj",
            "src/Services/Metering/EnterpriseAiPlatform.Metering.Api/EnterpriseAiPlatform.Metering.Api.csproj",
            "src/Services/Audit/EnterpriseAiPlatform.Audit.Api/EnterpriseAiPlatform.Audit.Api.csproj",
            "src/Services/Observability/EnterpriseAiPlatform.Observability.Api/EnterpriseAiPlatform.Observability.Api.csproj",
            "src/Services/PortalBff/EnterpriseAiPlatform.PortalBff.Api/EnterpriseAiPlatform.PortalBff.Api.csproj",
            "src/WorkerServices/EnterpriseAiPlatform.BackgroundWorkers.Host/EnterpriseAiPlatform.BackgroundWorkers.Host.csproj"
        ];

        foreach (string expectedProject in expectedProjects)
        {
            Assert.True(
                File.Exists(Path.Combine(root, expectedProject)),
                $"Expected project was not found: {expectedProject}");
        }
    }

    [Fact]
    public void TemplatePlaceholderFilesHaveBeenRemoved()
    {
        string root = FindSolutionRoot();

        string[] placeholderFiles = Directory
            .EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(path =>
                Path.GetFileName(path) is "Class1.cs" or "UnitTest1.cs" or "Worker.cs")
            .Select(path => Path.GetRelativePath(root, path))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.Empty(placeholderFiles);
    }

    private static string FindSolutionRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            string solutionPath = Path.Combine(directory.FullName, "EnterpriseAiPlatform.sln");
            if (File.Exists(solutionPath))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate EnterpriseAiPlatform.sln.");
    }
}
