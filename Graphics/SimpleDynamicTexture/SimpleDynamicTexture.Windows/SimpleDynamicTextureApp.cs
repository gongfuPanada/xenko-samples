
namespace SimpleDynamicTexture
{
    class SimpleDynamicTextureApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new SimpleDynamicTextureGame())
            {
                game.Run();
            }
        }
    }
}
