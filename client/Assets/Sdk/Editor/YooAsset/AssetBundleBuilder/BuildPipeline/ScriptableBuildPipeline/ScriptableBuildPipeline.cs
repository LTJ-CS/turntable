using System;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    public class ScriptableBuildPipeline : IBuildPipeline
    {
        public BuildResult Run(BuildParameters buildParameters, bool enableLog)
        {
            if (buildParameters is ScriptableBuildParameters)
            {
                AssetBundleBuilder builder = new AssetBundleBuilder();
                return builder.Run(buildParameters, GetAABuildPipeline(), enableLog);
            }
            else
            {
                throw new Exception($"Invalid build parameter type : {buildParameters.GetType().Name}");
            }
        }
        
        /// <summary>
        /// 获取 Addressable 构建流程
        /// </summary>
        /// <returns></returns>
        private List<IBuildTask> GetAABuildPipeline()
        {
            // 目前只使用 YooAsset 的资源查找算法, 再用于构建 AA 的 Group
            var pipeline = new List<IBuildTask>
                           {
                               new TaskPrepare_BBP(),
                               // new TaskGetBuildMap_BBP(),
                               new TaskBuildAddressablesGroup(),
                           };
            return pipeline;
        }

        /// <summary>
        /// 获取默认的构建流程
        /// </summary>
        private List<IBuildTask> GetDefaultBuildPipeline()
        {
            List<IBuildTask> pipeline = new List<IBuildTask>
                {
                    new TaskPrepare_SBP(),
                    new TaskGetBuildMap_SBP(),
                    new TaskVerifyBuildResult_SBP(),
                    new TaskEncryption_SBP(),
                    new TaskCreateReport_SBP(),
                    new TaskCreatePackage_SBP(),
                    new TaskCopyBuildinFiles_SBP(),
                };
            return pipeline;
        }
    }
}