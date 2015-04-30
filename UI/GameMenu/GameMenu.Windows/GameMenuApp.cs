using SiliconStudio.Paradox.Engine;

namespace GameMenu
{
    class GameMenuApp
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
