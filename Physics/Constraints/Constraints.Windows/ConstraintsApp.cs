
namespace Constraints
{
    class ConstraintsApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new ConstraintsGame())
            {
                game.Run();
            }
        }
    }
}
