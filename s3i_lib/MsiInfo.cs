using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Comtypes = System.Runtime.InteropServices.ComTypes;


namespace s3i_lib
{
    public class MsiInfo : IDisposable
    {
        //
        // Adopted from here: https://social.msdn.microsoft.com/Forums/windows/en-US/678f0c0e-eed3-4b13-a420-ee94802d8330/reading-msi-file-properties?forum=winformssetup
        //
        /*
        public static void GetMsiInfo(string msiFilePath)
        {
            IntPtr hMsi = IntPtr.Zero, hSum = IntPtr.Zero;
            IntPtr MSIDBOPEN_READONLY = (IntPtr)0;
            const uint ERROR_MORE_DATA = 234;
            const uint PID_TEMPLATE = 7;

            uint ret = NativeMethods.MsiOpenDatabase(msiFilePath, MSIDBOPEN_READONLY, out hMsi);
            if (ret == 0)
            {
                ret = NativeMethods.MsiGetSummaryInformation(hMsi, string.Empty, 0, out hSum);

                if (ret == 0)
                {
                    uint uiData = 0;
                    int iValue = 0;
                    Comtypes.FILETIME ftValue;
                    IntPtr strEmpty = Marshal.StringToHGlobalUni(string.Empty);
                    uint uicch = 0;

                    if (NativeMethods.MsiSummaryInfoGetProperty(hSum, PID_TEMPLATE, out uiData, out iValue, out ftValue, strEmpty, ref uicch) == ERROR_MORE_DATA)
                    {
                        uicch = uicch + 1;

                        IntPtr strBuffer = Marshal.AllocHGlobal((int)uicch * 2);

                        ret = NativeMethods.MsiSummaryInfoGetProperty(hSum, PID_TEMPLATE, out uiData, out iValue, out ftValue, strBuffer, ref uicch);

                        if (ret == 0)
                        {
                            string strProperty = Marshal.PtrToStringUni(strBuffer);
                            Console.WriteLine("Property is {0}", strProperty);
                        }

                        Marshal.FreeHGlobal(strBuffer);
                    }

                    Marshal.FreeHGlobal(strEmpty);
                }

                NativeMethods.MsiCloseHandle(hSum);
            }

            NativeMethods.MsiCloseHandle(hMsi);
        }
        */
        /// <summary>
        /// Msi installer property access
        /// </summary>
        /// 

        protected IntPtr hMsi = IntPtr.Zero;
        protected IntPtr hSummary = IntPtr.Zero;

        public uint Open(string msiFilePath)
        {
            IntPtr MSIDBOPEN_READONLY = (IntPtr)0;
            uint ret = NativeMethods.MsiOpenDatabase(msiFilePath, MSIDBOPEN_READONLY, out hMsi);
            if (ret == 0)
            {
                ret = NativeMethods.MsiGetSummaryInformation(hMsi, string.Empty, 0, out hSummary);
            }
            return ret;
        }

        public void Close()
        {
            IDisposable_FreeUnmanagedResources();
        }

        public string GetProperty(uint uiProperty, string defaultValue = null)
        {
            uint uiData;
            int iValue;
            Comtypes.FILETIME ftValue;
            IntPtr strEmpty = Marshal.StringToHGlobalUni(string.Empty);
            uint uicch = 0;
            const uint ERROR_MORE_DATA = 234;
            string propertyValue = defaultValue;
            if (NativeMethods.MsiSummaryInfoGetProperty(hSummary, uiProperty, out uiData, out iValue, out ftValue, strEmpty, ref uicch) == ERROR_MORE_DATA)
            {
                IntPtr strBuffer = Marshal.AllocHGlobal((int)uicch++ * 2);
                uint ret = NativeMethods.MsiSummaryInfoGetProperty(hSummary, uiProperty, out uiData, out iValue, out ftValue, strBuffer, ref uicch);
                if (ret == 0) propertyValue = Marshal.PtrToStringUni(strBuffer);
                Marshal.FreeHGlobal(strBuffer);
            }
            Marshal.FreeHGlobal(strEmpty);
            return propertyValue;
        }

        void IDisposable_FreeUnmanagedResources()
        {
            if (IntPtr.Zero != hSummary) { NativeMethods.MsiCloseHandle(hSummary); hSummary = IntPtr.Zero; }
            if (IntPtr.Zero != hMsi) { NativeMethods.MsiCloseHandle(hMsi); hMsi = IntPtr.Zero; }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                //**********************************************
                IDisposable_FreeUnmanagedResources(); //********
                //**********************************************
                // TODO: set large fields to null.
                disposedValue = true;
            }
        }
        //TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~MsiInfo()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }
        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
