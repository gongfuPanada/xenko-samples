using System;
using SiliconStudio.Core.Mathematics;

namespace SimpleTerrain
{
    /// <summary>
    /// A factory to create HeightMap algorithmically.
    /// It contains an algorithm to generate heightmap: Fault Formation.
    /// </summary>
    public static class HeightMapFactory
    {
        /// <summary>
        /// Creates HeightData using Fault Formation algorithm.
        /// Produces smooth terrain by adding a random line to a blank height field, and add random height to one of the two sides.
        /// </summary>
        /// <param name="size">Size of square HeightData to be created</param>
        /// <param name="numberIteration">The number of iteration of the algorithm. More iteration yields more spaces</param>
        /// <param name="minHeight">Min value of height produced from the algorithm</param>
        /// <param name="maxHeight">Max value of height produced from the algorithm</param>
        /// <param name="scaleHeight"></param>
        /// <param name="filter"></param>
        /// <returns>HeightData created with Fault Formation algorithm that has "size" of size</returns>
        public static HeightMap CreateDataWithFaultFormation(int size, int numberIteration, float minHeight, float maxHeight, float scaleHeight, float filter)
        {
            var data = new HeightMap(size) { ScaleHeight = scaleHeight };

            var random = new Random();

            for(var i = 0 ; i < numberIteration ; ++i)
            {
                // Calculate height for this iteration
                var height = maxHeight - (maxHeight - minHeight) * i / numberIteration;
                var currentPassHeight = height * ((float)random.NextDouble() - 0.1f);

                // Find the line mark a half space for this iteration
                var point1 = new Point(random.Next( size ), random.Next( size ));
                var point2 = new Point(random.Next( size ), random.Next( size ));

                var halfSpaceLineVector = new Vector2(point2.X - point1.X, point2.Y - point1.Y);

                for (var iX = 0; iX < size; ++iX)
                {
                    for (var iZ = 0; iZ < size; ++iZ)
                    {
                        var currentPointLine = new Vector2(iX - point1.X, iZ - point1.Y);

                        float sign;
                        Vector2.Dot(ref halfSpaceLineVector, ref currentPointLine, out sign);

                        if (sign > 0) data[iX, iZ] += currentPassHeight;
                    }
                }

                FilterHeightField(data, filter);
            }

            NormalizeHeightMap(data);

            return data;
        }

        private static void NormalizeHeightMap(HeightMap heightMap)
        {
            var maxHeight = float.MinValue;
            var minHeight = float.MaxValue;

            for (var i = 0; i < heightMap.DataSize; ++i)
            {
                if (maxHeight < heightMap[i]) maxHeight = heightMap[i];
                if (minHeight > heightMap[i]) minHeight = heightMap[i];
            }

            maxHeight -= minHeight;

            for (var i = 0; i < heightMap.DataSize; ++i)
                heightMap[i] = (heightMap[i] - minHeight) / maxHeight;
        }

        private static void FilterHeightField(HeightMap data, float filter)
        {
            var size = data.Size;
            // Erode left to right   
            for (var i = 0; i < size; i++)
                FilterHeightBand(data, size * i, 1, size, filter);

            //erode right to left   
            for (var i = 0; i < size; i++)
                FilterHeightBand(data, size * i + size - 1, -1, size, filter);

            //erode top to bottom   
            for (var i = 0; i < data.Size; i++)
                FilterHeightBand(data, i, size, size, filter);

            //erode from bottom to top   
            for (var i = 0; i < data.Size; i++)
                FilterHeightBand(data, size * (size - 1) + i, -size, size, filter);  
        }

        /// <summary>
        /// Filters HeightData using band-based filter. by "Jason Shankel"
        /// It simulates terrain erosion.
        /// Throws an ArgumentOutOfRangeException if heightData is empty. 
        /// </summary>
        /// <param name="heightData">In: Already created HeightData, Out: Smoothed HeightData</param>
        /// <param name="startIndex"></param>
        /// <param name="stride"></param>
        /// <param name="count"></param>
        /// <param name="filter"></param>
        public static void FilterHeightBand(HeightMap heightData, int startIndex, int stride, int count, float filter)
        {
            var v = heightData[startIndex];
            var j = stride;

            //go through the height band and apply the erosion filter   
            for (var i = 0; i < count - 1; ++i)
            {
                heightData[startIndex + j] = filter * v + (1 - filter) * heightData[startIndex + j];

                v = heightData[startIndex + j];
                j += stride;
            }   
        }
    }

    /// <summary>
    /// A container that represents HeightData which have the indexer for getting and setting height for the specific point, and gets the size of the data.
    /// </summary>
    public class HeightMap
    {
        public float this[int x, int z]
        {
            get { return data[x*Size + z]; }
            set { data[x*Size + z] = value; }
        }

        public float this[int x]
        {
            get { return data[x]; }
            set { data[x] = value; }
        }

        public int Size { get; private set; }
        public int DataSize { get { return data.Length; } }

        public float ScaleHeight = 1f;

        private readonly float[] data;

        public HeightMap(int size)
        {
            if (!IsPowerOfTwo(size))
                throw new ArgumentException("Size of Terrain must be a power of two");

            data = new float[size * size];
            Size = size;    
        }

        public float GetScaledHeight(int x, int z)
        {
            return ScaleHeight * this[x, z];
        }

        public static bool IsPowerOfTwo(int value) { return (value != 0) && ((value & (value - 1)) == 0); }
    }
}
