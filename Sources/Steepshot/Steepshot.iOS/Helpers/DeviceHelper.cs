using System;
using System.Runtime.InteropServices;
using UIKit;

namespace Steepshot.iOS.Helpers
{
    public static class DeviceHelper
    {
        private const string HardwareProperty = "hw.machine";

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
        static internal extern int sysctlbyname([MarshalAs(UnmanagedType.LPStr)] string property, IntPtr output, IntPtr oldLen, IntPtr newp, uint newlen);

        public static HardwareVersion GetVersion()
        {
            var pLen = Marshal.AllocHGlobal(sizeof(int));
            sysctlbyname(HardwareProperty, IntPtr.Zero, pLen, IntPtr.Zero, 0);

            var length = Marshal.ReadInt32(pLen);

            var pStr = Marshal.AllocHGlobal(length);
            sysctlbyname(HardwareProperty, pStr, pLen, IntPtr.Zero, 0);

            var hardwareStr = Marshal.PtrToStringAnsi(pStr);

            var ret = HardwareVersion.Unknown;

            switch (hardwareStr)
            {
                case "iPhone1,1":
                    ret = HardwareVersion.iPhone;
                    break;
                case "iPhone1,2":
                    ret = HardwareVersion.iPhone3G;
                    break;
                case "iPhone2,1":
                    ret = HardwareVersion.iPhone3GS;
                    break;
                case "iPhone3,1":
                case "iPhone3,2":
                case "iPhone3,3":
                    ret = HardwareVersion.iPhone4;
                    break;
                case "iPhone4,1":
                    ret = HardwareVersion.iPhone4S;
                    break;
                case "iPhone5,1":
                case "iPhone5,2":
                    ret = HardwareVersion.iPhone5;
                    break;
                case "iPhone5,3":
                case "iPhone5,4":
                    ret = HardwareVersion.iPhone5C;
                    break;
                case "iPhone6,1":
                case "iPhone6,2":
                    ret = HardwareVersion.iPhone5S;
                    break;
                case "iPhone7,2":
                    ret = HardwareVersion.iPhone6;
                    break;
                case "iPhone7,1":
                    ret = HardwareVersion.iPhone6Plus;
                    break;
                case "iPhone8,1":
                    ret = HardwareVersion.iPhone6S;
                    break;
                case "iPhone8,2":
                    ret = HardwareVersion.iPhone6SPlus;
                    break;
                case "iPhone8,4":
                    ret = HardwareVersion.iPhoneSE;
                    break;
                case "iPhone9,1":
                case "iPhone9,3":
                    ret = HardwareVersion.iPhone7;
                    break;
                case "iPhone9,2":
                case "iPhone9,4":
                    ret = HardwareVersion.iPhone7Plus;
                    break;
                case "iPhone10,1":
                case "iPhone10,4":
                    ret = HardwareVersion.iPhone8;
                    break;
                case "iPhone10,2":
                case "iPhone10,5":
                    ret = HardwareVersion.iPhone8Plus;
                    break;
                case "iPhone10,3":
                case "iPhone10,6":
                    ret = HardwareVersion.iPhoneX;
                    break;
                case "iPhone11,8":
                    ret = HardwareVersion.iPhoneXR;
                    break;
                case "iPhone11,2":
                    ret = HardwareVersion.iPhoneXS;
                    break;
                case "iPhone11,4":
                    ret = HardwareVersion.iPhoneXSMax;
                    break;
                case "iPad1,1":
                    ret = HardwareVersion.iPad;
                    break;
                case "iPad2,1":
                case "iPad2,2":
                case "iPad2,3":
                case "iPad2,4":
                    ret = HardwareVersion.iPad2;
                    break;
                case "iPad3,1":
                case "iPad3,2":
                case "iPad3,3":
                    ret = HardwareVersion.iPad3;
                    break;
                case "iPad3,4":
                case "iPad3,5":
                case "iPad3,6":
                    ret = HardwareVersion.iPad4;
                    break;
                case "iPad4,1":
                case "iPad4,2":
                case "iPad4,3":
                    ret = HardwareVersion.iPadAir;
                    break;
                case "iPad5,3":
                case "iPad5,4":
                    ret = HardwareVersion.iPadAir2;
                    break;
                case "iPad6,7":
                case "iPad6,8":
                    ret = HardwareVersion.iPadPro;
                    break;
                case "iPad4,4":
                case "iPad4,5":
                case "iPad4,6":
                    ret = HardwareVersion.iPadMini2;
                    break;
                case "iPad4,7":
                case "iPad4,8":
                case "iPad4,9":
                    ret = HardwareVersion.iPadMini3;
                    break;
                case "iPad5,1":
                case "iPad5,2":
                    ret = HardwareVersion.iPadMini4;
                    break;
                case "iPod1,1":
                    ret = HardwareVersion.iPod1G;
                    break;
                case "iPod2,1":
                    ret = HardwareVersion.iPod2G;
                    break;
                case "iPod3,1":
                    ret = HardwareVersion.iPod3G;
                    break;
                case "iPod4,1":
                    ret = HardwareVersion.iPod4G;
                    break;
                case "iPod5,1":
                    ret = HardwareVersion.iPod5G;
                    break;
                case "iPod7,1":
                    ret = HardwareVersion.iPod6G;
                    break;
                case "i386":
                case "x86_64":
                    if (UIDevice.CurrentDevice.Model.Contains("iPhone"))
                        ret = HardwareVersion.iPhoneSimulator;
                    else
                        ret = HardwareVersion.iPadSimulator;
                    break;
            }
            return ret;
        }

        public static bool IsSmallDevice
        {
            get
            {
                var currentVersion = GetVersion();
                return currentVersion == HardwareVersion.iPhone5 ||
                       currentVersion == HardwareVersion.iPhone5S ||
                       currentVersion == HardwareVersion.iPhone5C ||
                       currentVersion == HardwareVersion.iPhoneSE;
            }
        }

        public static bool IsPlusDevice
        {
            get
            {
                var currentVersion = GetVersion();
                return currentVersion == HardwareVersion.iPhone6Plus ||
                       currentVersion == HardwareVersion.iPhone6SPlus ||
                       currentVersion == HardwareVersion.iPhone7Plus ||
                       currentVersion == HardwareVersion.iPhone8Plus;
            }
        }

        public enum HardwareVersion
        {
            iPhone,
            iPhone3G,
            iPhone3GS,
            iPhone4,
            iPhone4S,
            iPhone5,
            iPhone5C,
            iPhone5S,
            iPhone6,
            iPhone6Plus,
            iPhone6S,
            iPhone6SPlus,
            iPhoneSE,
            iPhone7,
            iPhone7Plus,
            iPhone8,
            iPhone8Plus,
            iPhoneX,
            iPhoneXR,
            iPhoneXS,
            iPhoneXSMax,
            iPod1G,
            iPod2G,
            iPod3G,
            iPod4G,
            iPod5G,
            iPod6G,
            iPad,
            iPad2,
            iPad3,
            iPad4,
            iPadAir,
            iPadAir2,
            iPadPro,
            iPadMini,
            iPadMini2,
            iPadMini3,
            iPadMini4,
            iPhoneSimulator,
            iPhone4Simulator,
            iPadSimulator,
            Unknown
        }
    }
}
