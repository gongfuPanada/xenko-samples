using System.Threading.Tasks;
using NativeLibraryWrapper.MyLibrary;
using SiliconStudio.Paradox.Engine;

namespace NativeLinking
{
    public class Test : AsyncScript
    {
        public override Task Execute()
        {
            var c = new Class1();
            var x = c.Method1();
            Entity.Transform.Position.Y -= x;
            return Task.FromResult(0);
        }
    }
}