
namespace SpriteEntity
{
    class SpriteEntityApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new SpriteEntityGame())
            {
                game.Run();
            }
        }
    }
}
