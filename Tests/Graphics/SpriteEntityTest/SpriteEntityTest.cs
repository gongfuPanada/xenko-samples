using System;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Testing;

namespace SpriteEntityTest
{
    [TestFixture]
    public class SpriteEntityTest
    {
        private const string Path = "samples\\Graphics\\SpriteEntity\\Bin\\Windows-Direct3D11\\Debug\\SpriteEntity.exe";

        [Test]
        public void TestLaunch()
        {
            using (var game = new GameTest(Path, PlatformType.Windows))
            {
                game.Wait(TimeSpan.FromMilliseconds(2000));
            }
        }

        [Test]
        public void TestInputs()
        {
            using (var game = new GameTest(Path, PlatformType.Windows))
            {
                game.Wait(TimeSpan.FromMilliseconds(2000));

                game.Tap(new Vector2(0.83f, 0.05f), TimeSpan.FromMilliseconds(500));
                game.Wait(TimeSpan.FromMilliseconds(2000));
                game.TakeScreenshot();
                game.Wait(TimeSpan.FromMilliseconds(500));

                game.KeyPress(Keys.Space, TimeSpan.FromMilliseconds(200));
                game.Wait(TimeSpan.FromMilliseconds(100));
                game.TakeScreenshot();
                game.Wait(TimeSpan.FromMilliseconds(500));
            }
        }
    }
}
