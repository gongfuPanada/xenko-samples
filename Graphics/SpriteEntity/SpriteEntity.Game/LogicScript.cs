using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Paradox.Engine;

namespace SpriteEntity
{
    /// <summary>
    /// Watches bullets and enemies and detects collisions.
    /// </summary>
    public class LogicScript : SyncScript
    {
        public const float ScreenScale = 0.00625f;

        private readonly List<Entity> bullets = new List<Entity>();
        private readonly List<Entity> enemies = new List<Entity>();

        public override void Update()
        {
            // For each bullet
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                var bullet = bullets[i];
                var bulletScriptComponent = bullets[i].Get<ScriptComponent>();
                // Get the bullet script (suppose there's only one script)
                var bulletScript = (BeamScript)bulletScriptComponent.Scripts.FirstOrDefault();

                var bulletRectangleCollider = bulletScript.GetBoundingBox();
                bulletRectangleCollider.X = (int)bullet.Transform.Position.X - bulletRectangleCollider.Width / 2;
                bulletRectangleCollider.Y = (int)bullet.Transform.Position.Y - bulletRectangleCollider.Height / 2;

                if (bulletScript.IsAlive)
                {
                    // Checks for collision with enemies
                    foreach (var enemy in enemies)
                    {
                        var enemyScriptComponent = enemy.Get<ScriptComponent>();
                        var enemyScript = (EnemyScript)enemyScriptComponent.Scripts.FirstOrDefault();

                        if (!enemyScript.IsAlive) continue;

                        var enemyRectangleCollider = enemyScript.GetBoundingBox();
                        enemyRectangleCollider.X = (int)enemy.Transform.Position.X - enemyRectangleCollider.Width / 2;
                        enemyRectangleCollider.Y = (int)enemy.Transform.Position.Y - enemyRectangleCollider.Height / 2;

                        if (!bulletRectangleCollider.Intersects(enemyRectangleCollider)) continue;

                        // Collision detected
                        bulletScript.IsAlive = false;
                        enemyScript.Explode();
                        break;
                    }

                }

                if (!bulletScript.IsAlive)
                {
                    // The bullet is dead, remove it
                    SceneSystem.SceneInstance.Scene.RemoveChild(bullet);
                    bullets.Remove(bullet);
                }
            }
        }

        public override void Cancel()
        {
            foreach (var bullet in bullets)
            {
                SceneSystem.SceneInstance.Scene.RemoveChild(bullet);
            }
        }

        /// <summary>
        /// Adds a bullet we will monitor for collisions.
        /// </summary>
        /// <param name="bullet"></param>
        public void WatchBullet(Entity bullet)
        {
            bullets.Add(bullet);
        }

        /// <summary>
        /// Adds a enemy to monitor
        /// </summary>
        /// <param name="enemy"></param>
        public void WatchEnemy(Entity enemy)
        {
            enemies.Add(enemy);
        }
    }
}
