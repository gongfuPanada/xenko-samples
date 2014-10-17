
namespace HelloWorld
{
    class HelloWorldApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new HelloWorldGame())
            {
                game.Run();
            }
        }
    }
}
