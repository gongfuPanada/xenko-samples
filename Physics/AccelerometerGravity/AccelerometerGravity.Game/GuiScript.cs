using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.UI;
using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Panels;

namespace AccelerometerGravity
{
    public class GuiScript : AsyncScript
    {
        public override async Task Execute()
        {
            var textBlock = new TextBlock
            {
                Text = "Use arrows to play with gravity!", 
                Font = Asset.Load<SpriteFont>("SpriteFont"), 
                TextColor = Color.White, 
                TextSize = 40
            };
            textBlock.SetCanvasPinOrigin(new Vector3(0.5f, 0.5f, 0));
            textBlock.SetCanvasRelativePosition(new Vector3(0.5f, 0.75f, 0f));
            Entity.Get<UIComponent>().RootElement = new Canvas { Children = { textBlock } };

            while (Game.IsRunning)
            {
                await Script.NextFrame();

                if (!Input.IsKeyPressed(Keys.Left) && !Input.IsKeyPressed(Keys.Right) && !Input.IsKeyPressed(Keys.Up) &&
                    !Input.IsKeyPressed(Keys.Down)) continue;

                Entity.Get<UIComponent>().RootElement = null;
                return;
            }
        }
    }
}