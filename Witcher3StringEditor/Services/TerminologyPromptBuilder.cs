using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Terminology;

namespace Witcher3StringEditor.Services;

internal sealed class TerminologyPromptBuilder : ITerminologyPromptBuilder
{
    public Task<TerminologyPrompt> BuildAsync(
        TerminologyPack? terminologyPack,
        TerminologyPack? styleGuidePack,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sections = new List<string>();

        var terminologySection = BuildTerminologySection(terminologyPack);
        if (!string.IsNullOrWhiteSpace(terminologySection))
        {
            sections.Add(terminologySection);
        }

        var styleSection = BuildStyleGuideSection(styleGuidePack);
        if (!string.IsNullOrWhiteSpace(styleSection))
        {
            sections.Add(styleSection);
        }

        if (sections.Count == 0)
        {
            return Task.FromResult(new TerminologyPrompt());
        }

        return Task.FromResult(new TerminologyPrompt
        {
            SystemPrompt = string.Join($"{Environment.NewLine}{Environment.NewLine}", sections)
        });
    }

    private static string? BuildTerminologySection(TerminologyPack? pack)
    {
        if (pack?.Entries is null || pack.Entries.Count == 0)
        {
            return null;
        }

        var builder = new StringBuilder();
        builder.AppendLine("Terminology:");
        foreach (var entry in pack.Entries.OrderBy(e => e.Term, StringComparer.OrdinalIgnoreCase))
        {
            builder.Append("- ").Append(entry.Term);
            if (!string.IsNullOrWhiteSpace(entry.Translation))
            {
                builder.Append(" => ").Append(entry.Translation);
            }

            AppendEntryDetails(builder, entry);
            builder.AppendLine();
        }

        return builder.ToString().TrimEnd();
    }

    private static string? BuildStyleGuideSection(TerminologyPack? pack)
    {
        if (pack?.Entries is null || pack.Entries.Count == 0)
        {
            return null;
        }

        var builder = new StringBuilder();
        builder.AppendLine("Style guide terminology:");
        foreach (var entry in pack.Entries.OrderBy(e => e.Term, StringComparer.OrdinalIgnoreCase))
        {
            builder.Append("- ").Append(entry.Term);
            if (!string.IsNullOrWhiteSpace(entry.Translation))
            {
                builder.Append(" => ").Append(entry.Translation);
            }

            AppendEntryDetails(builder, entry);
            builder.AppendLine();
        }

        return builder.ToString().TrimEnd();
    }

    private static void AppendEntryDetails(StringBuilder builder, TerminologyEntry entry)
    {
        var details = new List<string>();
        if (!string.IsNullOrWhiteSpace(entry.Mode))
        {
            details.Add($"mode: {entry.Mode}");
        }

        if (!string.IsNullOrWhiteSpace(entry.Notes))
        {
            details.Add($"notes: {entry.Notes}");
        }

        if (details.Count == 0)
        {
            return;
        }

        builder.Append(" (").Append(string.Join("; ", details)).Append(')');
    }
}
