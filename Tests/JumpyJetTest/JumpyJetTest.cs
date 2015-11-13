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
        public SocketMessageLayer StartGame()
        {
            var url = $"/service/{XenkoVersion.CurrentAsText}/SiliconStudio.Xenko.SamplesTestServer.exe";

            var socketContext = RouterClient.RequestServer(url).Result;

            var success = false;
            var message = "";
            var ev = new AutoResetEvent(false);

            var socket = new SocketMessageLayer(socketContext, false);

            socket.AddPacketHandler<StatusMessageRequest>(request =>
            {
                success = !request.Error;
                message = request.Message;
                ev.Set();
            });

            socket.AddPacketHandler<LogRequest>(request =>
            {
                Console.WriteLine(request.Message);
            });

            Task.Run(() => socket.MessageLoop());

            var xenkoDir = Environment.GetEnvironmentVariable("SiliconStudioXenkoDir");

            socket.Send(new TestRegistrationRequest
            {
                Platform = (int)PlatformType.Windows,
                Tester = true,
                Cmd = xenkoDir + "\\samples\\Games\\JumpyJet\\Bin\\Windows-Direct3D11\\Debug\\JumpyJet.exe"
            }).Wait();

            if (!ev.WaitOne(10000))
            {
                throw new Exception("Time out while launching the game");
            }

            if (!success)
            {
                throw new Exception("Failed: " + message);
            }

            Console.WriteLine("Game started. (message: " + message + ")");

            return socket;
        }

        [Test]
        public void TestLaunch()
        {
            var socket = StartGame();

            Task.Delay(2000).Wait();

            socket.Send(new TestEndedRequest()).Wait();
        }

        [Test]
        public void TestInputs()
        {
            var socket = StartGame();           

            Task.Delay(2000).Wait();

            socket.Send(new TapSimulationRequest { Down = true, Coords = new Vector2(0.5f, 0.7f) }).Wait();
            Console.WriteLine("TapSimulationRequest Down.");
            Task.Delay(500).Wait();
            socket.Send(new TapSimulationRequest { Down = false, Coords = new Vector2(0.5f, 0.7f) }).Wait();
            Console.WriteLine("TapSimulationRequest Up.");
            Task.Delay(500).Wait();

            socket.Send(new KeySimulationRequest {Down = true, Key = Keys.Space}).Wait();
            Console.WriteLine("KeySimulationRequest Down.");
            Task.Delay(500).Wait();
            socket.Send(new KeySimulationRequest { Down = false, Key = Keys.Space }).Wait();
            Console.WriteLine("KeySimulationRequest Up.");
            Task.Delay(500).Wait();

            var xenkoDir = Environment.GetEnvironmentVariable("SiliconStudioXenkoDir");

            socket.Send(new ScreenshotRequest {Filename = xenkoDir + "\\screenshots\\JumpyJet.png" }).Wait();
            Console.WriteLine("ScreenshotRequest.");
            Task.Delay(500).Wait();

            socket.Send(new TestEndedRequest()).Wait();
        }
    }
}
