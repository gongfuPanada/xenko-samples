using System.Runtime.InteropServices;

namespace NativeLinking.MyLibrary
{
    public class NativeClass
    {
        private const string LibraryName = "NativeLibrary";

        static NativeClass()
        {
            //This step is necessary under Windows Desktop platform to figure the arch
            SiliconStudio.Core.NativeLibrary.PreloadLibrary(LibraryName + ".dll");
        }

#if SILICONSTUDIO_PLATFORM_IOS
        [DllImport("__Internal")]
        private static extern float NativeFunction(float a, float b);
#else
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern float NativeFunction(float a, float b);
#endif

        public float Method1()
        {
            return NativeFunction(1.0f, 2.0f);
        }
    }
}
