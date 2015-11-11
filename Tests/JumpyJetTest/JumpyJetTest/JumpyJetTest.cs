using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine.Network;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Testing;

namespace JumpyJetTest
{
    [TestFixture]
    public class JumpyJetTest
    {
        private SocketMessageLayer socket;

        public void StartGame()
        {
            var url = $"/service/{XenkoVersion.CurrentAsText}/SiliconStudio.Xenko.SamplesTestServer.exe";

            var socketContext = RouterClient.RequestServer(url).Result;

            var success = true;
            var message = "";
            var ev = new AutoResetEvent(false);

            socket = new SocketMessageLayer(socketContext, false);

            socket.AddPacketHandler<StatusMessageRequest>(request =>
            {
                success = !request.Error;
                message = request.Message;
                ev.Set();
            });

            Task.Run(() => socket.MessageLoop());

            socket.Send(new TestRegistrationRequest
            {
                Platform = (int)PlatformType.Windows,
                Tester = true,
                Cmd = "..\\..\\samples\\Games\\JumpyJet\\Bin\\Windows-Direct3D11\\Debug\\JumpyJet.exe"
            }).Wait();

            if (!ev.WaitOne(10000))
            {
                throw new Exception("Time out while launching the game");
            }

            if (!success)
            {
                throw new Exception("Failed: " + message);
            }
        }

        [Test]
        public void TestLaunch()
        {
            StartGame();

            Task.Delay(2000).Wait();

            socket.Send(new TestEndedRequest()).Wait();
        }

        [Test]
        public void TestInputs()
        {
            StartGame();

            Task.Delay(2000).Wait();

            socket.Send(new TapSimulationRequest { Down = true, Coords = new Vector2(0.5f, 0.7f) }).Wait();
            Task.Delay(500).Wait();
            socket.Send(new TapSimulationRequest { Down = false, Coords = new Vector2(0.5f, 0.7f) }).Wait();
            Task.Delay(500).Wait();

            socket.Send(new KeySimulationRequest {Down = true, Key = Keys.Space}).Wait();
            Task.Delay(500).Wait();
            socket.Send(new KeySimulationRequest { Down = false, Key = Keys.Space }).Wait();
            Task.Delay(500).Wait();

            socket.Send(new ScreenshotRequest {Filename = "C:\\Users\\giovanni.petrantoni\\Desktop\\screenshot.png"}).Wait();
            Task.Delay(500).Wait();

            socket.Send(new TestEndedRequest()).Wait();
        }
    }
}
