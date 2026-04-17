using Flow.Launcher.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;

namespace flowx
{
    public class main : IPlugin, ISettingProvider
    {
        private const string DefaultShareXPath = "C:\\Program Files\\ShareX\\";

        private PluginInitContext _context;
        private readonly string _sharexExe = "ShareX.exe";
        private List<SharexCommand> _sharexCommands;
        private Settings _settings;

        public void Init(PluginInitContext context)
        {
            _context = context;
            var loaded = _context.API.LoadSettingJsonStorage<Settings>();
            var isNewSettings = loaded == null;
            _settings = loaded ?? new Settings();
            _settings.ShareXPath = NormalizeShareXPath(_settings.ShareXPath);

            _sharexCommands = new List<SharexCommand>
            {
                // upload
                new SharexCommand { Id = "FileUpload",                        Title = "upload file",                                                                Command = "-FileUpload",                        Category = SharexCommand.Cat.Upload,        IcoPath = "" },
                new SharexCommand { Id = "FolderUpload",                      Title = "upload folder",                                                              Command = "-FolderUpload",                      Category = SharexCommand.Cat.Upload,        IcoPath = "" },
                new SharexCommand { Id = "ClipboardUpload",                   Title = "upload clipboard",                                                           Command = "-ClipboardUpload",                   Category = SharexCommand.Cat.Upload,        IcoPath = "" },
                new SharexCommand { Id = "ClipboardUploadWithContentViewer",  Title = "upload clipboard + viewer",                                                  Command = "-ClipboardUploadWithContentViewer",  Category = SharexCommand.Cat.Upload,        IcoPath = "" },
                new SharexCommand { Id = "UploadText",                        Title = "upload text",                                                                Command = "-UploadText",                        Category = SharexCommand.Cat.Upload,        IcoPath = "" },
                new SharexCommand { Id = "UploadURL",                         Title = "upload url",                                                                 Command = "-UploadURL",                         Category = SharexCommand.Cat.Upload,        IcoPath = "" },
                new SharexCommand { Id = "DragDropUpload",                    Title = "drag & drop",           SubTitle = "drag and drop files to upload",          Command = "-DragDropUpload",                    Category = SharexCommand.Cat.Upload,        IcoPath = "" },
                new SharexCommand { Id = "ShortenURL",                        Title = "shorten url",                                                                Command = "-ShortenURL",                        Category = SharexCommand.Cat.Upload,        IcoPath = "" },
                new SharexCommand { Id = "TweetMessage",                      Title = "tweet",                 SubTitle = "post a tweet via sharex",                Command = "-TweetMessage",                      Category = SharexCommand.Cat.Upload,        IcoPath = "" },
                new SharexCommand { Id = "StopUploads",                       Title = "stop uploads",          SubTitle = "cancel all active uploads",              Command = "-StopUploads",                       Category = SharexCommand.Cat.Upload,        IcoPath = "" },

                // screen capture
                new SharexCommand { Id = "PrintScreen",                       Title = "fullscreen",            SubTitle = "capture entire screen",                  Command = "-PrintScreen",                       Category = SharexCommand.Cat.ScreenCapture, IcoPath = "" },
                new SharexCommand { Id = "ActiveWindow",                      Title = "active window",                                                              Command = "-ActiveWindow",                      Category = SharexCommand.Cat.ScreenCapture, IcoPath = "" },
                new SharexCommand { Id = "ActiveMonitor",                     Title = "active monitor",                                                             Command = "-ActiveMonitor",                     Category = SharexCommand.Cat.ScreenCapture, IcoPath = "" },
                new SharexCommand { Id = "RectangleRegion",                   Title = "region",                SubTitle = "select & capture a region",              Command = "-RectangleRegion",                   Category = SharexCommand.Cat.ScreenCapture, IcoPath = "" },
                new SharexCommand { Id = "RectangleLight",                    Title = "region (light)",                                                             Command = "-RectangleLight",                    Category = SharexCommand.Cat.ScreenCapture, IcoPath = "" },
                new SharexCommand { Id = "RectangleTransparent",              Title = "region (transparent)",                                                       Command = "-RectangleTransparent",              Category = SharexCommand.Cat.ScreenCapture, IcoPath = "" },
                new SharexCommand { Id = "CustomRegion",                      Title = "custom region",         SubTitle = "capture a preset region",                Command = "-CustomRegion",                      Category = SharexCommand.Cat.ScreenCapture, IcoPath = "" },
                new SharexCommand { Id = "LastRegion",                        Title = "last region",           SubTitle = "repeat last capture region",             Command = "-LastRegion",                        Category = SharexCommand.Cat.ScreenCapture, IcoPath = "" },
                new SharexCommand { Id = "ScrollingCapture",                  Title = "scrolling",             SubTitle = "capture a scrolling page",               Command = "-ScrollingCapture",                  Category = SharexCommand.Cat.ScreenCapture, IcoPath = "" },
                new SharexCommand { Id = "AutoCapture",                       Title = "auto capture",          SubTitle = "timed auto capture",                     Command = "-AutoCapture",                       Category = SharexCommand.Cat.ScreenCapture, IcoPath = "" },
                new SharexCommand { Id = "StartAutoCapture",                  Title = "auto capture (last)",   SubTitle = "auto capture using last region",         Command = "-StartAutoCapture",                  Category = SharexCommand.Cat.ScreenCapture, IcoPath = "" },
                new SharexCommand { Id = "ShowCursor",                        Title = "show cursor",                                                                Command = "-ShowCursor",                        Category = SharexCommand.Cat.ScreenCapture, IcoPath = "" },
                new SharexCommand { Id = "HideCursor",                        Title = "hide cursor",                                                                Command = "-HideCursor",                        Category = SharexCommand.Cat.ScreenCapture, IcoPath = "" },

                // screen record
                new SharexCommand { Id = "ScreenRecorder",                    Title = "record",                SubTitle = "start/stop a recording",                 Command = "-ScreenRecorder",                    Category = SharexCommand.Cat.ScreenRecord,  IcoPath = "" },
                new SharexCommand { Id = "ScreenRecorderActiveWindow",        Title = "record window",         SubTitle = "record active window",                   Command = "-ScreenRecorderActiveWindow",        Category = SharexCommand.Cat.ScreenRecord,  IcoPath = "" },
                new SharexCommand { Id = "ScreenRecorderCustomRegion",        Title = "record region",         SubTitle = "record a custom region",                 Command = "-ScreenRecorderCustomRegion",        Category = SharexCommand.Cat.ScreenRecord,  IcoPath = "" },
                new SharexCommand { Id = "StartScreenRecorder",               Title = "record (last)",         SubTitle = "record using last region",               Command = "-StartScreenRecorder",               Category = SharexCommand.Cat.ScreenRecord,  IcoPath = "" },
                new SharexCommand { Id = "ScreenRecorderGIF",                 Title = "gif",                   SubTitle = "start/stop a recording (gif)",           Command = "-ScreenRecorderGIF",                 Category = SharexCommand.Cat.ScreenRecord,  IcoPath = "" },
                new SharexCommand { Id = "ScreenRecorderGIFActiveWindow",     Title = "gif (window)",          SubTitle = "record gif of active window",            Command = "-ScreenRecorderGIFActiveWindow",     Category = SharexCommand.Cat.ScreenRecord,  IcoPath = "" },
                new SharexCommand { Id = "ScreenRecorderGIFCustomRegion",     Title = "gif (region)",          SubTitle = "record gif of custom region",            Command = "-ScreenRecorderGIFCustomRegion",     Category = SharexCommand.Cat.ScreenRecord,  IcoPath = "" },
                new SharexCommand { Id = "StartScreenRecorderGIF",            Title = "gif (last)",            SubTitle = "record gif of last region",              Command = "-StartScreenRecorderGIF",            Category = SharexCommand.Cat.ScreenRecord,  IcoPath = "" },
                new SharexCommand { Id = "StopScreenRecording",               Title = "stop recording",                                                             Command = "-StopScreenRecording",               Category = SharexCommand.Cat.ScreenRecord,  IcoPath = "" },
                new SharexCommand { Id = "AbortScreenRecording",              Title = "abort recording",                                                            Command = "-AbortScreenRecording",              Category = SharexCommand.Cat.ScreenRecord,  IcoPath = "" },

                // tools
                new SharexCommand { Id = "ColorPicker",                       Title = "color picker",          SubTitle = "open color picker window",               Command = "-ColorPicker",                       Category = SharexCommand.Cat.Tools,         IcoPath = "" },
                new SharexCommand { Id = "ScreenColorPicker",                 Title = "screen color picker",   SubTitle = "get color from screen",                  Command = "-ScreenColorPicker",                 Category = SharexCommand.Cat.Tools,         IcoPath = "" },
                new SharexCommand { Id = "Ruler",                             Title = "ruler",                                                                      Command = "-Ruler",                             Category = SharexCommand.Cat.Tools,         IcoPath = "" },
                new SharexCommand { Id = "ImageEditor",                       Title = "image editor",                                                               Command = "-ImageEditor",                       Category = SharexCommand.Cat.Tools,         IcoPath = "" },
                new SharexCommand { Id = "ImageEffects",                      Title = "image effects",         SubTitle = "apply effects to an image",              Command = "-ImageEffects",                      Category = SharexCommand.Cat.Tools,         IcoPath = "" },
                new SharexCommand { Id = "ImageViewer",                       Title = "image viewer",                                                               Command = "-ImageViewer",                            Category = SharexCommand.Cat.Tools,         IcoPath = "" },
                new SharexCommand { Id = "ImageCombiner",                     Title = "image combiner",        SubTitle = "combine multiple images",                Command = "-ImageCombiner",                     Category = SharexCommand.Cat.Tools,         IcoPath = "" },
                new SharexCommand { Id = "ImageSplitter",                     Title = "image splitter",        SubTitle = "split an image into parts",              Command = "-ImageSplitter",                     Category = SharexCommand.Cat.Tools,         IcoPath = "" },
                new SharexCommand { Id = "ImageThumbnailer",                  Title = "image thumbnails",      SubTitle = "generate image thumbnails",              Command = "-ImageThumbnailer",                  Category = SharexCommand.Cat.Tools,         IcoPath = "" },
                new SharexCommand { Id = "VideoConverter",                    Title = "video converter",       SubTitle = "convert a video file",                   Command = "-VideoConverter",                    Category = SharexCommand.Cat.Tools,         IcoPath = "" },
                new SharexCommand { Id = "VideoThumbnailer",                  Title = "video thumbnails",      SubTitle = "generate video thumbnails",              Command = "-VideoThumbnailer",                  Category = SharexCommand.Cat.Tools,         IcoPath = "" },
                new SharexCommand { Id = "OCR",                               Title = "ocr",                   SubTitle = "capture text from screen",               Command = "-OCR",                               Category = SharexCommand.Cat.Tools,         IcoPath = "" },
                new SharexCommand { Id = "QRCode",                            Title = "qr code",                                                                    Command = "-QRCode",                                 Category = SharexCommand.Cat.Tools,         IcoPath = "" },
                new SharexCommand { Id = "QRCodeDecodeFromScreen",            Title = "qr decode",                                                                  Command = "-QRCodeDecodeFromScreen",                 Category = SharexCommand.Cat.Tools,         IcoPath = "" },
                new SharexCommand { Id = "HashCheck",                         Title = "hash check",            SubTitle = "verify a file's hash",                   Command = "-HashCheck",                         Category = SharexCommand.Cat.Tools,         IcoPath = "" },
                new SharexCommand { Id = "IndexFolder",                       Title = "index folder",          SubTitle = "generate a folder index",                Command = "-IndexFolder",                       Category = SharexCommand.Cat.Tools,         IcoPath = "" },
                new SharexCommand { Id = "ClipboardViewer",                   Title = "clipboard viewer",      SubTitle = "view clipboard contents",                Command = "-ClipboardViewer",                   Category = SharexCommand.Cat.Tools,         IcoPath = "" },
                new SharexCommand { Id = "BorderlessWindow",                  Title = "borderless window",     SubTitle = "remove window borders",                  Command = "-BorderlessWindow",                  Category = SharexCommand.Cat.Tools,         IcoPath = "" },
                new SharexCommand { Id = "InspectWindow",                     Title = "inspect window",        SubTitle = "view window info and details",           Command = "-InspectWindow",                     Category = SharexCommand.Cat.Tools,         IcoPath = "" },
                new SharexCommand { Id = "MonitorTest",                       Title = "monitor test",          SubTitle = "display test patterns",                  Command = "-MonitorTest",                       Category = SharexCommand.Cat.Tools,         IcoPath = "" },
                new SharexCommand { Id = "DNSChanger",                        Title = "dns",                   SubTitle = "change dns settings",                    Command = "-DNSChanger",                        Category = SharexCommand.Cat.Tools,         IcoPath = "" },

                // other
                new SharexCommand { Id = "DisableHotkeys",                    Title = "toggle hotkeys",        SubTitle = "enable/disable global hotkeys",          Command = "-DisableHotkeys",                    Category = SharexCommand.Cat.Other,         IcoPath = "" },
                new SharexCommand { Id = "OpenMainWindow",                    Title = "open sharex",           SubTitle = "bring up sharex main window",            Command = "-OpenMainWindow",                    Category = SharexCommand.Cat.Other,         IcoPath = "" },
                new SharexCommand { Id = "OpenScreenshotsFolder",             Title = "open screenshots folder",                                                    Command = "-OpenScreenshotsFolder",             Category = SharexCommand.Cat.Other,         IcoPath = "" },
                new SharexCommand { Id = "OpenHistory",                       Title = "browse history",                                                             Command = "-OpenHistory",                       Category = SharexCommand.Cat.Other,         IcoPath = "" },
                new SharexCommand { Id = "OpenImageHistory",                  Title = "image history",                                                              Command = "-OpenImageHistory",                  Category = SharexCommand.Cat.Other,         IcoPath = "" },
                new SharexCommand { Id = "ToggleActionsToolbar",              Title = "toggle actions toolbar",                                                     Command = "-ToggleActionsToolbar",              Category = SharexCommand.Cat.Other,         IcoPath = "" },
                new SharexCommand { Id = "ToggleTrayMenu",                    Title = "toggle tray menu",                                                           Command = "-ToggleTrayMenu",                    Category = SharexCommand.Cat.Other,         IcoPath = "" },
                new SharexCommand { Id = "ExitShareX",                        Title = "exit sharex",                                                                Command = "-ExitShareX",                        Category = SharexCommand.Cat.Other,         IcoPath = "" },
            };

            if (isNewSettings)
            {
                _settings.DisabledCommands.Clear();
                foreach (var cmd in _sharexCommands)
                {
                    if (!RecommendedPreset.CommandIds.Contains(cmd.Id))
                        _settings.DisabledCommands.Add(cmd.Id);
                }
                _context.API.SaveSettingJsonStorage<Settings>();
            }
        }

