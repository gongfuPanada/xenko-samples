
namespace SpaceEscape
{
    class SpaceEscapeApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new SpaceEscapeGame())
            {
                game.Run();
            }
        }
    }
}
