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
        // Loosely adopted from here: https://social.msdn.microsoft.com/Forums/windows/en-US/678f0c0e-eed3-4b13-a420-ee94802d8330/reading-msi-file-properties?forum=winformssetup
        //

        IntPtr hMsi = IntPtr.Zero;
        IntPtr hSummary = IntPtr.Zero;

        public enum StringPropertyType { 
            Title = 2, 
            Subject = 3, 
            Author = 4, 
            Keywords = 5, 
            Comments = 6, 
            Template = 7, 
            LastSavedBy = 8, 
            RevisionNumber = 9 
        };

        public uint ErrorCode { get; protected set; }
        public static implicit operator bool(MsiInfo info) { return IntPtr.Zero != info.hMsi && IntPtr.Zero != info.hSummary; }
        public MsiInfo(string msiFilePath)
        {
            Open(msiFilePath);
        }

        public uint Open(string msiFilePath)
        {
            IntPtr MSIDBOPEN_READONLY = (IntPtr)0;
            ErrorCode = NativeMethods.MsiOpenDatabase(msiFilePath, MSIDBOPEN_READONLY, out hMsi);
            if (ErrorCode == 0)
            {
                ErrorCode = NativeMethods.MsiGetSummaryInformation(hMsi, string.Empty, 0, out hSummary);
            }
            return ErrorCode;
        }

        public void Close()
        {
            IDisposable_FreeUnmanagedResources();
        }


        public string GetStringProperty(StringPropertyType propertType, string defaultValue)
        {
            IntPtr strEmpty = Marshal.StringToHGlobalUni(string.Empty);
            uint uicch = 0;
            const uint ERROR_MORE_DATA = 234;
            string propertyValue = defaultValue;
            if (NativeMethods.MsiSummaryInfoGetProperty(hSummary, (uint)propertType, out uint _, out _, out _, strEmpty, ref uicch) == ERROR_MORE_DATA)
            {
                IntPtr strBuffer = Marshal.AllocHGlobal((int)uicch++ * 2);
                ErrorCode = NativeMethods.MsiSummaryInfoGetProperty(hSummary, (uint)propertType, out _, out _, out _, strBuffer, ref uicch);
                if (ErrorCode == 0) propertyValue = Marshal.PtrToStringUni(strBuffer);
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
