using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Terminology;

namespace Witcher3StringEditor.Services;

internal sealed class TerminologyLoader : ITerminologyLoader
{
    private static readonly StringComparer ColumnComparer = StringComparer.OrdinalIgnoreCase;

    public Task<TerminologyPack> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Terminology path is required.", nameof(path));

        var extension = Path.GetExtension(path);
        return extension.ToLowerInvariant() switch
        {
            ".csv" => LoadFromCsv(path, cancellationToken),
            ".tsv" => LoadFromTsv(path, cancellationToken),
            ".md" or ".markdown" => LoadFromMarkdown(path, cancellationToken),
            _ => throw new NotSupportedException($"Unsupported terminology file format: '{extension}'.")
        };
    }

    public async Task<TerminologyPack> LoadFromCsv(string path, CancellationToken cancellationToken = default)
    {
        var lines = await ReadAllLinesAsync(path, cancellationToken);
        return BuildPack(path, lines, ',');
    }

    public async Task<TerminologyPack> LoadFromTsv(string path, CancellationToken cancellationToken = default)
    {
        var lines = await ReadAllLinesAsync(path, cancellationToken);
        return BuildPack(path, lines, '\t');
    }

    public async Task<TerminologyPack> LoadFromMarkdown(string path, CancellationToken cancellationToken = default)
    {
        var styleGuide = await LoadStyleGuideFromMarkdown(path, cancellationToken);
        return MapStyleGuideToPack(styleGuide);
    }

    public async Task<TerminologyLoaderValidationResult> ValidateSamplesAsync(string repoRoot,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(repoRoot))
            throw new ArgumentException("Repository root is required.", nameof(repoRoot));

        var terminologyPath = Path.Combine(repoRoot, "docs", "samples", "terminology.sample.tsv");
        var styleGuidePath = Path.Combine(repoRoot, "docs", "samples", "style-guide.sample.md");

        var terminologyPack = await LoadFromTsv(terminologyPath, cancellationToken);
        var styleGuidePack = await LoadFromMarkdown(styleGuidePath, cancellationToken);

        return new TerminologyLoaderValidationResult(terminologyPack, styleGuidePack);
    }

    public async Task<StyleGuide> LoadStyleGuideFromMarkdown(string path,
        CancellationToken cancellationToken = default)
    {
        var lines = await ReadAllLinesAsync(path, cancellationToken);
        var required = new List<string>();
        var forbidden = new List<string>();
        var tone = new List<string>();
        var section = StyleGuideSection.None;

        foreach (var rawLine in lines)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith('#'))
            {
                section = ResolveSection(line);
                continue;
            }

            if (!IsBullet(line))
                continue;

            var bulletText = ExtractBulletText(line);
            if (string.IsNullOrWhiteSpace(bulletText))
                continue;

            switch (section)
            {
                case StyleGuideSection.Required:
                    required.AddRange(ExtractPrimaryTerms(bulletText));
                    break;
                case StyleGuideSection.Forbidden:
                    forbidden.AddRange(ExtractPrimaryTerms(bulletText));
                    break;
                case StyleGuideSection.Tone:
                    tone.Add(bulletText);
                    break;
            }
        }

        return new StyleGuide
        {
            Name = Path.GetFileNameWithoutExtension(path),
            SourcePath = path,
            RequiredTerms = required,
            ForbiddenTerms = forbidden,
            ToneNotes = tone
        };
    }

    private static TerminologyPack MapStyleGuideToPack(StyleGuide styleGuide)
    {
        var entries = new List<TerminologyEntry>();

        foreach (var term in styleGuide.RequiredTerms.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(term))
                continue;

            entries.Add(new TerminologyEntry
            {
                Term = term,
                Translation = term,
                Notes = "Required term from style guide.",
                Mode = "required"
            });
        }

        foreach (var term in styleGuide.ForbiddenTerms.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(term))
                continue;

            entries.Add(new TerminologyEntry
            {
                Term = term,
                Translation = string.Empty,
                Notes = "Forbidden term from style guide.",
                Mode = "forbidden"
            });
        }

        return new TerminologyPack
        {
            Name = styleGuide.Name,
            SourcePath = styleGuide.SourcePath,
            Entries = entries
        };
    }

    private static TerminologyPack BuildPack(string path, IReadOnlyList<string> lines, char delimiter)
    {
        var entries = new List<TerminologyEntry>();
        var header = FindHeader(lines, delimiter, out var dataStart);
        for (var index = dataStart; index < lines.Count; index++)
        {
            var line = lines[index];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var columns = SplitDelimitedLine(line, delimiter);
            if (columns.Count == 0)
                continue;

            var term = GetColumnValue(columns, header, "term", 0);
            if (string.IsNullOrWhiteSpace(term))
                continue;

            var translation = GetColumnValue(columns, header, "translation", 1);
            var notes = GetColumnValue(columns, header, "notes", 2);
            var mode = GetColumnValue(columns, header, "mode", 3);

            entries.Add(new TerminologyEntry
            {
                Term = term,
                Translation = translation,
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes,
                Mode = string.IsNullOrWhiteSpace(mode) ? null : mode
            });
        }

        return new TerminologyPack
        {
            Name = Path.GetFileNameWithoutExtension(path),
            SourcePath = path,
            Entries = entries
        };
    }

    private static Dictionary<string, int>? FindHeader(IReadOnlyList<string> lines, char delimiter, out int dataStart)
    {
        dataStart = 0;
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var columns = SplitDelimitedLine(line, delimiter);
            if (columns.Count == 0)
            {
                dataStart = i + 1;
                return null;
            }

            if (columns.Any(column => ColumnComparer.Equals(column, "term") ||
                                      ColumnComparer.Equals(column, "translation") ||
                                      ColumnComparer.Equals(column, "notes") ||
                                      ColumnComparer.Equals(column, "mode")))
            {
                dataStart = i + 1;
                return columns
                    .Select((value, index) => new { value, index })
                    .ToDictionary(item => item.value, item => item.index, ColumnComparer);
            }

            dataStart = i;
            return null;
        }

        return null;
    }

    private static string GetColumnValue(IReadOnlyList<string> columns, Dictionary<string, int>? header,
        string key, int fallbackIndex)
    {
        if (header is not null && header.TryGetValue(key, out var index) && index < columns.Count)
            return columns[index].Trim();

        return fallbackIndex < columns.Count ? columns[fallbackIndex].Trim() : string.Empty;
    }

    private static List<string> SplitDelimitedLine(string line, char delimiter)
    {
        var results = new List<string>();
        if (string.IsNullOrEmpty(line))
            return results;

        var builder = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var current = line[i];
            if (current == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    builder.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (current == delimiter && !inQuotes)
            {
                results.Add(builder.ToString().Trim());
                builder.Clear();
                continue;
            }

            builder.Append(current);
        }

        results.Add(builder.ToString().Trim());
        return results;
    }

    private static async Task<IReadOnlyList<string>> ReadAllLinesAsync(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Terminology file was not found.", path);

        cancellationToken.ThrowIfCancellationRequested();
        return await File.ReadAllLinesAsync(path, cancellationToken);
    }

    private static StyleGuideSection ResolveSection(string heading)
    {
        var normalized = heading.Trim().TrimStart('#').Trim().ToUpperInvariant();
        if (normalized.Contains("REQUIRED"))
            return StyleGuideSection.Required;
        if (normalized.Contains("FORBIDDEN"))
            return StyleGuideSection.Forbidden;
        if (normalized.Contains("TONE"))
            return StyleGuideSection.Tone;
        return StyleGuideSection.None;
    }

    private static bool IsBullet(string line)
    {
        return line.StartsWith("- ") || line.StartsWith("* ") || line.StartsWith("• ");
    }

    private static string ExtractBulletText(string line)
    {
        return line.Length > 2 ? line[2..].Trim() : string.Empty;
    }

    private static IReadOnlyList<string> ExtractPrimaryTerms(string bulletText)
    {
        var matches = Regex.Matches(bulletText, "\"([^\"]+)\"");
        if (matches.Count > 0)
        {
            return matches
                .Cast<Match>()
                .Select(match => match.Groups[1].Value)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToArray();
        }

        return new[] { bulletText };
    }

    private enum StyleGuideSection
    {
        None,
        Required,
        Forbidden,
        Tone
    }
}

internal sealed record TerminologyLoaderValidationResult(
    TerminologyPack TerminologyPack,
    TerminologyPack StyleGuidePack);
