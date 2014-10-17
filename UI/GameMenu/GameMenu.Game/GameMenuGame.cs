using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;

namespace GameMenu
{
    public class GameMenuGame : Game
    {
        private SplashScene splashScene;
        private MainScene mainScene;

        public const string TextureDirectory = "Textures/";
        public const string FontDirectory = "Fonts/";

        private static readonly Vector3 GameResolution = new Vector3(640, 1136, 1);

        public GameMenuGame()
        {
            // Target 9.1 profile by default
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1 };
            GraphicsDeviceManager.PreferredBackBufferWidth = (int) GameResolution.X;
            GraphicsDeviceManager.PreferredBackBufferHeight = (int) GameResolution.Y;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            // Enable the user to resize the screen and automatically adjust the size of the Virtual Resolution to the size of the screen to see how layout adapt
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += (s, e) => VirtualResolution = new Vector3(GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height, 1000);

            // For easy debugging, show mouse cursor
            IsMouseVisible = true;

            // Create UI Renderer, and set Virtual resolution for UI
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services));
            RenderSystem.Pipeline.Renderers.Add(new UIRenderer(Services));
            VirtualResolution = GameResolution;

            // Create Scenes
            splashScene = new SplashScene(this);
            mainScene = new MainScene(this);
            
            // Start splash scene
            Script.Add(splashScene); //Execute the first scene (Splash scene)
            UI.RootElement = splashScene.RootElement;
        }

        public void GoToMainScene()
        {
            // Stop updating splash scene
            splashScene.IsRunning = false;
            // Change UI root to Main scene
            UI.RootElement = mainScene.RootElement;
            // Pop welcome dialog in MainScene
            mainScene.ShowWelcomePopup();
            // Execute Main scene script for update
            Script.Add(mainScene);
        }

        protected override void Destroy()
        {
            splashScene.Dispose();
            base.Destroy();
        }
    }
}
