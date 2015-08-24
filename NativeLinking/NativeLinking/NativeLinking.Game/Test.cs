using System.Threading.Tasks;
using NativeLinking.MyLibrary;
using SiliconStudio.Paradox.Engine;

namespace NativeLinking
{
    public class Test : AsyncScript
    {
        public override Task Execute()
        {
            var x = new Class1();
            x.Method1();
            return Task.FromResult(0);
        }
    }
}
