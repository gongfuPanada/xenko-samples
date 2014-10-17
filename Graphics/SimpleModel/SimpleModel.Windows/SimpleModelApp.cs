
namespace SimpleModel
{
    class SimpleModelApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new SimpleModelGame())
            {
                game.Run();
            }
        }
    }
}
