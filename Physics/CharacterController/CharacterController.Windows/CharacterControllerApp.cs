
namespace CharacterController
{
    class CharacterControllerApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new CharacterControllerGame())
            {
                game.Run();
            }
        }
    }
}
