
namespace CustomEffect
{
    class CustomEffectApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new CustomEffectGame())
            {
                game.Run();
            }
        }
    }
}
