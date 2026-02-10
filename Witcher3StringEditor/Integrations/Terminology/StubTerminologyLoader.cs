using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Integrations.Terminology;

public sealed class StubTerminologyLoader : ITerminologyLoader, IStyleGuideLoader
{
    public Task<TerminologyPack> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return Task.FromResult(new TerminologyPack("Stub Terminology", string.Empty, Array.Empty<TerminologyEntry>()));
        }

        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".tsv" => LoadDelimitedAsync(path, '\t', cancellationToken),
            ".csv" => LoadDelimitedAsync(path, ',', cancellationToken),
            ".md" or ".markdown" => LoadFromMarkdownAsync(path, cancellationToken),
            _ => Task.FromResult(new TerminologyPack(Path.GetFileNameWithoutExtension(path), path,
                Array.Empty<TerminologyEntry>()))
        };
    }

    async Task<StyleGuide> IStyleGuideLoader.LoadAsync(string path, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return new StyleGuide("Stub Style Guide", string.Empty, new List<StyleGuideSection>(),
                Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
        }

        var extension = Path.GetExtension(path).ToLowerInvariant();
        if (extension is not ".md" and not ".markdown")
        {
            return new StyleGuide(Path.GetFileNameWithoutExtension(path), path, new List<StyleGuideSection>(),
                Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
        }

        var lines = await ReadAllLinesAsync(path, cancellationToken);
        return ParseStyleGuide(path, lines);
    }

    private static async Task<TerminologyPack> LoadDelimitedAsync(string path, char delimiter,
        CancellationToken cancellationToken)
    {
        var lines = await ReadAllLinesAsync(path, cancellationToken);
        if (lines.Count == 0)
        {
            return new TerminologyPack(Path.GetFileNameWithoutExtension(path), path, Array.Empty<TerminologyEntry>());
        }

        var (headers, dataStart) = ParseHeader(lines[0], delimiter);
        var entries = new List<TerminologyEntry>();

        for (var index = dataStart; index < lines.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = lines[index];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var columns = SplitLine(line, delimiter);
            if (columns.Count == 0)
                continue;

            var term = GetColumn(headers, columns, "term", 0);
            if (string.IsNullOrWhiteSpace(term))
                continue;

            var translation = GetColumn(headers, columns, "translation", 1);
            var notes = GetColumn(headers, columns, "notes", 2);
            var mode = GetColumn(headers, columns, "mode", 3);

            entries.Add(new TerminologyEntry(
                term.Trim(),
                translation?.Trim() ?? string.Empty,
                string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
                string.IsNullOrWhiteSpace(mode) ? null : mode.Trim()));
        }

        return new TerminologyPack(Path.GetFileNameWithoutExtension(path), path, entries);
    }

    private static async Task<TerminologyPack> LoadFromMarkdownAsync(string path,
        CancellationToken cancellationToken)
    {
        var lines = await ReadAllLinesAsync(path, cancellationToken);
        var styleGuide = ParseStyleGuide(path, lines);
        var entries = new List<TerminologyEntry>();

        foreach (var term in styleGuide.RequiredTerms.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            entries.Add(new TerminologyEntry(term, term, "Required term from style guide.", "required"));
        }

        foreach (var term in styleGuide.ForbiddenTerms.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            entries.Add(new TerminologyEntry(term, string.Empty, "Forbidden term from style guide.", "forbidden"));
        }

        return new TerminologyPack(styleGuide.Name, styleGuide.SourcePath, entries);
    }

    private static StyleGuide ParseStyleGuide(string path, IReadOnlyList<string> lines)
    {
        var required = new List<string>();
        var forbidden = new List<string>();
        var tone = new List<string>();
        var sectionOrder = new List<string>();
        var sectionRules = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        string? currentSectionName = null;
        var section = StyleGuideSectionKind.None;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith('#'))
            {
                section = ResolveSection(line);
                currentSectionName = ExtractHeadingTitle(line);
                if (!string.IsNullOrWhiteSpace(currentSectionName) &&
                    !sectionRules.ContainsKey(currentSectionName))
                {
                    sectionRules[currentSectionName] = new List<string>();
                    sectionOrder.Add(currentSectionName);
                }
                continue;
            }

            if (!IsBullet(line))
                continue;

            var bulletText = ExtractBulletText(line);
            if (string.IsNullOrWhiteSpace(bulletText))
                continue;

            if (!string.IsNullOrWhiteSpace(currentSectionName) &&
                sectionRules.TryGetValue(currentSectionName, out var rules))
            {
                rules.Add(bulletText);
            }

            switch (section)
            {
                case StyleGuideSectionKind.Required:
                    required.AddRange(ExtractPrimaryTerms(bulletText));
                    break;
                case StyleGuideSectionKind.Forbidden:
                    forbidden.AddRange(ExtractPrimaryTerms(bulletText));
                    break;
                case StyleGuideSectionKind.Tone:
                    tone.Add(bulletText);
                    break;
            }
        }

        var sections = sectionOrder
            .Select(name => new StyleGuideSection
            {
                Name = name,
                Rules = sectionRules[name]
            })
            .ToList();

        return new StyleGuide(Path.GetFileNameWithoutExtension(path), path, sections, required, forbidden, tone);
    }

    private static async Task<IReadOnlyList<string>> ReadAllLinesAsync(string path,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Terminology file not found.", path);

        var lines = await File.ReadAllLinesAsync(path, cancellationToken);
        return lines.ToList();
    }

    private static (Dictionary<string, int> Headers, int DataStart) ParseHeader(string line, char delimiter)
    {
        var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var columns = SplitLine(line, delimiter);
        for (var index = 0; index < columns.Count; index++)
        {
            var name = columns[index].Trim();
            if (string.IsNullOrWhiteSpace(name))
                continue;

            headers[name] = index;
        }

        return (headers, headers.Count == 0 ? 0 : 1);
    }

    private static string? GetColumn(Dictionary<string, int> headers, IReadOnlyList<string> columns,
        string name, int fallbackIndex)
    {
        if (headers.TryGetValue(name, out var index) && index < columns.Count)
            return columns[index];

        if (fallbackIndex < columns.Count)
            return columns[fallbackIndex];

        return null;
    }

    private static List<string> SplitLine(string line, char delimiter)
    {
        // TODO: Replace with a proper CSV/TSV parser to handle quoted delimiters.
        return line.Split(delimiter).Select(value => value.Trim()).ToList();
    }

    private static bool IsBullet(string line)
    {
        return line.StartsWith("- ") || line.StartsWith("* ");
    }

    private static string ExtractBulletText(string line)
    {
        var trimmed = line[1..].Trim();
        return trimmed.Trim('"');
    }

    private static IEnumerable<string> ExtractPrimaryTerms(string text)
    {
        return text
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => value.Trim().Trim('"'))
            .Where(value => !string.IsNullOrWhiteSpace(value));
    }

    private static StyleGuideSectionKind ResolveSection(string heading)
    {
        var normalized = heading.ToLowerInvariant();
        if (normalized.Contains("required"))
            return StyleGuideSectionKind.Required;
        if (normalized.Contains("forbidden"))
            return StyleGuideSectionKind.Forbidden;
        if (normalized.Contains("tone"))
            return StyleGuideSectionKind.Tone;
        return StyleGuideSectionKind.None;
    }

    private static string? ExtractHeadingTitle(string heading)
    {
        var title = heading.Trim().TrimStart('#').Trim();
        return string.IsNullOrWhiteSpace(title) ? null : title;
    }

    private enum StyleGuideSectionKind
    {
        None,
        Required,
        Forbidden,
        Tone
    }
}
