using System.Threading.Tasks;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Rendering;
using SpaceEscape.Background;

namespace SpaceEscape
{
    /// <summary>
    /// GameScript manages all entities in the game: Camera, CharacterScript, BackgroundScript and Obstacles.
    /// </summary>
    public class GameScript : AsyncScript
    {
        /// <summary>
        /// The reference to the character script
        /// </summary>
        public CharacterScript CharacterScript;

        /// <summary>
        /// The reference to the background script
        /// </summary>
        public BackgroundScript BackgroundScript;

        /// <summary>
        /// The reference to the UI script
        /// </summary>
        public UIScript UIScript;
        
        public override void Start()
        {
            base.Start();

            // Enable visual of mouse in the game
            Game.Window.IsMouseVisible = true;

            // Disable multi-touch input for the game, since there is no need
            Input.MultiTouchEnabled = false;

            // Update the distance displayed in the UI
            BackgroundScript.DistanceUpdated += curDist => UIScript.SetDistance((int)curDist);

            // Adjust the color of fog effect.
            GraphicsDevice.Parameters.Set(FogEffectKeys.FogColor, Color.FromAbgr(0xFF7D02FF));
        }

        /// <summary>
        /// Script update loop that detect collision between CharacterScript an obstacles, 
        /// and detect if the CharacterScript falls to any hole.
        /// </summary>
        /// <returns></returns>
        public override async Task Execute()
        {
            // set behavior of UI button
            UIScript.StartButton.Click += (s, e) => StartGame();
            UIScript.RetryButton.Click += (s, e) => StartGame();
            UIScript.MenuButton.Click += (s, e) => GoToMenu();

            while (Game.IsRunning)
            {
                await Script.NextFrame();

                if (CharacterScript.IsDead)
                    continue;

                float floorHeight;
                var agentBoundingBox = CharacterScript.CalculateCurrentBoundingBox();

                // Detect collision between agents and real-world obstacles.
                if (BackgroundScript.DetectCollisions(ref agentBoundingBox))
                    KillAgent(0);

                // Detect if the CharacterScript falls into a hole
                if (BackgroundScript.DetectHoles(ref CharacterScript.Entity.Transform.Position, out floorHeight))
                    KillAgent(floorHeight);
            }
        }

        /// <summary>
        /// Kills the player.
        /// </summary>
        private void KillAgent(float height)
        {
            CharacterScript.OnDied(height);
            UIScript.StartGameOverMode();
            StartGameOverMode();
        }

        /// <summary>
        /// Reset game's entities: CharacterScript and LevelBlocks.
        /// </summary>
        private void ResetGame()
        {
            CharacterScript.Reset();
            BackgroundScript.Reset();
        }

        /// <summary>
        /// Start playing
        /// </summary>
        private void StartGame()
        {
            ResetGame();
            UIScript.StartPlayMode();
            BackgroundScript.StartScrolling();
            CharacterScript.Activate();
        }

        /// <summary>
        /// Go to the menu screen
        /// </summary>
        private void GoToMenu()
        {
            UIScript.StartMainMenuMode();
            ResetGame();
        }

        /// <summary>
        /// Set game state to game over
        /// </summary>
        private void StartGameOverMode()
        {
            BackgroundScript.StopScrolling();
        }
    }
}
