using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Animations;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Engine;

namespace SpaceEscape.Background
{
    /// <summary>
    /// SectionsFactory constructs and stores all level bock which can, then, be requested.
    /// </summary>
    public class SectionsFactory
    {
        private class ObstacleDesc
        {
            public readonly string Model;
            public readonly string Animation;
            public readonly bool UseSubMeshBoundingBoxes;
            public Entity Entity;
            
            public ObstacleDesc(string model, string animation, bool useSubMeshBoundingBoxes)
            {
                Model = model;
                Animation = animation;
                UseSubMeshBoundingBoxes = useSubMeshBoundingBoxes;
                Entity = null;
            }
        }

        private enum BgKeys
        {
            bg_a00,
            bg_b00,
            bg_b01,
            bg_b02,
            bg_b03,
            bg_b04
        }

        private readonly ObstacleDesc[] obstacles = { new ObstacleDesc("obj00", null, true), new ObstacleDesc("obj01", "obj01_Anim", false) };

        private static readonly Dictionary<BgKeys, BackgroundEntity> BgEntityDict = new Dictionary<BgKeys, BackgroundEntity>
        {
            {BgKeys.bg_a00, new BackgroundEntity { MaxNbObstacles = 3 }},
            {BgKeys.bg_b00, new BackgroundEntity { MaxNbObstacles = 1 }},
            {BgKeys.bg_b01, new BackgroundEntity 
            { 
                MaxNbObstacles = 1, 
                    Holes = new List<Hole>
                    {
                        new Hole
                        {
                            //Area = new RectangleF(-250, 1000, 500, 2000),
                            Area = new RectangleF(-2.50f, 15.00f, 5.00f, 30.00f),
                            Height = -2.00f
                        }
                    }
                }},
            {BgKeys.bg_b02, new BackgroundEntity
                {
                    MaxNbObstacles = 1,
                    Holes = new List<Hole>
                    {
                        new Hole
                        {
                            Area = new RectangleF(-7.50f, 15.00f, 5.00f, 30.00f),
                            Height = -2.00f
                        }
                    }
                }},
            {BgKeys.bg_b03, new BackgroundEntity
                {
                    MaxNbObstacles = 0,
                    Holes = new List<Hole>
                    {
                        new Hole
                        {
                            Area = new RectangleF(-7.50f, 15.00f, 5.00f, 30.00f),
                            Height = -2.00f
                        },
                        new Hole
                        {
                            Area = new RectangleF(2.50f, 15.00f, 5.00f, 30.00f),
                            Height = -2.00f
                        }
                    }
                }},
            {BgKeys.bg_b04, new BackgroundEntity
                {
                    MaxNbObstacles = 0,
                    Holes = new List<Hole>
                    {
                        new Hole
                        {
                            Area = new RectangleF(-7.50f, 15.00f, 10.00f, 30.00f),
                            Height = -2.00f
                        }
                    }
                }},
        };

        private readonly Random random;

        public SectionsFactory(Random random)
        {
            this.random = random;
        }

        /// <summary>
        /// Load and cache Background and Obstacle entities.
        /// </summary>
        /// <param name="assetManager"></param>
        /// <returns></returns>
        public void LoadContent(AssetManager assetManager)
        {
            LoadBgEntities(assetManager);
            CreateObstacleEntities(assetManager);
        }

        /// <summary>
        /// Randomly create pattern from defined Factory method of each one.
        /// </summary>
        /// <returns></returns>
        public Section RandomCreateLevelBlock()
        {
            var bgKeysArr = Enum.GetValues(typeof(BgKeys));
            var bgKey = (BgKeys)bgKeysArr.GetValue(random.Next(bgKeysArr.Length));

            var bgEntity = GetBackgroundEntity(bgKey);
            return CreateLevelBlock(bgEntity);
        }

        public Section CreateSafeLevelBlock()
        {
            var safeBackground = GetBackgroundEntity(BgKeys.bg_a00);
            var levelBlock = new Section();
            levelBlock.AddBackgroundEntity(safeBackground.Entity);
            return levelBlock;
        }

