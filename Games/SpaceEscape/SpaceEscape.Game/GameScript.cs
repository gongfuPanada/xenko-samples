﻿using System;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Rendering;
using SpaceEscape.Background;

namespace SpaceEscape
{
    /// <summary>
    /// GameScript manages all entities in the game: Camera, CharacterScript, BackgroundScript and Obstacles.
    /// </summary>
    public class GameScript : SyncScript
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

        private bool isFirstUpdate;

        public override void Start()
        {
            // Enable visual of mouse in the game
            Game.Window.IsMouseVisible = true;

            // Disable multi-touch input for the game, since there is no need
            Input.MultiTouchEnabled = false;

            // Update the distance displayed in the UI
            BackgroundScript.DistanceUpdated += SetDistanceInUI;

            // Adjust the color of fog effect.
            GraphicsDevice.Parameters.Set(FogEffectKeys.FogColor, Color.FromAbgr(0xFF7D02FF));

            // set behavior of UI button
            UIScript.StartButton.Click += StartGame;
            UIScript.RetryButton.Click += StartGame;
            UIScript.MenuButton.Click += GoToMenu;

            isFirstUpdate = true;
        }

        /// <summary>
        /// Script update loop that detect collision between CharacterScript an obstacles, 
        /// and detect if the CharacterScript falls to any hole.
        /// </summary>
        /// <returns></returns>
        public override void Update()
        {
            if (isFirstUpdate)
            {
                KillAgent(0);
                GoToMenu(this, EventArgs.Empty);
                isFirstUpdate = false;
            }

            if (CharacterScript.IsDead)
                return;

            float floorHeight;
            var agentBoundingBox = CharacterScript.CalculateCurrentBoundingBox();

            // Detect collision between agents and real-world obstacles.
            if (BackgroundScript.DetectCollisions(ref agentBoundingBox))
                KillAgent(0);

            // Detect if the CharacterScript falls into a hole
            if (BackgroundScript.DetectHoles(ref CharacterScript.Entity.Transform.Position, out floorHeight))
                KillAgent(floorHeight);
        }

        public override void Cancel()
        {
            BackgroundScript.DistanceUpdated -= SetDistanceInUI;

            UIScript.StartButton.Click -= StartGame;
            UIScript.RetryButton.Click -= StartGame;
            UIScript.MenuButton.Click -= GoToMenu;
        }

        private void SetDistanceInUI(float curDist)
        {
            UIScript.SetDistance((int)curDist);
        }

        /// <summary>
        /// Kills the player.
        /// </summary>
        private void KillAgent(float height)
        {
            CharacterScript.OnDied(height);
            UIScript.StartGameOverMode();
            BackgroundScript.StopScrolling();
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
        private void StartGame(object sender, EventArgs args)
        {
            ResetGame();
            UIScript.StartPlayMode();
            BackgroundScript.StartScrolling();
            CharacterScript.Activate();
        }

        /// <summary>
        /// Go to the menu screen
        /// </summary>
        private void GoToMenu(object sender, EventArgs args)
        {
            UIScript.StartMainMenuMode();
            ResetGame();
        }
    }
}