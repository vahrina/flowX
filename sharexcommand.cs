namespace flowx
{
    public class SharexCommand
    {
        public enum Cat { Upload, ScreenCapture, ScreenRecord, Tools, Other }

        public string Id { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Command { get; set; }
        public Cat Category { get; set; }
        public string IcoPath { get; set; }
    }
}