        public Control CreateSettingPanel() => new ui.settingsview(_context, _settings, _sharexCommands);

        public List<Result> Query(Query query)
        {
            var results = new List<Result>();
            var term = query.Search ?? string.Empty;
            foreach (var command in _sharexCommands.Where(IsCommandEnabled).Where(c => c.Title.Contains(term, StringComparison.OrdinalIgnoreCase)))
            {
                results.Add(new Result
                {
                    Title = command.Title,
                    SubTitle = command.SubTitle,
                    IcoPath = string.IsNullOrEmpty(command.IcoPath) ? "icon.png" : command.IcoPath,
                    Action = e => RunCommand(e, command.Command)
                });
            }
            return results;
        }

        private bool RunCommand(ActionContext e, string cmd)
        {
            try
            {
                var startInfo = new ProcessStartInfo(_sharexExe, cmd)
                {
                    UseShellExecute = true,
                    WorkingDirectory = NormalizeShareXPath(_settings.ShareXPath)
                };
                Process.Start(startInfo);
            }
            catch (Win32Exception w32Ex)
            {
                if (w32Ex.Message != "the operation was canceled by the user")
                    throw;
            }
            catch (FormatException)
            {
                _context.API.ShowMsg("error: check arguments format");
            }
            return true;
        }

        private bool IsCommandEnabled(SharexCommand command)
            => command != null && !_settings.DisabledCommands.Contains(command.Id);

        private static string NormalizeShareXPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return DefaultShareXPath;

            var clean = path.Trim().Trim('"');
            if (!clean.EndsWith("\\", StringComparison.Ordinal))
                clean += "\\";
            return clean;
        }
    }
}
