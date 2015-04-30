
using SiliconStudio.Paradox.Engine;

namespace SpaceEscape
{
    class SpaceEscapeApp
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
