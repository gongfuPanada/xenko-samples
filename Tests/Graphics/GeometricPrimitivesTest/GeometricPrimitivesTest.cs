﻿using System;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Testing;

namespace GeometricPrimitivesTest
{
    [TestFixture]
    public class GeometricPrimitivesTest
    {
        private const string Path = "samples\\Graphics\\GeometricPrimitives\\Bin\\Windows-Direct3D11\\Debug\\GeometricPrimitives.exe";

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

                game.Wait(TimeSpan.FromMilliseconds(500));
                game.TakeScreenshot();

                game.Drag(new Vector2(0.5f, 0.5f), new Vector2(0.01f, 0.5f), TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(500));

                game.Wait(TimeSpan.FromMilliseconds(500));
                game.TakeScreenshot();

                game.Drag(new Vector2(0.5f, 0.5f), new Vector2(0.9f, 0.5f), TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(500));
                game.Drag(new Vector2(0.5f, 0.5f), new Vector2(0.9f, 0.5f), TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(500));

                game.Wait(TimeSpan.FromMilliseconds(500));
                game.TakeScreenshot();

                game.Wait(TimeSpan.FromMilliseconds(2000));
            }
        }
    }
}
