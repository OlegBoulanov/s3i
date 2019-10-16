using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Text.RegularExpressions;

namespace s3iLib
{
    public static class IniReader
    {
        static readonly Regex rexSectionName = new Regex(@"^\s*\[([^\]]+)\]\s*$", RegexOptions.Compiled);
        public static async Task Read(Stream stream, Action<string, string, string> onNewKeyValue)
        {
            using (var reader = new StreamReader(stream))
            {
                for (string sectionName = null, line; null != (line = await reader.ReadLineAsync());)
                {
                    line = line.Split(';')[0];
                    var m = rexSectionName.Match(line);
                    if (m.Success && 1 < m.Groups.Count)
                    {
                        sectionName = m.Groups[1].Value.Trim();
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(sectionName)) continue;
                    string keyName = null, keyValue = null;
                    for (var pe = line.IndexOf('='); 0 <= pe;)
                    {
                        keyName = line.Substring(0, pe).Trim();
                        keyValue = line.Substring(pe + 1).Trim();
                        break;
                    }
                    if (string.IsNullOrWhiteSpace(keyName)) continue;
                    onNewKeyValue.Invoke(sectionName, keyName, keyValue);
                }
            }
        }
    }
}
