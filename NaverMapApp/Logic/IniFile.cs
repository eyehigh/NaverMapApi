using System.Runtime.InteropServices;
using System.Text;

namespace NaverMapApp.Logic
{
    internal class IniFile
    {
        // INI 관련
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        private string Path;
        private string FileName;

        public IniFile()
        {
            // 현재 프로그램 실행 위치
            Path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            FileName = "Config.ini";
            Path = System.IO.Path.GetDirectoryName(Path) + "\\" + FileName;
        }
        public string Read(string section, string key)
        {
            StringBuilder retVal = new StringBuilder();
            int result1 = GetPrivateProfileString(section, key, "N/A", retVal, 300, Path); // key 있음
            return retVal.ToString();
        }
    }
}
