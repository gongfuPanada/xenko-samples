
namespace GameMenu
{
    class GameMenuApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new GameMenuGame())
            {
                game.Run();
            }
        }
    }
}
