using System;
using System.Runtime.InteropServices;

namespace IntegraTech_POS.Platforms.Windows.Printing
{
    
    public static class RawPrinterHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class DOCINFO
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pDocName = string.Empty;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pOutputFile = string.Empty;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pDatatype = "RAW";
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterW", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern bool OpenPrinter(string src, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", SetLastError = true)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterW", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In] DOCINFO di);

        [DllImport("winspool.Drv", SetLastError = true)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        public static bool SendBytesToPrinter(string printerName, byte[] bytes)
        {
            IntPtr hPrinter = IntPtr.Zero;
            var di = new DOCINFO { pDocName = "POS Ticket", pDatatype = "RAW" };
            try
            {
                if (!OpenPrinter(printerName, out hPrinter, IntPtr.Zero))
                    return false;
                if (!StartDocPrinter(hPrinter, 1, di))
                    return false;
                if (!StartPagePrinter(hPrinter))
                    return false;

                IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(bytes.Length);
                Marshal.Copy(bytes, 0, pUnmanagedBytes, bytes.Length);
                bool success = WritePrinter(hPrinter, pUnmanagedBytes, bytes.Length, out _);
                Marshal.FreeCoTaskMem(pUnmanagedBytes);

                EndPagePrinter(hPrinter);
                EndDocPrinter(hPrinter);

                return success;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (hPrinter != IntPtr.Zero)
                    ClosePrinter(hPrinter);
            }
        }
    }
}

