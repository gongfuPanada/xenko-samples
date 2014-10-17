
namespace RenderSceneToTexture
{
    class RenderSceneToTextureApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new RenderSceneToTextureGame())
            {
                game.Run();
            }
        }
    }
}
