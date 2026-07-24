using EnterpriseAiPlatform.Cli.Commands;

namespace EnterpriseAiPlatform.Cli;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
        {
            PrintHelp();
            return 0;
        }

        string command = args[0].ToLowerInvariant();

        return command switch
        {
            "prompt" => await CliCommands.ExecutePromptAsync(GetArgValue(args, 1, "Explain Clean Architecture")),
            "agent" when args.Length > 1 && args[1].ToLowerInvariant() == "run" => await CliCommands.RunAgentAsync(GetArgValue(args, 2, "Refactor module")),
            "rag" when args.Length > 1 && args[1].ToLowerInvariant() == "search" => await CliCommands.SearchRagAsync(GetArgValue(args, 2, "coding standards")),
            "status" => await CliCommands.ShowStatusAsync(),
            "models" => await CliCommands.ListModelsAsync(),
            _ => UnknownCommand(command)
        };
    }

    private static string GetArgValue(string[] args, int index, string fallback)
    {
        return args.Length > index ? string.Join(" ", args.Skip(index)) : fallback;
    }

    private static int UnknownCommand(string command)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Unknown command '{command}'. Run 'ai-cli --help' to view available commands.");
        Console.ResetColor();
        return 1;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("⚡ Enterprise AI Platform CLI (ai-cli) v1.0.0");
        Console.WriteLine("Usage: ai-cli <command> [options]\n");
        Console.WriteLine("Commands:");
        Console.WriteLine("  prompt <text>         Execute quick prompt against AI Gateway");
        Console.WriteLine("  agent run <goal>      Run autonomous agent workflow task");
        Console.WriteLine("  rag search <query>    Search internal RAG knowledge base");
        Console.WriteLine("  status                Inspect Gateway, Circuit Breakers, and Token Quotas");
        Console.WriteLine("  models                List available models and local GPU cluster nodes");
    }
}
