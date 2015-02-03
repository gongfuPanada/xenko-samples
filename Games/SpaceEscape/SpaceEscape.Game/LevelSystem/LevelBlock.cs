using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;

namespace SpaceEscape.LevelSystem
{
    /// <summary>
    /// LevelBlock holds a section of background-
    /// model which could contains other game objects.
    /// </summary>
    public class LevelBlock
    {
        public float Length { get; private set; }
        public Entity RootEntity { get; private set; }
        public Entity ModelEntity { get; private set; }

        public List<CollidableObstacle> CollidableObstacles{ get; private set; }
        public List<Hole> Holes{ get; private set; }

        public float PositionZ
        {
            get
            {
                return RootEntity.Transformation.Translation.Z;
            }
            set
            {
                RootEntity.Transformation.Translation.Z = value;
            }
        }

        public LevelBlock()
        {
            RootEntity = new Entity();
            ModelEntity = new Entity();
            CollidableObstacles = new List<CollidableObstacle>();
            Holes = new List<Hole>();

            RootEntity.Transformation.Children.Add(ModelEntity.Transformation);
        }

        public LevelBlock AddBackgroundEntity(Entity backgroundEntity)
        {
            // Attach  it in ModelEntity
            ModelEntity.Transformation.Children.Add(backgroundEntity.Transformation);

            // Get length via its boundingbox
            var modelComponent = backgroundEntity.Get<ModelComponent>().Model;
            var boundingBox = modelComponent.BoundingBox;

            Length += boundingBox.Maximum.Z - boundingBox.Minimum.Z;

            return this;
        }

        /// <summary>
        /// Chaining method for adding an obstacle to this LevelBlock.
        /// It initializes bounding boxes and stores in Collidable Obstacles.
        /// </summary>
        /// <param name="obstacleEntity"></param>
        /// <returns></returns>
        public LevelBlock AddObstacleEntity(Entity obstacleEntity)
        {
            // Attach it in ModelEntity
            ModelEntity.Transformation.Children.Add(obstacleEntity.Transformation);

            // Get and add bb to CollidableObstacles
            var modelComponent = obstacleEntity.Get<ModelComponent>().Model;

            var collidableObstacle = new CollidableObstacle
            {
                Entity = obstacleEntity,
            };

            // Note that for "obj00" use bounding boxes from parts of the obstacle.
            if (obstacleEntity.Name.Equals("obj00"))
            {
                foreach (var mesh in modelComponent.Meshes)
                {
                    collidableObstacle.BoundingBoxes.Add(mesh.BoundingBox);
                }
            }
            else
            {
                // For other obstacle use global bounding box obtained from model component.
                collidableObstacle.BoundingBoxes.Add(modelComponent.BoundingBox);
            }

            // Remove collision model (without materials)
            for (int index = 0; index < modelComponent.Meshes.Count; index++)
            {
                var mesh = modelComponent.Meshes[index];
                if (mesh.Material == null)
                    modelComponent.Meshes.RemoveAt(index--);
            }

            CollidableObstacles.Add(collidableObstacle);

            return this;
        }

        /// <summary>
        /// Add list of Holes to this LevelBlock.
        /// </summary>
        /// <param name="holes"></param>
        /// <returns></returns>
        public LevelBlock AddHoleRange(List<Hole> holes)
        {
            if(holes != null)
                Holes.AddRange(holes);

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnAttachedToWorld()
        {
            foreach (var animComp in CollidableObstacles.Select(collidableObstacle => collidableObstacle.Entity.Get<AnimationComponent>())
                .Where(animComp => animComp != null)
                .Where(animComp => animComp.Animations.ContainsKey("ObstAnim")))
            {
                animComp.Play("ObstAnim");
            }
        }
    }

    public class CollidableObstacle
    {
        public List<BoundingBox> BoundingBoxes = new List<BoundingBox>(); 
        public Entity Entity { get; set; }
    }

    public class Hole
    {
        public RectangleF Area { get; set; }
        public float Height { get; set; }
    }
}
