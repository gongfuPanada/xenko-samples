
using SiliconStudio.Paradox.Engine;

namespace AccelerometerGravity
{
    class AccelerometerGravityApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
