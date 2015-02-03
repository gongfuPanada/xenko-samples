using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.UI;
using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Panels;

namespace SimpleDynamicTexture
{
    /// <summary>
    /// This sample shows how to create and write data to a texture on CPU side, and then use it for rendering.
    /// A 16*16 texture is created and stretched to cover a screen. You could tap on the screen to lit/dim a pixel of the texture.
    /// </summary>
    public class SimpleDynamicTextureGame : Game
    {
        /// <summary>
        /// An array containing indices to draw a default shape to a texture
        /// </summary>
        private static readonly int[] SymmetricDefaultShape =
        {
            6, 1, 7, 1,
            0, 2, 5, 2, 6, 2, 7, 2,
            1, 3, 4, 3, 5, 3, 6, 3, 7, 3,
            1, 4, 3, 4, 4, 4, 7, 4,
            2, 5, 3, 5,
            2, 6, 3, 6, 5, 6, 6, 6,
            2, 7, 3, 7,
            2, 8, 3, 8,
            3, 9, 4, 9, 7, 9,
            4, 10, 5, 10, 6, 10, 7, 10,
            5, 11, 6, 11, 7, 11,
            4, 12, 5, 12, 7, 12,
            4, 13, 7, 13,
            6, 14, 7, 14,
            6, 15
        };

        /// <summary>
        /// Lit color
        /// </summary>
        private static readonly ColorBGRA ParadoxColor = Color.MediumPurple;

        /// <summary>
        /// Dim color
        /// </summary>
        private static readonly ColorBGRA TransparentColor = Color.Zero;

        /// <summary>
        /// A sprite batch that is used to draw a texture
        /// </summary>
        private SpriteBatch spriteBatch;

        private const int RenderTextureSize = 16;
        private Texture renderTexture;
        private readonly ColorBGRA[] textureData = new ColorBGRA[RenderTextureSize * RenderTextureSize];

        private SpriteFont arial;

        public SimpleDynamicTextureGame()
        {
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1 };
            GraphicsDeviceManager.PreferredBackBufferWidth = 640;
            GraphicsDeviceManager.PreferredBackBufferHeight = 1136;
        }

        /// <summary>
        /// Initializes resources: Pipeline, a textbox for "Tap The Screen" and initial texture data
        /// </summary>
        /// <returns></returns>
        protected override async Task LoadContent()
        {
            await base.LoadContent();

            // Setup the default rendering pipeline
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services));
            RenderSystem.Pipeline.Renderers.Add(new BackgroundRenderer(Services, "ParadoxBackground"));
            RenderSystem.Pipeline.Renderers.Add(new DelegateRenderer(Services) { Render = RenderTexture });
            RenderSystem.Pipeline.Renderers.Add(new UIRenderer(Services));

            arial = Asset.Load<SpriteFont>("Arial");

            // Create UI
            var textBlock = new TextBlock
            {
                Font = arial,
                Text = "Tap The Screen!",
                TextAlignment = TextAlignment.Center,
            };

            textBlock.SetCanvasPinOrigin(new Vector3(0.5f, 0.5f, 0f));
            textBlock.SetCanvasRelativePosition(new Vector3(0.5f, 0.85f, 0f));

            var canvas = new Canvas();
            canvas.Children.Add(textBlock);

            UI.RootElement = canvas;

            spriteBatch = new SpriteBatch(GraphicsDevice);

            renderTexture = Texture.New2D(GraphicsDevice, RenderTextureSize, RenderTextureSize, 1, PixelFormat.B8G8R8A8_UNorm, usage:GraphicsResourceUsage.Dynamic);

            // Setup initial data in "SymmetricDefaultShape" to the texture
            for (var i = 0; i < SymmetricDefaultShape.Length; i += 2)
            {
                TogglePixel(SymmetricDefaultShape[i], SymmetricDefaultShape[i + 1]);
                if (SymmetricDefaultShape[i] != (RenderTextureSize - 1) - SymmetricDefaultShape[i]) 
                    TogglePixel((RenderTextureSize - 1) - SymmetricDefaultShape[i], SymmetricDefaultShape[i + 1]);
            }

            renderTexture.SetData(textureData);
            Script.Add(UpdateInput);
        }

        /// <summary>
        /// Lids or Dims a pixel in the texture for a given coordinate (x, y)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void TogglePixel(int x, int y)
        {
            var index = RenderTextureSize*y + x;
            textureData[index] = (textureData[index] != ParadoxColor) ? ParadoxColor : TransparentColor;
        }

        /// <summary>
        /// Updates input by polling to check for a tap event in order to lid or dim a target pixel, then update the new data to the texture
        /// </summary>
        /// <returns></returns>
        private async Task UpdateInput()
        {
            while (IsRunning)
            {
                await Script.NextFrame();

                if (Input.PointerEvents.Count == 0) continue;

                var destinationRectangle = new RectangleF(0, 0, GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height);

                // Process pointer event
                foreach (var pointerEvent in Input.PointerEvents)
                {
                    var pixelPosition = pointerEvent.Position * new Vector2(GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height);

                    if (pointerEvent.State != PointerState.Down || !destinationRectangle.Contains(pixelPosition)) continue;

                    var relativePosition = (pixelPosition - destinationRectangle.TopLeft);

                    var pixelX = (int)((relativePosition.X / destinationRectangle.Width) * RenderTextureSize);
                    var pixelY = (int)((relativePosition.Y / destinationRectangle.Height) * RenderTextureSize);

                    TogglePixel(pixelX, pixelY);
                }

                renderTexture.SetData(textureData);
            }
        }

        /// <summary>
        /// Renders the dynamic texture to the screen with sprite batch 
        /// </summary>
        /// <param name="renderContext"></param>
        private void RenderTexture(RenderContext renderContext)
        {
            spriteBatch.Begin(SpriteSortMode.Texture, null, GraphicsDevice.SamplerStates.PointClamp);

            spriteBatch.Draw(renderTexture, new RectangleF(0, 0, GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height), Color.White);

            spriteBatch.End();
        }

        /// <summary>
        /// Unloads the resources: arial font
        /// </summary>
        protected override void UnloadContent()
        {
            base.UnloadContent();

            // todo: sprite font crash when unloading
//            Asset.Unload(arial);
        }
    }
}
