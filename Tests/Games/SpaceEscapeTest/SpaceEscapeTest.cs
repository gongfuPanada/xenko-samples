using System;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Testing;

namespace SpaceEscapeTest
{
    [TestFixture]
    public class SpaceEscapeTest
    {
        private const string Path = "samples\\Games\\SpaceEscape\\Bin\\Windows-Direct3D11\\Debug\\SpaceEscape.exe";

#if TEST_ANDROID
        private const PlatformType TestPlatform = PlatformType.Android;
#else
        private const PlatformType TestPlatform = PlatformType.Windows;
#endif

        [Test]
        public void TestLaunch()
        {
            using (var game = new GameTest(Path, TestPlatform))
            {
                game.Wait(TimeSpan.FromMilliseconds(2000));
            }
        }

        [Test]
        public void TestInputs()
        {
            using (var game = new GameTest(Path, TestPlatform))
            {
                game.Wait(TimeSpan.FromMilliseconds(2000));
                //X:0.496875 Y:0.8010563
                game.Tap(new Vector2(0.496875f, 0.8010563f), TimeSpan.FromMilliseconds(250));
                game.Wait(TimeSpan.FromMilliseconds(1000));
                game.TakeScreenshot();
                game.KeyPress(Keys.Space, TimeSpan.FromMilliseconds(500));
                game.Wait(TimeSpan.FromMilliseconds(500));
                game.TakeScreenshot();
                game.Wait(TimeSpan.FromMilliseconds(500));
            }
        }
    }
}
