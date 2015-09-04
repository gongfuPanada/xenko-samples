
using NativeLinking.MyLibrary;
using SiliconStudio.Paradox.Engine;

namespace NativeLinking
{
    public class NativeScript : StartupScript
    {
        public override void Start()
        {
            var c = new NativeClass();
            var x = c.Method1();
            Entity.Transform.Position.Y -= x;
        }
    }
}