using System;
using System.Collections.Generic;
using System.IO;
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


        public static void LocalDiskTraversal(Form f)
        {
            // Walk the real local filesystem instead of a fake tree, cross-platform.
            //
            //   + DirectoryInfo(...).Root.FullName -> the platform root ("C:\" on Windows,
            //                                "/" on Linux/macOS).
            //   + Path.DirectorySeparatorChar       -> this OS's separator ("\\" on Windows,
            //                                "/" elsewhere).
            //   + Path.GetFileName()                -> turn a full child path back into just its
            //                                name (OS-agnostic).
            //   + Directory.GetDirectories()        -> immediate child folders of a path on any OS.
            //
            // The control's committed path holds just the segments it has chosen, joined by the
            // separator ("", "Windows", "Windows\System32", ...).  We re-attach the root to get
            // the real directory on disk: "" means "at the root".

            string separator = System.IO.Path.DirectorySeparatorChar.ToString();
            string root = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory())
                .Root.FullName;

            f.Model["localFolderPath"] = "";

            // The list of top folders under the currently committed path, shown next to the
            // navigator so the test is easy to eyeball.  Rebuilt every time the path changes.
            var folderList = new System.Collections.ObjectModel.ObservableCollection<string>();
            f.Model["localFolderList"] = folderList;

            // Given the committed (relative) path, rebuild folderList with the immediate
            // child folder names (top 20).  "" means "at the root".
            void refreshFolderList(string committedPath)
            {
                string directory = string.IsNullOrEmpty(committedPath)
                    ? root
                    : System.IO.Path.Combine(root, committedPath);

                var names = new List<string>();

                try
                {
                    names = System.IO.Directory
                        .GetDirectories(directory)
                        .Select(full => System.IO.Path.GetFileName(full))
                        .Where(name => !name.StartsWith("$"))   // hide $Recycle.Bin etc.
                        .Take(20)
                        .ToList();
                }
                catch
                {
                    // A folder we can't read (permissions, transient) shouldn't crash the control.
                }

                folderList.Clear();
                foreach (var name in names)
                {
                    folderList.Add(name);
                }
            }

            // seed the initial list for the starting path
            refreshFolderList(string.Empty);

            // Given (committedPath, partialText) return the matching immediate child-folder names.
            Func<string, string, Task<IEnumerable<string>>> populateSubFolders =
                (committedPath, partialText) =>
            {
                // Map the control's relative committed path back to an absolute directory on disk.
                string directory = string.IsNullOrEmpty(committedPath)
                    ? root
                    : System.IO.Path.Combine(root, committedPath);

                var children = new List<string>();

                try
                {
                    // Directory.GetDirectories returns full paths; Path.GetFileName gives just
                    // the segment name, so it is identical on Windows and Unix.
                    children = System.IO.Directory
                        .GetDirectories(directory)
                        .Select(full => System.IO.Path.GetFileName(full))
                        .Where(name => !name.StartsWith("$"))   // hide $Recycle.Bin etc.
                        .ToList();
                }
                catch
                {
                    // A folder we can't read (permissions, transient) shouldn't crash the control.
                    return Task.FromResult<IEnumerable<string>>(Enumerable.Empty<string>());
                }

                if (!string.IsNullOrWhiteSpace(partialText))
                {
                    children = children
                        .Where(name =>
                            name.StartsWith(partialText, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                return Task.FromResult<IEnumerable<string>>(children);
            };

            f.PathNavigatorFor("localFolderPath",
                    populateSubFolders: populateSubFolders,
                    pathSeparator: separator,
                    initialPath: "",
                    onPathChanged: (newPath) =>
                    {
                        // Re-attach the root so the log shows the real absolute path.
                        string abs = string.IsNullOrEmpty(newPath) ? root : System.IO.Path.Combine(root, newPath);
                        log.Debug($"LocalDiskTraversal new path: {abs}");

                        // keep the "top 20 folders" list in sync with the new location
                        refreshFolderList(newPath);
                        return Task.CompletedTask;
                    })
                .HorizontalGroup(hg =>
                {
                    hg.Text("Browsing (segments): ")
                        .TextFor("localFolderPath");
                })
                .HorizontalGroup(hg =>
                {
                    hg.Text("Root: ")
                        .Text(root);
                })
                .Text("Top 20 folders in current path:")
                .List<string>(itemSourcePropertyName: "localFolderList");
        }
    }
}
