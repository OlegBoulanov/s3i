using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace s3iLib
{
    public static class Win32Helper
    {
        public static string ErrorMessage(uint code) { return ErrorMessage((int)code); }
        public static string ErrorMessage(int code)
        {
            return new Win32Exception(code).Message;
        }
        public static string ErrorMessage()
        {
            return ErrorMessage(Marshal.GetLastWin32Error());
        }
    }
}
