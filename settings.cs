using System.Collections.Generic;

namespace flowx
{
    public class Settings
    {
        public string ShareXPath { get; set; } = "C:\\Program Files\\ShareX\\";
        public HashSet<string> DisabledCommands { get; set; } = new HashSet<string>();
    }
}
