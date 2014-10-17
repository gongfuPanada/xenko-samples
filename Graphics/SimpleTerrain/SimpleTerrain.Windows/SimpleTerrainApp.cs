
namespace SimpleTerrain
{
    class SimpleTerrainApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new SimpleTerrainGame())
            {
                game.Run();
            }
        }
    }
}
