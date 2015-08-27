using System;
using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Animations;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Rendering.Sprites;

namespace SpriteEntity
{
    public class EnemyScript : SyncScript
    {
        public LogicScript Logic;

        private const float enemyInitPositionY = 8;

        // enemy age
        private const float enemyTimeToLive = 2.4f;   // seconds
        private const float enemyTimeToWait = -2f;    // seconds
        private float enemyAge;
        // enemy position
        private const float enemyDownSpeed = 8f;
        private const float floorPosiionY = 0f;
        private const float gameWidthX = 16f;       // from -8f to 8f
        private const float gameWidthHalfX = gameWidthX / 2f;
        // enemy animation
        private const float enemyActiveFps = 2f;
        private const float enemyBlowupFps = 18f;
        private SpriteComponent enemySpriteComponent;
        private SpriteSheet spriteSheet;

        // random
        private static int seed = Environment.TickCount;
        private static Random enemyRandomLocal = new Random(seed);

        private float elapsedTime;

        internal bool IsAlive { get; set; }

        public override void Start()
        {
            spriteSheet = Asset.Load<SpriteSheet>("SpriteSheet");

            // Register ourself to the logic to detect collision
            Logic.WatchEnemy(Entity);

            enemySpriteComponent = Entity.Get<SpriteComponent>();

            Reset();
        }

        public override void Update()
        {
            elapsedTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;
            enemyAge += elapsedTime;

            // Wait for the appearing
            if (enemyAge < 0f)
                return;

            if (enemyAge >= enemyTimeToLive)
            {
                // Die
                Reset();
                return;
            }

            if (!IsAlive)
            {
                // Let the explosion animation play
                return;
            }

            // Moving
            Entity.Transform.Position.Y -= enemyDownSpeed * elapsedTime;
            if (Entity.Transform.Position.Y <= floorPosiionY) Entity.Transform.Position.Y = floorPosiionY;
        }

        private void Reset()
        {
            IsAlive = true;
            Entity.Transform.Position.Y = enemyInitPositionY;

            var random = enemyRandomLocal;
            // Appearance position
            Entity.Transform.Position.X = (((float)(random.NextDouble())) * gameWidthX) - gameWidthHalfX;
            // Waiting time
            enemyAge = enemyTimeToWait - (((float)(random.NextDouble())));

            enemySpriteComponent.SpriteProvider = new SpriteFromSheet { Sheet = spriteSheet };
            SpriteAnimation.Play(enemySpriteComponent, spriteSheet.FindImageIndex("active0"), spriteSheet.FindImageIndex("active1"), AnimationRepeatMode.LoopInfinite, enemyActiveFps);
        }

        public void Explode()
        {
            IsAlive = false;
            enemySpriteComponent.SpriteProvider = new SpriteFromSheet { Sheet = spriteSheet };
            SpriteAnimation.Play(enemySpriteComponent, spriteSheet.FindImageIndex("blowup0"), spriteSheet.FindImageIndex("blowup7"), AnimationRepeatMode.LoopInfinite, enemyBlowupFps);
            enemyAge = enemyTimeToLive - 0.3f;
        }

        public RectangleF GetBoundingBox()
        {
            var result = spriteSheet.Sprites.First().Region;
            result.Width *= LogicScript.ScreenScale;
            result.Height *= LogicScript.ScreenScale;
            return result;
        }
    }
}
