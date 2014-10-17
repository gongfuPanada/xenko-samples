using System.Threading.Tasks;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Effects;

namespace HelloWorld
{
    public class HelloWorldGame : Game
    {
        protected override async Task LoadContent()
        {
            await base.LoadContent();

            RenderSystem.Pipeline.Renderers.Add(new BackgroundRenderer(Services, "ParadoxBackground"));
        }
    }
}
