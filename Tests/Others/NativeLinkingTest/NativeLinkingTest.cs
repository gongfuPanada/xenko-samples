using System;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Testing;

namespace NativeLinkingTest
{
    [TestFixture]
    public class NativeLinkingTest
    {
        private const string Path = "samples\\Others\\NativeLinking\\Bin\\Windows\\Debug\\NativeLinking.exe";

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

                game.TakeScreenshot();

                game.Wait(TimeSpan.FromMilliseconds(20000));
            }
        }
    }
}
