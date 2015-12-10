﻿using System;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games.Testing;

namespace CustomEffectTest
{
    [TestFixture]
    public class CustomEffectTest
    {
        private const string Path = "samples\\Graphics\\CustomEffect\\Bin\\Windows-Direct3D11\\Debug\\CustomEffect.exe";

#if TEST_ANDROID
        private const PlatformType TestPlatform = PlatformType.Android;
#else
        private const PlatformType TestPlatform = PlatformType.Windows;
#endif

        [Test]
        public void TestLaunch()
        {
            using (var game = new GameTestingClient(Path, TestPlatform))
            {
                game.Wait(TimeSpan.FromMilliseconds(2000));
            }
        }

        [Test]
        public void TestInputs()
        {
            using (var game = new GameTestingClient(Path, TestPlatform))
            {
                game.Wait(TimeSpan.FromMilliseconds(2000));
                game.TakeScreenshot();
                game.Wait(TimeSpan.FromMilliseconds(500));
            }
        }
    }
}
