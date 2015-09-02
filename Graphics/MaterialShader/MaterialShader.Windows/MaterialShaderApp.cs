using SiliconStudio.Paradox.Engine;

namespace MaterialShader
{
    class MaterialShaderApp
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
