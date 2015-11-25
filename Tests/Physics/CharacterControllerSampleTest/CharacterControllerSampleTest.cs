using System;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Testing;

namespace CharacterControllerSampleTest
{
    [TestFixture]
    public class CharacterControllerSampleTest
    {
        private const string Path = "samples\\Physics\\CharacterControllerSample\\Bin\\Windows-Direct3D11\\Debug\\CharacterControllerSample.exe";

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

                game.TakeScreenshot();

                game.KeyPress(Keys.Right, TimeSpan.FromMilliseconds(1000));
                
                game.TakeScreenshot();

                game.KeyPress(Keys.Space, TimeSpan.FromMilliseconds(200));
                
                game.TakeScreenshot();

                game.Wait(TimeSpan.FromMilliseconds(500));
            }
        }
    }
}
