
namespace AccelerometerGravity
{
    class AccelerometerGravityApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new AccelerometerGravityGame())
            {
                game.Run();
            }
        }
    }
}
