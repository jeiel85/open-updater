namespace AutoUpdate.Shared
{
    /// <summary>
    /// 로그 기록 유틸리티
    /// </summary>
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static string _logFolder = string.Empty;

        /// <summary>
        /// 로그 폴더 경로 설정
        /// </summary>
        public static string LogFolder
        {
            set => _logFolder = value;
        }

        /// <summary>
        /// 로그 기록
        /// </summary>
        public static void WriteLog(string folderName, string className, string methodName, string category, string message)
        {
            try
            {
                string logPath = Path.Combine(_logFolder ?? Path.GetDirectoryName(Environment.ProcessPath) ?? ".", "Log");
                string subFolder = Path.Combine(logPath, folderName);

                if (!Directory.Exists(subFolder))
                    Directory.CreateDirectory(subFolder);

                string fileName = Path.Combine(subFolder, $"{folderName}_{DateTime.Now:yyyyMMdd}.txt");
                string logText = $"[{DateTime.Now:HH:mm:ss}] {className}■{methodName}\t{category}■{message}";

                lock (_lock)
                {
                    File.AppendAllText(fileName, Environment.NewLine + logText);
                }
            }
            catch
            {
                // 로그 실패 시 무시
            }
        }

        /// <summary>
        /// 간단한 에러 로그
        /// </summary>
        public static void Error(string source, Exception ex)
        {
            WriteLog("AUTOUPDATE", source, "ERROR", "EXCEPTION", ex.Message);
        }
    }
}