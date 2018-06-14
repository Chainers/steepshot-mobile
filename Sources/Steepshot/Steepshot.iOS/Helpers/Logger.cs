using System;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;

namespace Steepshot.iOS.Helpers
{
    public static class Logger
    {
        [DllImport(FoundationLibrary)]
        extern static void NSLog(IntPtr format, IntPtr s);

        [DllImport(FoundationLibrary, EntryPoint = "NSLog")]
        extern static void NSLog_ARM64(IntPtr format, IntPtr p2, IntPtr p3, IntPtr p4, IntPtr p5, IntPtr p6, IntPtr p7, IntPtr p8, IntPtr s);

        private const string FoundationLibrary = "/System/Library/Frameworks/Foundation.framework/Foundation";
        private static readonly bool Is64Bit = IntPtr.Size == 8;
        private static readonly bool IsDevice = Runtime.Arch == Arch.DEVICE;
        private static readonly NSString nsFormat = new NSString(@"%@");

        static void OutputStringToConsole(string text)
        {
            using (var nsText = new Foundation.NSString(text))
            {
                if (IsDevice && Is64Bit)
                {
                    NSLog_ARM64(nsFormat.Handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, nsText.Handle);
                }
                else
                {
                    NSLog(nsFormat.Handle, nsText.Handle);
                }
            }
        }
    }
}
