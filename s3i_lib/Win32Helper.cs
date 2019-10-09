using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace s3i_lib
{
    public class Win32Helper
    {
        public static string ErrorMessage(int code)
        {
            var msg = new Win32Exception(code).Message;
            // NetStandard: adds a dot in the end
            // NetFramework: does not
            return msg.EndsWith(".") ? msg.Substring(0, msg.Length - 1) : msg;
        }
        public static string ErrorMessage()
        {
            return ErrorMessage(Marshal.GetLastWin32Error());
        }
    }
}
