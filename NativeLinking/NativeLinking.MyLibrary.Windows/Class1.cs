using System;
using System.Runtime.InteropServices;

namespace NativeLinking.MyLibrary
{
    public class Class1
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int MessageBox(IntPtr hWnd, String text, String caption, int options);

        public void Method1()
        {
            MessageBox(IntPtr.Zero, "Text", "Caption", 0);
        }
    }
}
