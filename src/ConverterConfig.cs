namespace JsonToPngConverter
{
    public class ConverterConfig
    {
        public string InputPath { get; set; } = string.Empty;
        public string OutputPath { get; set; } = string.Empty;
        public bool DownloadMode { get; set; } = false;
        public int Concurrency { get; set; } = 5;
        public int Width { get; set; } = 1920;
        public int Height { get; set; } = 1080;
        public double Scale { get; set; } = 2.0;
        public bool FullPage { get; set; } = true;
        public int Timeout { get; set; } = 30000;
        
        // Legacy properties for backward compatibility
        public string InputJsonPath
        {
            get => InputPath;
            set => InputPath = value;
        }
        
        public string OutputDirectory
        {
            get => OutputPath;
            set => OutputPath = value;
        }
        
        public int MaxConcurrency => Concurrency;
        public double DeviceScaleFactor => Scale;
        public int RetryCount { get; set; } = 3;
        public int RetryDelay { get; set; } = 1000;
    }
}
