using SiliconStudio.Paradox.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteStudioDemo
{
    public class EnemyCollisionScript : AsyncScript
    {
        public override async Task Execute()
        {
            var rigidbodyElement = Entity.Get<PhysicsComponent>()[0];
            var enemyScript = (EnemyScript)Entity.Get<ScriptComponent>().Scripts.First(x => x.Name == "EnemyScript");

            while (Game.IsRunning)
            {
                var collision = await rigidbodyElement.RigidBody.NewCollision();

                if (collision.ColliderA.Entity.Name == "bullet" && !rigidbodyElement.RigidBody.IsTrigger) //if we are trigger we should ignore the bullet
                {
                    var script = (BeamScript)collision.ColliderA.Entity.Get<ScriptComponent>().Scripts[0];
                    script.Die();
                    enemyScript.Explode();
                }
                else if (collision.ColliderB.Entity.Name == "bullet" && !rigidbodyElement.RigidBody.IsTrigger)
                {
                    var script = (BeamScript)collision.ColliderB.Entity.Get<ScriptComponent>().Scripts[0];
                    script.Die();
                    enemyScript.Explode();
                }
            }
        }
    }
}
