using System.Runtime.InteropServices;

namespace NativeLibraryWrapper.MyLibrary
{
    public class Class1
    {
        private const string LibraryName = "NativeLibrary";

        static Class1()
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
