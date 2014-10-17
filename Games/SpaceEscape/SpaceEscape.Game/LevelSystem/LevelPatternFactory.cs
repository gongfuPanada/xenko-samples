using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.EntityModel;

namespace SpaceEscape.LevelSystem
{
    /// <summary>
    /// LevelPatternFactory constructs and stores all level
    ///  patterns which can, then, be requested.
    /// </summary>
    public class LevelPatternFactory
    {
        private enum BgKeys
        {
            bg_a00,
            bg_b00,
            bg_b01,
            bg_b02,
            bg_b03,
            bg_b04
        }

        private enum ObstacleKeys
        {
            obj00,
            obj01
        }

        #region BackgroundEntity
        private static readonly Dictionary<BgKeys, BackgroundEntity> BgEntityDict =
            new Dictionary<BgKeys, BackgroundEntity>
        {
            {BgKeys.bg_a00, new BackgroundEntity
                {
                    MaxNbObstacles = 3
                }},
            {BgKeys.bg_b00, new BackgroundEntity
                {
                    MaxNbObstacles = 1
                }},
            {BgKeys.bg_b01, new BackgroundEntity
                {
                    MaxNbObstacles = 1,
                    Holes = new List<Hole>
                    {
                        new Hole
                        {
                            //Area = new RectangleF(-250, 1000, 500, 2000),
                            Area = new RectangleF(-250, 1500, 500, 3000),
                            Height = -200f
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
                            Area = new RectangleF(-750, 1500, 500, 3000),
                            Height = -200f
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
                            Area = new RectangleF(-750, 1500, 500, 3000),
                            Height = -200f
                        },
                        new Hole
                        {
                            Area = new RectangleF(250, 1500, 500, 3000),
                            Height = -200f
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
                            Area = new RectangleF(-750, 1500, 1000, 3000),
                            Height = -200f
                        }
                    }
                }},
        };
        #endregion BackgroundEntity

        private readonly Random random;
        private readonly Dictionary<ObstacleKeys, Entity> obstEntityDict;

        public LevelPatternFactory(Random random)
        {
            this.random = random;
            obstEntityDict = new Dictionary<ObstacleKeys, Entity>();
        }

        /// <summary>
        /// Load and cache Background and Obstacle entities.
        /// </summary>
        /// <param name="assetManager"></param>
        /// <returns></returns>
        public void LoadContent(AssetManager assetManager)
        {
            LoadBgEntities(assetManager);
            LoadObstacleEntities(assetManager);
        }

        /// <summary>
        /// Randomly create pattern from defined Factory method of each one.
        /// </summary>
        /// <returns></returns>
        public LevelBlock RandomCreateLevelBlock()
        {
            var bgKeysArr = Enum.GetValues(typeof(BgKeys));
            var bgKey = (BgKeys)bgKeysArr.GetValue(random.Next(bgKeysArr.Length));

            var bgEntity = GetBackgroundEntity(bgKey);
            return CreateLevelBlock(bgEntity);
        }

        public LevelBlock CreateSafeLevelBlock()
        {
            var safeBackground = GetBackgroundEntity(BgKeys.bg_a00);
            var levelBlock = new LevelBlock();
            levelBlock.AddBackgroundEntity(safeBackground.Entity);
            return levelBlock;
        }

        private void LoadObstacleEntities(AssetManager assetManager)
        {
            var obstKeys = Enum.GetValues(typeof(ObstacleKeys));
            foreach (var obstKeyObj in obstKeys)
            {
                var obstKey = (ObstacleKeys)obstKeyObj;
                obstEntityDict[obstKey] = assetManager.Load<Entity>(obstKey.ToString());
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
                BgEntityDict[bgKey].Entity = assetManager.Load<Entity>(bgKey.ToString());
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
        private Entity RandomPickObstEnt()
        {
            var values = Enum.GetValues(typeof(ObstacleKeys));
            var obstKey = (ObstacleKeys)values.GetValue(random.Next(values.Length));

            return obstEntityDict[obstKey].Clone();
        }

        /// <summary>
        /// Randomly add obstacles to the given level block, while
        /// the number of obstacles to be added is from nbObst.
        /// </summary>
        /// <param name="levelBlock"></param>
        /// <param name="patternLen"></param>
        /// <param name="nbObst"></param>
        private void RandomAddObstacles(LevelBlock levelBlock, float patternLen, int nbObst)
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
                var posX = (1 - lane) * 500f;

                // Randomly get the obstacle, and set the position of this obstacle.
                var obsEnt = RandomPickObstEnt();
                obsEnt.Transformation.Translation = new Vector3(posX, 0, posZ);

                levelBlock.AddObstacleEntity(obsEnt);
            }
        }

        /// <summary>
        /// Factory method to create LevelBlock from a given BackgroundEntity
        /// </summary>
        /// <param name="backgroundEnt"></param>
        /// <returns></returns>
        private LevelBlock CreateLevelBlock(BackgroundEntity backgroundEnt)
        {
            var levelBlock = new LevelBlock();

            levelBlock.AddBackgroundEntity(backgroundEnt.Entity).AddHoleRange(backgroundEnt.Holes);

            var len = levelBlock.Length;
            RandomAddObstacles(levelBlock, len, backgroundEnt.MaxNbObstacles);

            return levelBlock;
        }
    }

    /// <summary>
    /// Represents a background.
    /// A background is composed of: Holes (Optional), Entity (Entity of this background),
    ///  and max number of Obstacles that could be placed in this background.
    /// </summary>
    internal class BackgroundEntity
    {
        public List<Hole> Holes { get; set; }
        public int MaxNbObstacles { get; set; }
        public Entity Entity { get; set; }
    }
}
