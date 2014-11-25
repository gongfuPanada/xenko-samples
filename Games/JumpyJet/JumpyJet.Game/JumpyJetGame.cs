using System;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;

namespace JumpyJet
{
    /// <summary>
    /// A game class which is an entry point of the application.
    /// It manages game and ui components and the game state.
    /// </summary>
    public class JumpyJetGame : Game
    {
        public enum GameState
        {
            Menu,
            Play,
            GameOver,
        }

        // Setting Virtual Resolution so that the screen has 640 and 1136 of Width and Height respectively.
        // Note that the Z component indicates the near and farplane [near, far] = [-10, 10]
        private static readonly Vector3 GameVirtualResolution = new Vector3(640, 1136, 20f);

        private GameModule gameModule;
        private UIModule uiModule;
        private GameState gameState;
        private Entity camera;

        private new GameState State
        {
            set
            {
                gameState = value;
                OnGameStateChanged(gameState);
            }
        }

        public JumpyJetGame()
        {
            // Target 9.1 profile by default.
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1 };
            GraphicsDeviceManager.PreferredBackBufferWidth = (int) GameVirtualResolution.X;
            GraphicsDeviceManager.PreferredBackBufferHeight = (int) GameVirtualResolution.Y;
        }

        /// <summary>
        /// Callback that executed when game state is changed.
        /// It, in turn, notifies game and ui components and change their modes.
        /// </summary>
        /// <param name="state"></param>
        private void OnGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Menu:
                    uiModule.StartMainMenuMode();
                    gameModule.StartMainMenuMode();
                    break;
                case GameState.Play:
                    uiModule.StartPlayMode();
                    gameModule.StartPlayMode();
                    break;
                case GameState.GameOver:
                    uiModule.StartGameOverMode();
                    gameModule.StartGameOverMode();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("state");
            }
        }

        /// <summary>
        /// Initialize game and ui components, and their state
        /// </summary>
        /// <returns></returns>
        protected override async Task LoadContent()
        {
            await base.LoadContent();
            
            // Set virtual Resolution
            VirtualResolution = GameVirtualResolution;

            // Create the camera
            camera = new Entity("Camera") { new CameraComponent { UseProjectionMatrix = true, ProjectionMatrix = SpriteBatch.CalculateDefaultProjection(VirtualResolution)} };
            Entities.Add(camera);

            CreateRenderingPipelines();

            // Enable visual of mouse in the game
            Window.IsMouseVisible = true;

            // Disable multi-touch input for the game, since there is no need
            Input.MultiTouchEnabled = false;

            // Create and initialize game component
            gameModule = new GameModule(Services);
            gameModule.LoadContent();
            gameModule.AgentDied += () => State = GameState.GameOver;

            // Start script for game component
            gameModule.ExecuteScript();

            // Create and initialize ui component
            uiModule = new UIModule(Services);
            uiModule.LoadContent();

            gameModule.ScoreUpdated += uiModule.SetScore;
            uiModule.StartButton.Click += (s,e) => State = GameState.Play;
            uiModule.RetryButton.Click += (s, e) => State = GameState.Play;
            uiModule.MenuButton.Click += (s, e) => State = GameState.Menu;

            // Initialize the initial state of the game
            State = GameState.Menu;
        }

        private void CreateRenderingPipelines()
        {
            // Create the camera setter. This set the camera at the beginning of the pipeline
            var cameraSetter = new CameraSetter(Services) {Camera = camera.Get<CameraComponent>()};

            // Create the RenderTarget setter. This clears and sets the render targets.
            var renderTargetSetter = new RenderTargetSetter(Services) { ClearColor = Color.LightBlue };

            // Create a DelegateRenderer to render custom sprites manually.
            var delegateBackgroundRenderer = new DelegateRenderer(Services);
            delegateBackgroundRenderer.Render += delegate { gameModule.DrawBackgroundParallax(DrawTime); };

            // Create a Sprite Renderer to render SpriteComponents.
            var spriteRenderer = new SpriteRenderer(Services);

            // Create UI Renderer
            var uiRenderer = new UIRenderer(Services);

            // Add renderers into the pipeline. Note that the order matters. 
            // The renderers are called in the same order they are included into the pipeline.
            RenderSystem.Pipeline.Renderers.Add(cameraSetter);
            RenderSystem.Pipeline.Renderers.Add(renderTargetSetter);
            RenderSystem.Pipeline.Renderers.Add(delegateBackgroundRenderer);
            RenderSystem.Pipeline.Renderers.Add(spriteRenderer);
            RenderSystem.Pipeline.Renderers.Add(uiRenderer);
        }
    }
}

