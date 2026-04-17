using Flow.Launcher.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace flowx.ui
{
    public partial class settingsview : UserControl
    {
        private enum PresetState
        {
            Recommended,
            All,
            Custom
        }

        private readonly PluginInitContext _context;
        private readonly Settings _settings;
        private readonly IReadOnlyList<SharexCommand> _commands;
        private readonly Dictionary<string, CheckBox> _commandBoxes = new Dictionary<string, CheckBox>();

        public settingsview(PluginInitContext context, Settings settings, IReadOnlyList<SharexCommand> commands)
        {
            InitializeComponent();
            _context = context;
            _settings = settings;
            _commands = commands;

            LoadFromSettings();
            BuildCommandGroups();
            WireEvents();
            UpdatePresetButton();
        }

        private void LoadFromSettings()
        {
            ShareXPathBox.Text = _settings.ShareXPath;
        }

        private static string CategoryLabel(SharexCommand.Cat cat)
        {
            return cat switch
            {
                SharexCommand.Cat.Upload => "upload",
                SharexCommand.Cat.ScreenCapture => "screen capture",
                SharexCommand.Cat.ScreenRecord => "screen record",
                SharexCommand.Cat.Tools => "tools",
                SharexCommand.Cat.Other => "other",
                _ => cat.ToString().ToLowerInvariant()
            };
        }

        private void BuildCommandGroups()
        {
            CommandsPanel.Children.Clear();
            _commandBoxes.Clear();

            foreach (var group in _commands.GroupBy(c => c.Category).OrderBy(g => g.Key))
            {
                var expander = new Expander
                {
                    Header = CategoryLabel(group.Key),
                    IsExpanded = true,
                    Margin = new Thickness(0, 0, 0, 8)
                };

                var wrap = new WrapPanel();
                foreach (var command in group.OrderBy(c => c.Title))
                {
                    var box = new CheckBox
                    {
                        Content = command.Title,
                        Margin = new Thickness(0, 0, 14, 8),
                        IsChecked = !_settings.DisabledCommands.Contains(command.Id),
                        Tag = command.Id
                    };
                    box.Click += (_, __) => Save();
                    wrap.Children.Add(box);
                    _commandBoxes[command.Id] = box;
                }

                expander.Content = wrap;
                CommandsPanel.Children.Add(expander);
            }
        }

        private void WireEvents()
        {
            ShareXPathBox.LostFocus += (_, __) => Save();

            PresetToggleBtn.Click += (_, __) =>
            {
                switch (GetPresetState())
                {
                    case PresetState.Recommended:
                        ApplyAllPreset();
                        break;
                    case PresetState.All:
                    case PresetState.Custom:
                        ApplyRecommendedPreset();
                        break;
                }
            };
        }

        private void ApplyRecommendedPreset()
        {
            foreach (var (id, box) in _commandBoxes)
                box.IsChecked = RecommendedPreset.CommandIds.Contains(id);
            Save();
        }

        private void ApplyAllPreset()
        {
            foreach (var box in _commandBoxes.Values)
                box.IsChecked = true;
            Save();
        }

        private void UpdatePresetButton()
        {
            PresetToggleBtn.Content = GetPresetState() switch
            {
                PresetState.Recommended => "recommended",
                PresetState.All => "all",
                _ => "custom"
            };
        }

        private PresetState GetPresetState()
        {
            if (IsRecommendedSelection())
                return PresetState.Recommended;
            if (IsAllSelection())
                return PresetState.All;
            return PresetState.Custom;
        }

        private bool IsRecommendedSelection()
        {
            foreach (var (id, box) in _commandBoxes)
            {
                if ((box.IsChecked == true) != RecommendedPreset.CommandIds.Contains(id))
                    return false;
            }
            return true;
        }

        private static bool AllBoxesChecked(IEnumerable<CheckBox> boxes)
        {
            foreach (var box in boxes)
            {
                if (box.IsChecked != true)
                    return false;
            }
            return true;
        }

        private bool IsAllSelection() => AllBoxesChecked(_commandBoxes.Values);

        private void Save()
        {
            _settings.ShareXPath = NormalizePath(ShareXPathBox.Text);
            ShareXPathBox.Text = _settings.ShareXPath;

            _settings.DisabledCommands.Clear();
            foreach (var (id, box) in _commandBoxes)
            {
                if (box.IsChecked != true)
                    _settings.DisabledCommands.Add(id);
            }

            UpdatePresetButton();
            _context.API.SaveSettingJsonStorage<Settings>();
        }

        private static string NormalizePath(string path)
        {
            var clean = (path ?? string.Empty).Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(clean))
                return "C:\\Program Files\\ShareX\\";

            if (!clean.EndsWith("\\", StringComparison.Ordinal))
                clean += "\\";
            return clean;
        }

        public void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            if (Parent is not UIElement parent) return;
            var args = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = this
            };
            parent.RaiseEvent(args);
        }

        public void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }
    }
}
