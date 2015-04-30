using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.UI;
using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Panels;

namespace SimpleAudio
{
    /// <summary>
    /// The script in charge displaying the sample UI.
    /// </summary>
    public class UITextScript : Script
    {
        /// <summary>
        /// The text to display on the screen.
        /// </summary>
        public string UIText = "Tap on the screen!";

        public override void Start()
        {
            base.Start();

            var font = Asset.Load<SpriteFont>("Font");
            var textBlock = new TextBlock { TextColor = Color.White, Font = font, Text = UIText };
            textBlock.SetCanvasPinOrigin(new Vector3(1, 0, 0));
            textBlock.SetCanvasRelativePosition(new Vector3(0.63f, 0.8f, 0f));

            Entity.Get<UIComponent>().RootElement = new Canvas { Children = { textBlock } };
        }
    }
}
