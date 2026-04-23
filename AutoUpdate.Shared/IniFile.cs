using System.Runtime.InteropServices;
using System.Text;

namespace AutoUpdate.Shared
{
    /// <summary>
    /// INI 파일 읽기/쓰기 유틸리티
    /// </summary>
    public static class IniFile
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder returnValue, int size, string filePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string defaultValue, byte[] returnValue, int size, string filePath);

        /// <summary>
        /// 문자열 읽기
        /// </summary>
        public static string Read(string section, string key, string filePath, string defaultValue = "")
        {
            var sb = new StringBuilder(1024);
            GetPrivateProfileString(section, key, defaultValue, sb, sb.Capacity, filePath);
            return sb.ToString();
        }

        /// <summary>
        /// 문자열 배열 읽기 (구분자 포함)
        /// </summary>
        public static string[] ReadKeys(string section, string filePath)
        {
            byte[] b = new byte[65536];
            int size = GetPrivateProfileString(section, null!, "", b, b.Length, filePath);

            if (size <= 0) return Array.Empty<string>();

            var result = new List<string>();
            string current = "";

            for (int i = 0; i < size; i++)
            {
                if (b[i] == 0)
                {
                    if (!string.IsNullOrEmpty(current))
                        result.Add(current);
                    current = "";
                }
                else
                {
                    current += (char)b[i];
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 문자열 쓰기
        /// </summary>
        public static void Write(string section, string key, string value, string filePath)
        {
            WritePrivateProfileString(section, key, value, filePath);
        }

        /// <summary>
        /// 섹션 삭제
        /// </summary>
        public static void DeleteSection(string section, string filePath)
        {
            WritePrivateProfileString(section, null!, "", filePath);
        }

        /// <summary>
        /// 키 삭제
        /// </summary>
        public static void DeleteKey(string section, string key, string filePath)
        {
            WritePrivateProfileString(section, key, null!, filePath);
        }
    }

    /// <summary>
    /// 레지스트리 유틸리티
    /// </summary>
    public static class RegistryHelper
    {
        /// <summary>
        /// 레지스트리 값 읽기
        /// </summary>
        public static string? ReadValue(string keyPath, string valueName)
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(keyPath);
                return key?.GetValue(valueName)?.ToString();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 레지스트리 값 쓰기
        /// </summary>
        public static void WriteValue(string keyPath, string valueName, object value)
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(keyPath);
                key?.SetValue(valueName, value);
            }
            catch
            {
                // 실패 시 무시
            }
        }
    }
}