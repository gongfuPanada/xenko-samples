using SiliconStudio.Paradox.Engine;

namespace SpriteStudioDemo
{
    class SpriteEntityApp
    {
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
