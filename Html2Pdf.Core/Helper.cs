using System;
using System.Collections;
using System.IO;
using Org.Mentalis.Files;

namespace Html2Pdf.Core
{
    public class Helper
    {
        public static Hashtable GetFirefoxProfiles() {
            Hashtable hashtable = new Hashtable();
            string str = Environment.GetEnvironmentVariable("appdata") + "\\Mozilla\\Firefox\\";
            string file = str + "profiles.ini";
            IniReader iniReader = new IniReader(file);
            IEnumerator enumerator = iniReader.GetSectionNames().GetEnumerator();
            while (enumerator.MoveNext()) {
                string text = enumerator.Current.ToString();
                if (text.StartsWith("Profile")) {
                    string key = iniReader.ReadString(text, "Name");
                    string text2 = iniReader.ReadString(text, "Path");
                    text2 = text2.Replace('/', '\\');
                    int num = iniReader.ReadInteger(text, "IsRelative");
                    if (num == 1) {
                        text2 = str + text2;
                    }
                    string prefsfile = text2 + "\\prefs.js";
                    string scrapbookDir = GetScrapbookDir(text2, prefsfile);
                    hashtable.Add(key, scrapbookDir);
                }
            }
            return hashtable;
        }
        public static string GetScrapbookDir(string path, string prefsfile) {
            FileStream fileStream = new FileStream(prefsfile, FileMode.Open);
            StreamReader streamReader = new StreamReader(fileStream);
            bool flag = false;
            string text;
            while ((text = streamReader.ReadLine()) != null) {
                text.Trim();
                int num = text.IndexOf("\"scrapbook.data.default\"");
                if (num >= 0) {
                    string text2 = text.Substring(num + 26, text.Length - num - 26);
                    int num2 = text2.LastIndexOf(')');
                    text2 = text2.Substring(0, num2);
                    if (text2 == "true") {
                        flag = true;
                    }
                }
            }
            if (flag) {
                string text3 = path + "\\Scrapbook";
                streamReader.Close();
                return text3;
            }
            fileStream.Seek(0L, SeekOrigin.Begin);
            while ((text = streamReader.ReadLine()) != null) {
                text.Trim();
                int num = text.IndexOf("\"scrapbook.data.path\"");
                if (num >= 0) {
                    string text3 = text.Substring(num + 21, text.Length - num - 21);
                    int num3 = text3.IndexOf('"');
                    int num2 = text3.LastIndexOf('"');
                    text3 = text3.Substring(num3 + 1, num2 - num3 - 1);
                    text3 = text3.Replace("\\\\", "\\");
                    streamReader.Close();
                    return text3;
                }
            }
            streamReader.Close();
            return "";
        }

    }
}