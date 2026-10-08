using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;

namespace nac.Forms.controls
{
    /*
     * The app-provided "list candidates for the next segment" function signature is:
     *
     *   Func<string, string, Task<IEnumerable<string>>>
     *
     *   committedPath      — the currently committed path (the full path minus the segment the user is currently typing)
     *   partialSegmentText — the partial text the user has typed for the segment they are about to commit
     *
     * Return value: the list of segment names (children of committedPath) that match partialSegmentText
     * (the app is responsible for doing the filter; the control just displays what comes back).
     *
     * Using Func<...> (rather than a custom delegate) matches the rest of the nac.Forms API
     * (e.g. Autocomplete uses Func<string, Task<IEnumerable<T>>>), and lets consumers just pass a lambda.
     */


    /*
     * Generic hierarchical-path navigator.
     *
     * Mimics what the OpenText/WPF FolderPathControl gives you, but generic:
     *   + Configurable path separator (default "\\", but can be "/", "::", ":", anything)
     *   + App-provided populator function; the control just shows the results and lets the user pick one
     *   + The committed full path is the source of truth (two-way bound to a model field via Form.PathNavigatorFor)
     *
     * Layout:  [ committedPath display ] [ next-segment autocomplete ] [ up-one-level button ]
     * The path display (TextBlock) is a one-way view of Path.
     * The autocomplete box is free for the user to type the next segment; when they commit (Enter or pick a candidate)
     * the segment is appended to Path (using the separator) and the box is cleared.
     * Up pops the last committed segment.
     */
    public class PathNavigator : UserControl
    {
        private static nac.Logging.Logger log = new();

        /*
         -----
         Events
         -----
         */
        public event EventHandler<string> PathChanged;

        /*
         ------
         Properties
         ------
         */

        public static readonly StyledProperty<string> PathProperty =
            AvaloniaProperty.Register<PathNavigator, string>(nameof(Path), defaultValue: string.Empty);

        public string Path
        {
            get { return GetValue(PathProperty); }
            set { SetValue(PathProperty, value ?? string.Empty); }
        }

        public static readonly StyledProperty<string> PathSeparatorProperty =
            AvaloniaProperty.Register<PathNavigator, string>(nameof(PathSeparator), defaultValue: "\\");

        public string PathSeparator
        {
            get { return GetValue(PathSeparatorProperty); }
            set { SetValue(PathSeparatorProperty, value ?? string.Empty); }
        }

        /*
         The app-provided function that returns candidate segment names for the next segment,
         given the currently committed path and the partial text the user has typed.
         */
        public Func<string, string, Task<IEnumerable<string>>> PopulateSubFolders { get; set; }

        /*
         -----
         Members
         -----
         */
        private AutoCompleteBox _nextSegmentBox;
        private TextBlock _pathDisplay;

        /*
         ------
         Constructor
         ------
         */
        public PathNavigator()
        {
            Content = BuildLayout();
        }

        /*
         --------
         Methods
         --------
         */
        private Control BuildLayout()
        {
            var layout = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            _pathDisplay = new TextBlock
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Margin = new Avalonia.Thickness(0, 0, 8, 0)
            };
            // keep the display in sync with Path (one-way observable -> property bind)
            _pathDisplay.Bind(Avalonia.Controls.TextBlock.TextProperty, this.GetObservable(PathProperty));

            _nextSegmentBox = new AutoCompleteBox
            {
                Width = 300,
                Watermark = "next segment"
            };

            // candidates depend on "what we have committed so far" + "what they are typing right now"
            _nextSegmentBox.AsyncPopulator = async (typedText, cancelToken) =>
            {
                if (PopulateSubFolders == null)
                {
                    return Enumerable.Empty<object>();
                }
                try
                {
                    var candidates = await PopulateSubFolders(
                        Path ?? string.Empty,
                        typedText ?? string.Empty);

                    return candidates?.Cast<object>() ?? Enumerable.Empty<object>();
                }
                catch (Exception ex)
                {
                    log.Warn($"[PathNavigator] PopulateSubFolders threw.  Exception: {ex}");
                    return Enumerable.Empty<object>();
                }
            };

            _nextSegmentBox.SelectionChanged += (_s, _args) =>
            {
                var chosen = _args.AddedItems.OfType<string>().FirstOrDefault();
                if (string.IsNullOrWhiteSpace(chosen))
                {
                    return;
                }

                CommitSegment(chosen);
                // reset the box for the next segment
                _nextSegmentBox.SelectedItem = null;
                _nextSegmentBox.Text = string.Empty;
            };

            var upButton = new Button
            {
                Content = "Up",
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(8, 0, 0, 0)
            };
            Avalonia.Controls.ToolTip.SetTip(upButton, "Go up one segment");
            upButton.Click += (_s, _e) => GoUpOneLevel();

            layout.Children.Add(_pathDisplay);
            layout.Children.Add(_nextSegmentBox);
            layout.Children.Add(upButton);

            return layout;
        }


        /*
         --------
         Pure path-segment logic (static, unit-testable with no UI)
         --------
         */

        public static string JoinPath(string committedPath, string segment, string separator)
        {
            committedPath = committedPath ?? string.Empty;
            segment = segment ?? string.Empty;
            separator = separator ?? string.Empty;

            if (string.IsNullOrEmpty(segment)) return committedPath;
            if (string.IsNullOrEmpty(committedPath)) return segment;

            return committedPath + separator + segment;
        }


        public static string PopTopSegment(string committedPath, string separator)
        {
            committedPath = committedPath ?? string.Empty;
            separator = separator ?? string.Empty;

            if (string.IsNullOrWhiteSpace(committedPath)) return string.Empty;

            // no separator means single-segment path — up just clears it
            if (string.IsNullOrEmpty(separator)) return string.Empty;

            // split on the separator, dropping any empty segments (protects against trailing sep etc.)
            var parts = committedPath
                .Split(new[] { separator }, StringSplitOptions.None)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            if (parts.Count <= 1)
            {
                return string.Empty;
            }

            parts.RemoveAt(parts.Count - 1);
            return string.Join(separator, parts);
        }


        /*
         --------
         Instance actions (drive the committed Path)
         --------
         */

        private void CommitSegment(string segment)
        {
            Path = JoinPath(Path, segment, PathSeparator); // triggers PathChanged and (two-way) model update
        }


        private void GoUpOneLevel()
        {
            Path = PopTopSegment(Path, PathSeparator);
        }


        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == PathProperty)
            {
                log.Debug($"[PathNavigator] Path -> [{Path}]");
                this.PathChanged?.Invoke(this, Path ?? string.Empty);
            }
        }
    }
}
