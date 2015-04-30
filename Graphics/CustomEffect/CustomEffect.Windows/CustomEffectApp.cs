using SiliconStudio.Paradox.Engine;

namespace CustomEffect
{
    class CustomEffectApp
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
