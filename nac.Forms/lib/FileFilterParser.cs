using System;
using System.Collections.Generic;
using System.Linq;

namespace nac.Forms.lib
{
    /*
     A file filter string describes one or more file types in the format already used
     by .NET style file dialogs:

         "PFX|(*.pfx;*.key)"

     Multiple file types are separated by a double pipe ("||"):

         "PFX|(*.pfx;*.key)||PEM|(*.pem)"

     Parsing is forgiving:
        - whitespace around names, patterns, and separators is ignored
        - the surrounding parentheses on the pattern portion are optional
        - the display name is optional; when missing, the patterns are used as the name
        - a bare extension like "pfx" is turned into "*.pfx"
        - full file name patterns like "report.txt" are left alone
     */
    public class FileFilterEntry
    {
        public FileFilterEntry(string name, List<string> patterns)
        {
            Name = name;
            Patterns = patterns;
        }

        public string Name { get; }
        public List<string> Patterns { get; }
    }

    public static class FileFilterParser
    {
        public static List<FileFilterEntry> Parse(string filter)
        {
            var entries = new List<FileFilterEntry>();

            if (string.IsNullOrWhiteSpace(filter))
            {
                return entries;
            }

            foreach (var entrySpec in filter.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var entry = parseEntry(entrySpec);
                if (entry != null)
                {
                    entries.Add(entry);
                }
            }

            return entries;
        }

        private static FileFilterEntry parseEntry(string entrySpec)
        {
            entrySpec = entrySpec.Trim();
            if (entrySpec.Length == 0)
            {
                return null;
            }

            string name;
            string patternSpec;

            int pipeIndex = entrySpec.IndexOf('|');
            if (pipeIndex >= 0)
            {
                name = entrySpec.Substring(0, pipeIndex).Trim();
                patternSpec = entrySpec.Substring(pipeIndex + 1).Trim();
            }
            else
            {
                name = "";
                patternSpec = entrySpec;
            }

            // surrounding parens are optional:  (*.pfx;*.key)  ==  *.pfx;*.key
            if (patternSpec.Length >= 2 &&
                patternSpec.StartsWith("(") &&
                patternSpec.EndsWith(")"))
            {
                patternSpec = patternSpec.Substring(1, patternSpec.Length - 2);
            }

            var patterns = patternSpec
                .Split(';')
                .Select(pattern => pattern.Trim().Trim('(', ')').Trim())
                .Where(pattern => pattern.Length > 0)
                .Select(normalizePattern)
                .ToList();

            if (patterns.Count == 0)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = string.Join("; ", patterns);
            }

            return new FileFilterEntry(name, patterns);
        }

        private static string normalizePattern(string pattern)
        {
            // "pfx" -> "*.pfx"
            // "report.txt" -> "report.txt" (already a full file name pattern)
            if (pattern.Contains('*') || pattern.Contains('?') || pattern.Contains('.'))
            {
                return pattern;
            }

            return "*" + "." + pattern;
        }
    }
}
