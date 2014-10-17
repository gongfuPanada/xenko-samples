using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;

namespace JumpyJet
{
    /// <summary>
    /// GameModule wraps all entities in that game, update pipe-sets,
    /// checking for collision between agent and pipe-sets, and draw those sprites.
    /// </summary>
    public class GameModule : ScriptContext
    {
        private const float GameSpeed = 290f;
        private const int GapBetweenPipe = 400;
        private const int StartPipePosition = 400;

        // Entities' depth
        public const int Pal0Depth = 0;
        public const int Pal1Depth = 1;
        public const int Pal2Depth = 2;
        public const int Pal3Depth = 3;
        public const int PipeDepth = 4;
        public const int AgentDepth = 5;

        private readonly SpriteBatch spriteBatch;

        private int score;
        private Agent agent;
        private readonly List<PipeSet> pipeSetList = new List<PipeSet>();
        private readonly List<BackgroundSprite> backgroundParallax = new List<BackgroundSprite>();

        public event Action AgentDied
        {
            add { agent.DieAnimationFinished += value; }
            remove { agent.DieAnimationFinished -= value; }
        }

        public event Action<int> ScoreUpdated;
        public bool IsGameUpdating { get; set; }
        public bool IsRunning { get; set; }

        public int Score
        {
            get{ return score; }
            set
            {
                score = value;
                if (ScoreUpdated != null)
                    ScoreUpdated(score);
            }
        }

        public GameModule(IServiceRegistry registry) 
            : base(registry)
        {
            spriteBatch = new SpriteBatch(GraphicsDevice){ VirtualResolution = VirtualResolution };
        }

        /// <summary>
        /// Load assets that will be used in the game,
        /// and create entities in the game {Agent, Parallax backgrounds, PipeSets}
        /// </summary>
        /// <returns></returns>
        public void LoadContent()
        {
            // Load assets
            var agentEntity = Asset.Load<Entity>("agent_entity");
            var pipeEntity = Asset.Load<Entity>("pipe_entity");
            var pal0SpriteGroup = Asset.Load<SpriteGroup>("pal0_sprite");
            var pal1SpriteGroup = Asset.Load<SpriteGroup>("pal1_sprite");
            var pal2SpriteGroup = Asset.Load<SpriteGroup>("pal2_sprite");
            var pal3SpriteGroup = Asset.Load<SpriteGroup>("pal3_sprite");

            agent = new Agent(agentEntity, Input, VirtualResolution);

            // Create Parallax Background
            backgroundParallax.Add(new BackgroundSprite(pal0SpriteGroup.Images.First(), VirtualResolution, GameSpeed / 4f, Pal0Depth));
            backgroundParallax.Add(new BackgroundSprite(pal1SpriteGroup.Images.First(), VirtualResolution, GameSpeed / 3f, Pal1Depth));
            backgroundParallax.Add(new BackgroundSprite(pal2SpriteGroup.Images.First(), VirtualResolution, GameSpeed / 1.5f, Pal2Depth));

            // For pal3Sprite: Ground, move it downward so that its bottom edge is at the bottom screen.
            var screenHeight = VirtualResolution.Y;
            var pal3Height = pal3SpriteGroup.Images.First().Region.Height;
            backgroundParallax.Add(new BackgroundSprite(pal3SpriteGroup.Images.First(), VirtualResolution, GameSpeed, Pal3Depth, Vector2.UnitY * (screenHeight - pal3Height) / 2));

            // Create PipeSets
            CreatePipe(pipeEntity, StartPipePosition, "Pipe1");
            CreatePipe(pipeEntity, StartPipePosition + GapBetweenPipe, "Pipe2");
        }

        /// <summary>
        /// Add in game scripts, and UpdateLoop in Script system to start the scripts.
        /// </summary>
        public void ExecuteScript()
        {
            // Add pipe entities to EntitySystem to enable rendering.
            foreach (var pipeSet in pipeSetList)
            {
                Entities.Add(pipeSet.TopSpriteEntity);
                Entities.Add(pipeSet.BottomSpriteEntity);
            }

            Entities.Add(agent.Entity);
            Script.Add(UpdateLoop);
        }

