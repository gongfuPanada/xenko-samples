using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.UI;
using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Panels;

namespace Raycasting
{
    /// <summary>
    /// The script in charge of displaying the UI
    /// </summary>
    public class GuiScript : StartupScript
    {
        public override void Start()
        {
            base.Start();

            var textBlock = new TextBlock
            {
                Text = "Shoot the cubes!",
                Font = Asset.Load<SpriteFont>("Font"),
                TextColor = Color.White,
                TextSize = 60
            };
            textBlock.SetCanvasPinOrigin(new Vector3(0.5f, 0.5f, 0));
            textBlock.SetCanvasRelativePosition(new Vector3(0.5f, 0.9f, 0f));

            Entity.Get<UIComponent>().RootElement = new Canvas { Children = { textBlock } };
        }
    }
}