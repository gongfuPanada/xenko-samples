using SiliconStudio.Paradox.Engine;

namespace SimpleModel
{
    class SimpleModelApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new Game())
                game.Run();
        }
    }
}
