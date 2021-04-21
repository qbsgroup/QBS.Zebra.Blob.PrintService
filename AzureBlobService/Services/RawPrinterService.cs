using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AzureBlobService.Services
{
    public class RawPrinterService
    {
        [
            DllImport("winspool.drv", CharSet = CharSet.Unicode, ExactSpelling = false,
                CallingConvention = CallingConvention.StdCall)]
        public static extern long OpenPrinter(string pPrinterName, ref IntPtr phPrinter, int pDefault);

        [
            DllImport("winspool.drv", CharSet = CharSet.Unicode, ExactSpelling = false,
                CallingConvention = CallingConvention.StdCall)]
        public static extern long StartDocPrinter(IntPtr hPrinter, int Level, ref DOCINFO pDocInfo);

        [
            DllImport("winspool.drv", CharSet = CharSet.Unicode, ExactSpelling = true,
                CallingConvention = CallingConvention.StdCall)]
        public static extern long StartPagePrinter(IntPtr hPrinter);

        [
            DllImport("winspool.drv", CharSet = CharSet.Ansi, ExactSpelling = true,
                CallingConvention = CallingConvention.StdCall)]
        public static extern long WritePrinter(IntPtr hPrinter, string data, int buf, ref int pcWritten);

        [
            DllImport("winspool.drv", CharSet = CharSet.Unicode, ExactSpelling = true,
                CallingConvention = CallingConvention.StdCall)]
        public static extern long EndPagePrinter(IntPtr hPrinter);

        [
            DllImport("winspool.drv", CharSet = CharSet.Unicode, ExactSpelling = true,
                CallingConvention = CallingConvention.StdCall)]
        public static extern long EndDocPrinter(IntPtr hPrinter);

        [
            DllImport("winspool.drv", CharSet = CharSet.Unicode, ExactSpelling = true,
                CallingConvention = CallingConvention.StdCall)]
        public static extern long ClosePrinter(IntPtr hPrinter);

        public static void SendToPrinter(string printerJobName, string rawStringToSendToThePrinter,
                                         string printerNameAsDescribedByPrintManager)
        {
            IntPtr handleForTheOpenPrinter = new IntPtr();
            DOCINFO documentInformation = new DOCINFO();
            int printerBytesWritten = 0;
            documentInformation.printerDocumentName = printerJobName;
            documentInformation.printerDocumentDataType = "RAW";
            OpenPrinter(printerNameAsDescribedByPrintManager, ref handleForTheOpenPrinter, 0);
            StartDocPrinter(handleForTheOpenPrinter, 1, ref documentInformation);
            StartPagePrinter(handleForTheOpenPrinter);
            WritePrinter(handleForTheOpenPrinter, rawStringToSendToThePrinter, rawStringToSendToThePrinter.Length,
                         ref printerBytesWritten);
            EndPagePrinter(handleForTheOpenPrinter);
            EndDocPrinter(handleForTheOpenPrinter);
            ClosePrinter(handleForTheOpenPrinter);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct DOCINFO
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string printerDocumentName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pOutputFile;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string printerDocumentDataType;
    }
}

