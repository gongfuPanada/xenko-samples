﻿using System;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Testing;

namespace MaterialSampleTest
{
    [TestFixture]
    public class MaterialSampleTest
    {
        private const string Path = "samples\\Graphics\\MaterialSample\\Bin\\Windows-Direct3D11\\Debug\\MaterialSample.exe";

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
                game.Wait(TimeSpan.FromMilliseconds(4000));

                game.TakeScreenshot();

                game.Wait(TimeSpan.FromMilliseconds(2000));
            }
        }
    }
}
