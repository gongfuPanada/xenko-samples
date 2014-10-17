using System;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;

namespace SpaceEscape
{
    /// <summary>
    /// Game Entry class that manages UI and Game components and their interaction.
    /// </summary>
    public class SpaceEscapeGame : Game
    {
        public enum GameState
        {
            Menu,
            Play,
            GameOver,
        }

        private static readonly Vector3 GameVirtualResolution = new Vector3(640, 1136, 1f);

        private GameModule gameModule;
        private UIModule uiModule;
        private GameState gameState;

        private new GameState State
        {
            set
            {
                gameState = value;
                OnGameStateChanged(gameState);
            }
        }

        public SpaceEscapeGame()
        {
            // Target 9.1 profile by default
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
                    gameModule.Reset();
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
        /// Create and load UI and Game component. Also, set required delegates for their interaction.
        /// </summary>
        /// <returns></returns>
        protected override async Task LoadContent()
        {
            await base.LoadContent();

            CreateRenderingPipelines();

            // Set virtual Resolution
            VirtualResolution = GameVirtualResolution;

            // Enable visual of mouse in the game
            Window.IsMouseVisible = true;

            // Disable multi-touch input for the game, since there is no need
            Input.MultiTouchEnabled = false;

            gameModule = new GameModule(Services);
            gameModule.LoadContent();

            uiModule = new UIModule(Services);
            uiModule.LoadContent();

            // Register delegates
            gameModule.AgentDied += () => State = GameState.GameOver;
            gameModule.DistanceUpdated += curDist => uiModule.SetDistance((int) curDist);
            uiModule.StartButton.Click += (s, e) => State = GameState.Play;
            uiModule.RetryButton.Click += (s, e) => State = GameState.Play;
            uiModule.MenuButton.Click += (s, e) => State = GameState.Menu;

            // Start Game script.
            Script.Add(gameModule);
            State = GameState.Menu;
        }

        private void CreateRenderingPipelines()
        {
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services) { ClearColor = Color.CornflowerBlue }); // sets and clears the render targets
            RenderSystem.Pipeline.Renderers.Add(new CameraSetter(Services)); // sets and update the camera
            RenderSystem.Pipeline.Renderers.Add(new ModelRenderer(Services, "SpaceEscapeEffectMain")); // render models added to the scene
            RenderSystem.Pipeline.Renderers.Add(new UIRenderer(Services));
        }
    }
}
