
namespace JumpyJet
{
    class JumpyJetApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new JumpyJetGame())
            {
                game.Run();
            }
        }
    }
}
