using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

//Credit to: Matt Becker https://stackoverflow.com/questions/31043829/how-to-get-count-of-monitors-connected-to-the-pc-in-c
namespace bossdoyKaraoke_NOW
{
    public class ScreenInformation
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct ScreenRect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, MonitorEnumProc callback, int dwData);

        private delegate bool MonitorEnumProc(IntPtr hDesktop, IntPtr hdc, ref ScreenRect pRect, int dwData);

        public static int GetMonitorCount()
        {
            int monCount = 0;
            MonitorEnumProc callback = (IntPtr hDesktop, IntPtr hdc, ref ScreenRect prect, int d) => ++monCount > 0;

            if (EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, 0))
                Console.WriteLine("You have {0} monitors", monCount);
            else
                Console.WriteLine("An error occured while enumerating monitors");

            return monCount;
        }
    }
}
