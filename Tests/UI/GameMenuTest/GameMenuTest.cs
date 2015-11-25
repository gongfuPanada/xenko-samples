﻿using System;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Testing;

namespace GameMenuTest
{
    [TestFixture]
    public class GameMenuTest
    {
        private const string Path = "samples\\UI\\GameMenu\\Bin\\Windows-Direct3D11\\Debug\\GameMenu.exe";

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

                /*
                [GameMenu.MainScript]: Info: X:0.4765625 Y:0.8389084
                [GameMenu.MainScript]: Info: X:0.6609375 Y:0.7315141
                [GameMenu.MainScript]: Info: X:0.6609375 Y:0.7315141
                [GameMenu.MainScript]: Info: X:0.5390625 Y:0.7764084
                [GameMenu.MainScript]: Info: X:0.5390625 Y:0.7764084
                */

                game.Tap(new Vector2(0.4765625f, 0.8389084f), TimeSpan.FromMilliseconds(250));
                game.Wait(TimeSpan.FromMilliseconds(250));
                game.TakeScreenshot();

                game.Tap(new Vector2(0.6609375f, 0.7315141f), TimeSpan.FromMilliseconds(250));
                game.Wait(TimeSpan.FromMilliseconds(250));
                game.TakeScreenshot();

                game.Tap(new Vector2(0.5390625f, 0.7764084f), TimeSpan.FromMilliseconds(250));
                game.Wait(TimeSpan.FromMilliseconds(250));
                game.TakeScreenshot();

                game.Wait(TimeSpan.FromMilliseconds(500));
            }
        }
    }
}