        /// <summary>
        /// Executed once a frame. It updates PipeSet, checks position between pipesets and the agent,
        /// and checks whether a score should be increased or not,
        /// if so, trigger an event ScoreUpdated to update UI.
        /// </summary>
        /// <returns></returns>
        public async Task UpdateLoop()
        {
            IsRunning = true;

            while (IsRunning)
            {
                // Wait next rendering frame
                await Script.NextFrame();

                var elapsedTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;

                // Update Parallax backgrounds
                foreach (var parallax in backgroundParallax)
                    parallax.Update(elapsedTime);

                if (!IsGameUpdating)
                    continue;

                // Update Agent
                agent.Update(elapsedTime);

                for (var i = 0; i < pipeSetList.Count; i++)
                {
                    var pipeSet = pipeSetList[i];

                    // Update a PipeSet.
                    pipeSet.Update(elapsedTime);

                    // Check if a pipe should be reset or not
                    if (pipeSet.ShouldReset)
                        ResetPipe(i, pipeSet);

                    // Update Collision
                    if (!agent.IsAlive || UpdateCollision(pipeSet))
                        continue;

                    // Update Score
                    UpdateScore(pipeSet);
                }
            }
        }

        private bool UpdateCollision(PipeSet pipeSet)
        {
            // Check collision, and score only if
            // the player pass through the pipe.
            if (agent.Position.X - agent.AgentWidth/2f > pipeSet.ScrollPos + pipeSet.PipeWidth/2f)
                return true;

            // Check collision between a pipe and the agent.
            if (pipeSet.IsCollide(agent))
            {
                agent.OnCollided();
                return true;
            }
            return false;
        }

        private void UpdateScore(PipeSet pipeSet)
        {
            // Check if we should increase a score or not by 
            // finding if a pipe set has passed the agent.
            if (pipeSet.IsPassedAgent || agent.Position.X < pipeSet.ScrollPos)
                return;

            // A pipe-set has passed the agent, increase a score,
            // and trigger an event to update UI score.
            Score++;
            pipeSet.IsPassedAgent = true;
        }

        private void ResetPipe(int pipeSetIndex, PipeSet pipeSet)
        {
            // When a pipe is determined to be reset,
            // get its next position by adding an offset to the position
            // of a pipe which index is before itself.
            var prevPipeSetIndex = pipeSetIndex - 1;

            if (prevPipeSetIndex < 0)
                prevPipeSetIndex = pipeSetList.Count - 1;

            var nextPosX = pipeSetList[prevPipeSetIndex].ScrollPos + GapBetweenPipe;

            pipeSet.ResetPipe(nextPosX);
        }

        public void DrawBackgroundParallax(GameTime gameTime)
        {
            spriteBatch.Begin();
            foreach (var pallaraxBackground in backgroundParallax)
            {
                pallaraxBackground.DrawSprite(spriteBatch);
            }
            spriteBatch.End();
        }

        /// <summary>
        /// Resets states of agent, pipe-set and score.
        /// </summary>
        public void Reset()
        {
            agent.ResetAgent();
            foreach (var pipeSet in pipeSetList)
            {
                pipeSet.ResetPipe();
            }
            Score = 0;
        }

        public void StartMainMenuMode()
        {
            Reset();
            IsGameUpdating = false;
            agent.IsUpdating = false;
            EnableAllParallaxesUpdate(true);
        }

        public void StartPlayMode()
        {
            Reset();
            IsGameUpdating = true;
            agent.IsUpdating = true;
            EnableAllParallaxesUpdate(true);
            agent.ResetAgent(Agent.AgentState.Alive);
        }

        public void StartGameOverMode()
        {
            IsGameUpdating = false;
            agent.IsUpdating = false;
            EnableAllParallaxesUpdate(false);
        }

        private void CreatePipe(Entity pipeEntity, float startPosX, string name)
        {
            var pipe = new PipeSet(pipeEntity, VirtualResolution, -GameSpeed, startPosX, name);
            pipeSetList.Add(pipe);
        }

        private void EnableAllParallaxesUpdate(bool isEnable)
        {
            foreach (var pallarax in backgroundParallax)
            {
                pallarax.IsUpdating = isEnable;
            }
        }
    }
}
