using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;
using Comtypes = System.Runtime.InteropServices.ComTypes;

namespace s3i_lib
{
    public static class NativeMethods
    {
        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        public static extern uint MsiOpenDatabase(string szDatabasePath, IntPtr szPersist, out IntPtr msiHandle);
        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        public static extern uint MsiGetSummaryInformation(IntPtr msiHandle, string szDatabasePath, uint uiUpdateCount, out IntPtr SummaryInfoHandle);
        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        public static extern uint MsiSummaryInfoGetProperty(IntPtr msiHandle, uint uiProperty, out uint uiDataType, out int iValue, out Comtypes.FILETIME ftValue, IntPtr szBuffer, ref uint cchBuf);
        [DllImport("msi.dll")]
        public static extern uint MsiCloseHandle(IntPtr msiHandle);
    }
}
