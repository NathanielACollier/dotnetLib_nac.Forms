using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nac.Forms;

namespace TestApp.lib.TestFunctionGroups
{
    public class PathNavigator
    {
        private static nac.Logging.Logger log = new();

        /*
         A small in-memory "folder tree" we can point the demo at.
         key    = the committed path so far
         value  = the list of child segment names under that key
         */
        private static Dictionary<string, List<string>> BuildFakeTree(string separator)
        {
            string j = separator;
            return new Dictionary<string, List<string>>(StringComparer.Ordinal)
            {
                { string.Empty,                        new List<string> { "Root" } },
                { "Root",                               new List<string> { "Projects", "Shared", "Temp" } },
                { "Root" + j + "Projects",              new List<string> { "Forms", "Website", "Tools" } },
                { "Root" + j + "Projects" + j + "Forms",new List<string> { "Controls", "Builders" } },
                { "Root" + j + "Shared",                new List<string> { "Docs", "Media" } },
                { "Root" + j + "Temp",                  new List<string> { "cache" } },
            };
        }

        /*
         Generic-ish populator: given the committed path + the partial text, return the matching children.
         Pretends an async lookup.
         */
        private static Func<string, string, Task<IEnumerable<string>>> MakePopulator(Dictionary<string, List<string>> tree)
        {
            return async (committedPath, partialText) =>
            {
                // pretend we're hitting a server somewhere
                await Task.Delay(150);

                var key = committedPath ?? string.Empty;
                if (!tree.TryGetValue(key, out var children))
                {
                    return Enumerable.Empty<string>();
                }

                IEnumerable<string> result = children;
                if (!string.IsNullOrWhiteSpace(partialText))
                {
                    result = result.Where(n => n.StartsWith(partialText, StringComparison.OrdinalIgnoreCase));
                }

                return result;
            };
        }


        public static void PathNavigator_DefaultBackslash(Form f)
        {
            f.Model["myTreePath"] = "";
            var separator = "\\";
            var tree = BuildFakeTree(separator);
            var populator = MakePopulator(tree);

            f.PathNavigatorFor("myTreePath",
                    populateSubFolders: populator,
                    pathSeparator: separator,
                    onPathChanged: async (newPath) => { log.Debug($"New path is: {newPath}"); })
                .HorizontalGroup(hg =>
                {
                    hg.Text("Committed path: ")
                        .TextFor("myTreePath");
                })
                .ObjectViewer(tree);
        }


        public static void PathNavigator_CustomSeparator(Form f)
        {
            // same control, but the segments are separated by "::" (e.g. namespaced / virtual paths)
            f.Model["nsPath"] = "";
            var separator = "::";
            var tree = BuildFakeTree(separator);
            var populator = MakePopulator(tree);

            f.PathNavigatorFor("nsPath",
                    populateSubFolders: populator,
                    pathSeparator: separator,
                    initialPath: "Root::Shared",
                    onPathChanged: async (newPath) => { log.Debug($"New path is: {newPath}"); })
                .HorizontalGroup(hg =>
                {
                    hg.Text("Committed path: ")
                        .TextFor("nsPath");
                });
        }
    }
}
