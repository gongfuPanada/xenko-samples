using SiliconStudio.Xenko.Engine;

namespace UIElementLink
{
    class UIElementLinkApp
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
