using System;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Animations;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Rendering.Sprites;

namespace SpriteEntity
{
    public class EnemyScript : AsyncScript
    {

        public LogicScript Logic;

        // enemy age
        private const float enemyTimeToLive = 2.4f;   // seconds
        private const float enemyTimeToWait = -2f;    // seconds
        private float enemyAge;
        // enemy position
        private const float enemyDownSpeed = 8f;
        private const float floorPosiionY = 0f;
        private const float gameWidthX = 16f;       // from -8f to 8f
        private const float gameWidthHalfX = gameWidthX / 2f;
        private float enemyInitPositionY;
        // enemy animation
        private const float enemyActiveFps = 2f;
        private const float enemyBlowupFps = 18f;
        private SpriteComponent enemySpriteComponent;
        private SpriteGroup enemyNormalSprite;
        private SpriteGroup enemyExplosionSprite;

        // random
        private static int seed = Environment.TickCount;
        private static Random enemyRandomLocal = new Random(seed);


        private float elapsedTime;

        internal bool IsAlive { get; set; }

        public override async Task Execute()
        {

            enemyNormalSprite = Asset.Load<SpriteGroup>("enemy_active");
            enemyExplosionSprite = Asset.Load<SpriteGroup>("enemy_blowup");

            // Register ourself to the logic to detect collision
            Logic.WatchEnemy(Entity);

            enemyInitPositionY = Entity.Transform.Position.Y;
            enemySpriteComponent = Entity.Get<SpriteComponent>();
            
            reset();

            while (Game.IsRunning)
            {
                await Script.NextFrame();

                elapsedTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;
                enemyAge += elapsedTime;

                // Wait for the appearing
                if (enemyAge < 0f) continue;

                if (enemyAge >= enemyTimeToLive)
                {
                    // Die
                    reset();
                    continue;
                }

                if (!IsAlive)
                {
                    // Let the explosion animation play
                    continue;
                }

                // Moving
                Entity.Transform.Position.Y -= enemyDownSpeed * elapsedTime;
                if (Entity.Transform.Position.Y <= floorPosiionY) Entity.Transform.Position.Y = floorPosiionY;
            }
        }

        private void reset()
        {
            IsAlive = true;
            Entity.Transform.Position.Y = enemyInitPositionY;

            var random = enemyRandomLocal;
            // Appearance position
            Entity.Transform.Position.X = (((float)(random.NextDouble())) * gameWidthX) - gameWidthHalfX;
            // Waiting time
            enemyAge = enemyTimeToWait - (((float)(random.NextDouble())));

            enemySpriteComponent.SpriteProvider = new SpriteFromSpriteGroup() { SpriteGroup = enemyNormalSprite };
            SpriteAnimation.Play(enemySpriteComponent, 0, enemySpriteComponent.SpriteProvider.SpritesCount - 1, AnimationRepeatMode.LoopInfinite, enemyActiveFps);
        }

        public void Explode()
        {
            IsAlive = false;
            enemySpriteComponent.SpriteProvider = new SpriteFromSpriteGroup() { SpriteGroup = enemyExplosionSprite };
            SpriteAnimation.Play(enemySpriteComponent, 0, enemySpriteComponent.SpriteProvider.SpritesCount - 1, AnimationRepeatMode.LoopInfinite, enemyBlowupFps);
            enemyAge = enemyTimeToLive - 0.3f;
        }

        public RectangleF GetBoundingBox()
        {
            var result = enemyNormalSprite.Images.First().Region;
            result.Width *= LogicScript.ScreenScale;
            result.Height *= LogicScript.ScreenScale;
            return result;
        }
    }
}
