using System;
using System.Diagnostics;
using static WindowsPhoneSpeedyBlupi.EnvClasses;

namespace WindowsPhoneSpeedyBlupi
{
    public static class DDebug
    {
        private static bool DETAILED_DEBUGGING { get; set; }
        public static void WriteLine(String msg)
        {
            if (DETAILED_DEBUGGING)
            {
                Debug.WriteLine(msg);
            }
        }
    }
}
