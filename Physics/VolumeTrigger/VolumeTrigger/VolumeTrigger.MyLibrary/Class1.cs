using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Serialization.Contents;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VolumeTrigger.MyLibrary
{
    [DataContract("RangeValues")] // Specify that this classes is serializable
    [ContentSerializer(typeof(DataContentSerializer<RangeValues>))]  // Specify that this class is serializable through the asset manager
    public partial class RangeValues
    {
        public RangeValues()
        {
            Values = new List<float>();
        }

        public List<float> Values { get; set; }
    }

    [DataContract("RangeAsset")] // Name of the Asset serialized in YAML
    [AssetCompiler(typeof(RangeAssetCompiler))] // The compiler used to transform this asset to RangeValues
    [AssetDescription(".pdxrange")] // A description used to display in the asset editor
    [ObjectFactory(typeof(RangeAssetFactory))]
    public class RangeAsset : Asset
    {
        public float From { get; set; }
        public float To { get; set; }
        public float Step { get; set; }

        public RangeAsset()
            : base()
        {
        }

        private class RangeAssetFactory : IObjectFactory
        {
            public object New(Type type)
            {
                return new RangeAsset();
            }
        }
    }

    internal class RangeAssetCompiler : AssetCompilerBase<RangeAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, RangeAsset asset, AssetCompilerResult result)
        {
            result.BuildSteps = new AssetBuildStep(AssetItem)
            {
                new RangeAssetCommand(urlInStorage, asset)
            };
        }

        /// <summary>
        /// Command used by the build engine to convert the asset
        /// </summary>
        private class RangeAssetCommand : AssetCommand<RangeAsset>
        {
            public RangeAssetCommand(string url, RangeAsset asset)
                : base(url, asset)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new AssetManager();
                // Generate our data for in-game time
                var inGameAsset = new RangeValues();

                for (float index = 0; index <= 40; index += 1)
                    inGameAsset.Values.Add(index);
                // Save in-game asset
                assetManager.Save(Url, inGameAsset);
                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}