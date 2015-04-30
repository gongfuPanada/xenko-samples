using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.UI;
using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Panels;

namespace SimpleDynamicTexture
{
    /// <summary>
    /// The script in charge of the game UI
    /// </summary>
    public class UIScript : AsyncScript
    {
        private TextBlock textBlock;

        // Create the UI layout and content
        public override void Start()
        {
            base.Start();

            textBlock = new TextBlock
            {
                Text = "Tap The Screen!",
                Font = Asset.Load<SpriteFont>("Font"),
                TextAlignment = TextAlignment.Center,
            };
            textBlock.SetCanvasPinOrigin(new Vector3(0.5f, 0.5f, 0f));
            textBlock.SetCanvasRelativePosition(new Vector3(0.5f, 0.85f, 0f));

            Entity.Get<UIComponent>().RootElement = new Canvas { Children = { textBlock } };
        }

        // Progressively hide the UI after the user first click.
        public override async Task Execute()
        {
            var transparency = 1f;
            var hideMessage = false;
            while (Game.IsRunning && transparency > MathUtil.ZeroTolerance)
            {
                if (Input.PointerEvents.Any())
                    hideMessage = true;

                if (hideMessage)
                    transparency *= 0.94f;

                textBlock.TextColor = transparency * Color.White;

                await Script.NextFrame();
            }
        }
    }
}