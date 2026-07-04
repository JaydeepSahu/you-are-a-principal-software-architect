using System.Text.RegularExpressions;
using EnterpriseAiPlatform.Knowledge.Application.Abstractions;
using EnterpriseAiPlatform.Knowledge.Application.Models;
using EnterpriseAiPlatform.Knowledge.Domain;

namespace EnterpriseAiPlatform.Knowledge.Infrastructure;

public sealed class HeuristicMetadataExtractor : IMetadataExtractor
{
    public KnowledgeDocumentMetadata Extract(KnowledgeIngestionDocument document)
    {
        var attributes = new Dictionary<string, string>(document.Metadata, StringComparer.OrdinalIgnoreCase)
        {
            ["word_count"] = CountWords(document.Content).ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["source_type"] = document.Source.Type.ToString(),
            ["external_id"] = document.Source.ExternalId,
        };

        if (document.Source.Uri is not null)
        {
            attributes["host"] = document.Source.Uri.Host;
        }

        return new KnowledgeDocumentMetadata(
            document.Title,
            document.ContentType ?? InferContentType(document.Source, document.Content),
            DetectLanguage(document.Content),
            attributes);
    }

    private static string InferContentType(KnowledgeSource source, string content)
    {
        if (source.Uri?.AbsolutePath.EndsWith(".md", StringComparison.OrdinalIgnoreCase) == true ||
            content.Contains("# ", StringComparison.Ordinal))
        {
            return "text/markdown";
        }

        if (source.Type == KnowledgeSourceType.Jira)
        {
            return "application/vnd.atlassian.jira.issue";
        }

        if (source.Type == KnowledgeSourceType.Confluence)
        {
            return "application/vnd.atlassian.confluence.page";
        }

        return "text/plain";
    }

    private static int CountWords(string content)
        => Regex.Matches(content, @"[\p{L}\p{N}_]+").Count;

    private static string DetectLanguage(string content)
    {
        var normalized = " " + content.ToLowerInvariant() + " ";
        var englishScore = CountMarkers(normalized, [" the ", " and ", " for ", " with ", " return ", " document "]);
        var spanishScore = CountMarkers(normalized, [" el ", " la ", " de ", " que ", " para ", " con "]);
        var frenchScore = CountMarkers(normalized, [" le ", " la ", " des ", " pour ", " avec ", " une "]);
        var hindiScore = content.Count(ch => ch >= '\u0900' && ch <= '\u097F');

        var scores = new Dictionary<string, int>
        {
            ["en"] = englishScore,
            ["es"] = spanishScore,
            ["fr"] = frenchScore,
            ["hi"] = hindiScore,
        };

        var best = scores.MaxBy(x => x.Value);
        return best.Value <= 0 ? "unknown" : best.Key;
    }

    private static int CountMarkers(string content, IReadOnlyList<string> markers)
        => markers.Sum(marker => content.Split(marker, StringSplitOptions.None).Length - 1);
}
