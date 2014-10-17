
namespace SpriteFonts
{
    class SpriteFontsApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new SpriteFontsGame())
            {
                game.Run();
            }
        }
    }
}
