
namespace GeometricPrimitives
{
    class GeometricPrimitivesApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new GeometricPrimitivesGame())
            {
                game.Run();
            }
        }
    }
}
