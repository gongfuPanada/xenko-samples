using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Testing;

namespace JumpyJetTest
{
    [TestFixture]
    public class JumpyJetTest
    {
        private const string Path = "samples\\Games\\JumpyJet\\Bin\\Windows-Direct3D11\\Debug\\JumpyJet.exe";

        [Test]
        public void TestLaunch()
        {
            using (var game = new GameTest(Path, PlatformType.Windows))
            {
                game.Wait(2000);
            }
        }

        [Test]
        public void TestInputs()
        {
            using (var game = new GameTest(Path, PlatformType.Windows))
            {
                game.Wait(2000);
                game.Tap(new Vector2(0.5f, 0.7f), 500);
                game.Wait(500);
                game.KeyPress(Keys.Space, 500);
                game.Wait(500);
                game.TakeScreenshot();
                game.Wait(500);
            }
        }
    }
}
