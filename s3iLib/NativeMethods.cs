using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;
using Comtypes = System.Runtime.InteropServices.ComTypes;

namespace s3iLib
{
// https://docs.microsoft.com/en-us/windows/win32/msi/summary-information-stream-property-set
/*
Property name	Property ID	PID	Type
Codepage	        PID_CODEPAGE	1	VT_I2
Title	            PID_TITLE	    2	VT_LPSTR
Subject	            PID_SUBJECT	    3	VT_LPSTR
Author	            PID_AUTHOR	    4	VT_LPSTR
Keywords	        PID_KEYWORDS	5	VT_LPSTR
Comments	        PID_COMMENTS	6	VT_LPSTR
Template	        PID_TEMPLATE	7	VT_LPSTR
Last Saved By	    PID_LASTAUTHOR	8	VT_LPSTR
Revision Number	    PID_REVNUMBER	9	VT_LPSTR
Last Printed	    PID_LASTPRINTED	11	VT_FILETIME
Create Time/Date	PID_CREATE_DTM	12	VT_FILETIME
Last Save Time/Date	PID_LASTSAVE_DTM	13	VT_FILETIME
Page Count	        PID_PAGECOUNT	14	VT_I4
Word Count	        PID_WORDCOUNT	15	VT_I4
Character Count	    PID_CHARCOUNT	16	VT_I4
Creating Application	PID_APPNAME	18	VT_LPSTR
Security	        PID_SECURITY	19	VT_I4
*/
    internal static class NativeMethods
    {
        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        internal static extern uint MsiOpenDatabase(string szDatabasePath, IntPtr szPersist, out IntPtr msiHandle);
        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        internal static extern uint MsiGetSummaryInformation(IntPtr msiHandle, string szDatabasePath, uint uiUpdateCount, out IntPtr SummaryInfoHandle);
        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        internal static extern uint MsiSummaryInfoGetProperty(IntPtr msiHandle, uint uiProperty, out uint uiDataType, out int iValue, out Comtypes.FILETIME ftValue, IntPtr szBuffer, ref uint cchBuf);
        [DllImport("msi.dll")]
        internal static extern uint MsiCloseHandle(IntPtr msiHandle);
    }
}
