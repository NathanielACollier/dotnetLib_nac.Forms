using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using nac.Forms.model;

namespace nac.Forms
{
    public partial class Form
    {
        /*
         * Generic hierarchical-path navigator (a.k.a. folder / tree path builder).
         *
         * This is the Avalonia / nac.Forms equivalent of the WPF "FolderPathControl" style control,
         * but generic across what a "path" means:
         *   + pathSeparator         — the string between segments (default "\\", but can be "/", "::", ":" ...)
         *   + populateSubFolders    — YOUR function that lists candidate segment names for the next segment,
         *                             given (the already-committed path, the partial text they are typing)
         *
         * The full committed path is stored on the model field (pathFieldName) and stays two-way bound, so the
         * model is always the source of truth.  Use onPathChanged if you want to react when the path changes
         * (from the user picking/committing a segment, going up, or the model being changed elsewhere).
         *
         * Example:
         *   f.PathNavigatorFor("myTreePath",
         *       populateSubFolders: async (committedPath, partialText) =>
         *       {
         *           var list = await myClient.GetChildren(committedPath);
         *           if (string.IsNullOrWhiteSpace(partialText)) return list;
         *           return list.Where(n => n.StartsWith(partialText, System.StringComparison.OrdinalIgnoreCase));
         *       },
         *       pathSeparator: "\\",
         *       initialPath: "Root\\Projects",
         *       onPathChanged: async newTreePath => { await DoSomething(newTreePath); });
         */
        public Form PathNavigatorFor(
            string pathFieldName,
            Func<string, string, Task<IEnumerable<string>>> populateSubFolders,
            string pathSeparator = "\\",
            string initialPath = null,
            Func<string, Task> onPathChanged = null,
            Style style = null)
        {
            if (populateSubFolders == null)
            {
                throw new Exception(
                    $"{nameof(populateSubFolders)} is required for {nameof(PathNavigatorFor)}.  " +
                    "Pass a function that, given (committedPath, partialSegmentText), returns the list of candidate segment names.");
            }

            // make sure the model has a value for this to start out
            setModelIfNull(pathFieldName, initialPath ?? string.Empty);

            var nav = new controls.PathNavigator
            {
                PathSeparator = pathSeparator,
                PopulateSubFolders = populateSubFolders
            };

            AddBinding<string>(pathFieldName, nav, controls.PathNavigator.PathProperty, isTwoWayDataBinding: true);

            if (onPathChanged != null)
            {
                nav.PathChanged += async (_s, _newPath) =>
                {
                    await onPathChanged(_newPath);
                };
            }

            lib.styleUtil.style(this, nav, style);

            AddRowToHost(nav);
            return this;
        }
    }
}