        private void CreateObstacleEntities(AssetManager assetManager)
        {
            foreach (var obstacle in obstacles)
            {
                var entity = new Entity();
                var model = assetManager.Load<Model>(obstacle.Model);
                entity.Add(new ModelComponent(model));
                if (!string.IsNullOrEmpty(obstacle.Animation))
                {
                    var anim = assetManager.Load<AnimationClip>(obstacle.Animation);
                    entity.Add(new AnimationComponent { Animations = { { PlayIdleAnimationScript.AnimationName, anim } } });
                    entity.Add(new ScriptComponent { Scripts = { new PlayIdleAnimationScript() } });
                }
                obstacle.Entity = entity;
            }
        }

        /// <summary>
        /// Load Background entities from predefined keys,
        /// and also get defined holes for that bg.
        /// </summary>
        /// <param name="assetManager"></param>
        /// <returns></returns>
        private static void LoadBgEntities(AssetManager assetManager)
        {
            var bgKeys = Enum.GetValues(typeof(BgKeys));
            foreach (var bgKeyObj in bgKeys)
            {
                var bgKey = (BgKeys)bgKeyObj;
                BgEntityDict[bgKey].Entity = new Entity { new ModelComponent(assetManager.Load<Model>(bgKey.ToString())) };
            }
        }

        /// <summary>
        /// Get as master BackgroundEntity, and
        /// copy its field {Entity, Hole and Maxnumber of Obstacle}
        /// Note that Entity has to be cloned properly.
        /// </summary>
        /// <param name="bgKey"></param>
        /// <returns></returns>
        private static BackgroundEntity GetBackgroundEntity(BgKeys bgKey)
        {
            var masterBackgroundEntity = BgEntityDict[bgKey];

            return new BackgroundEntity
                {
                    Entity = masterBackgroundEntity.Entity.Clone(),
                    Holes = masterBackgroundEntity.Holes,
                    MaxNbObstacles = masterBackgroundEntity.MaxNbObstacles
                };
        }

        /// <summary>
        /// Randomly get Obstacle from available keys and return the clone.
        /// </summary>
        /// <returns></returns>
        private Entity CloneRandomObstacle(out bool useSubmeshesBoundingBoxes)
        {
            var obstacle = obstacles[random.Next(obstacles.Length)];
            useSubmeshesBoundingBoxes = obstacle.UseSubMeshBoundingBoxes;
            return obstacle.Entity.Clone();
        }

        /// <summary>
        /// Randomly add obstacles to the given level block, while
        /// the number of obstacles to be added is from nbObst.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="patternLen"></param>
        /// <param name="nbObst"></param>
        private void RandomAddObstacles(Section section, float patternLen, int nbObst)
        {
            var halfPatternLen = patternLen / 2f;

            for (var i = 0; i < nbObst; i++)
            {
                // Random val in {0.0-1.0}
                var randVal = random.NextDouble();

                // Calculate position in Z axis by:
                // changing the value in uniform space (randVal) to world space.
                // the value is substracted with halfPatternLen because the origin (0) is at the center of the block.
                var posZ = patternLen * (float)((i + randVal) / nbObst) - halfPatternLen;

                // Random lane, and get the world position in X axis.
                var lane = random.Next(3);
                var posX = (1 - lane) * 5f;

                // Randomly get the obstacle, and set the position of this obstacle.
                bool useSubBoundingBoxes;
                var obsEnt = CloneRandomObstacle(out useSubBoundingBoxes);
                obsEnt.Transform.Position = new Vector3(posX, 0, posZ);

                section.AddObstacleEntity(obsEnt, useSubBoundingBoxes);
            }
        }

        /// <summary>
        /// Factory method to create Section from a given BackgroundEntity
        /// </summary>
        /// <param name="backgroundEnt"></param>
        /// <returns></returns>
        private Section CreateLevelBlock(BackgroundEntity backgroundEnt)
        {
            var levelBlock = new Section();

            levelBlock.AddBackgroundEntity(backgroundEnt.Entity).AddHoleRange(backgroundEnt.Holes);

            var len = levelBlock.Length;
            RandomAddObstacles(levelBlock, len, backgroundEnt.MaxNbObstacles);

            return levelBlock;
        }

        /// <summary>
        /// Represents a background.
        /// A background is composed of: Holes (Optional), Entity (Entity of this background),
        ///  and max number of Obstacles that could be placed in this background.
        /// </summary>
        private class BackgroundEntity
        {
            public List<Hole> Holes { get; set; }
            public int MaxNbObstacles { get; set; }
            public Entity Entity { get; set; }
        }
    }
}
