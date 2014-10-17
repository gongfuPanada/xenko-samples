
namespace TouchInputs
{
    class TouchInputsApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new TouchInputsGame())
            {
                game.Run();
            }
        }
    }
}
