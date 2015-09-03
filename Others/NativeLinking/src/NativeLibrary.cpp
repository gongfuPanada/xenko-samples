// NativeLibrary.Windows.cpp : Defines the exported functions for the DLL application.
//

#if  defined(WIN32) || defined(_WINDLL)
#define EXPORTDLL __declspec(dllexport)
#else
#define EXPORTDLL
#endif

extern "C" EXPORTDLL float NativeFunction(float a, float b)
{
	return a + b;
}


