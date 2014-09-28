using System;
using MonoTouch.ObjCRuntime;

namespace softWrench.iOS
{
    public static class RuntimeEnvironment
    {
        public static bool IsRunningOnSimulator
        {
            get
            {
                return Runtime.Arch == Arch.SIMULATOR;
            }
        }
    }
}

