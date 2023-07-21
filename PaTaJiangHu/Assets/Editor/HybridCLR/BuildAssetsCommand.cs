using HybridCLR.Editor.Commands;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HybridCLR.Editor
{
    public static class BuildAssetsCommand
    {
        // 这个是自己添加的, 作为热更新的dll转成bytes导出的目录以方便管理热更新+Addressables
        public static string AbAssetsPath => Application.dataPath + "/AbAssets";

        public static string HybridCLRBuildCacheDir => Application.dataPath + "/HybridCLRBuildCache";

        public static string AssetBundleOutputDir => $"{HybridCLRBuildCacheDir}/AssetBundleOutput";

        public static string AssetBundleSourceDataTempDir => $"{HybridCLRBuildCacheDir}/AssetBundleSourceData";


        public static string GetAssetBundleOutputDirByTarget(BuildTarget target)
        {
            return $"{AssetBundleOutputDir}/{target}";
        }

        public static string GetAssetBundleTempDirByTarget(BuildTarget target)
        {
            return $"{AssetBundleSourceDataTempDir}/{target}";
        }

        public static string ToRelativeAssetPath(string s)
        {
            return s.Substring(s.IndexOf("Assets/"));
        }

        /// <summary>
        /// 将HotFix.dll和HotUpdatePrefab.prefab打入common包.
        /// 将HotUpdateScene.unity打入scene包.
        /// </summary>
        /// <param name="tempDir"></param>
        /// <param name="outputDir"></param>
        /// <param name="target"></param>
        private static void BuildAssetBundles(string tempDir, string outputDir, BuildTarget target)
        {
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(outputDir);
            
            List<AssetBundleBuild> abs = new List<AssetBundleBuild>();

            {
                var prefabAssets = new List<string>();
                string testPrefab = $"{Application.dataPath}/Prefabs/Cube.prefab";
                prefabAssets.Add(testPrefab);
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                abs.Add(new AssetBundleBuild
                {
                    assetBundleName = "prefabs",
                    assetNames = prefabAssets.Select(s => ToRelativeAssetPath(s)).ToArray(),
                });
            }

            BuildPipeline.BuildAssetBundles(outputDir, abs.ToArray(), BuildAssetBundleOptions.None, target);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        public static void BuildAssetBundleByTarget(BuildTarget target)
        {
            BuildAssetBundles(GetAssetBundleTempDirByTarget(target), GetAssetBundleOutputDirByTarget(target), target);
        }

        [MenuItem("HybridCLR/Build/BuildAssetsAndCopyToStreamingAssets")]
        public static void BuildAndCopyABAOTHotUpdateDlls()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            BuildAssetBundleByTarget(target);
            CompileDllCommand.CompileDll(target);
            CopyABAOTHotUpdateDlls(target);
        }

        public static void CopyABAOTHotUpdateDlls(BuildTarget target)
        {
            CopyAssetBundlesToStreamingAssets(target);
            CopyAOTAssembliesToStreamingAssets();
            CopyHotUpdateAssembliesToStreamingAssets();
        }


        //[MenuItem("HybridCLR/Build/BuildAssetbundle")]
        public static void BuildSceneAssetBundleActiveBuildTargetExcludeAOT()
        {
            BuildAssetBundleByTarget(EditorUserBuildSettings.activeBuildTarget);
        }

        public static void CopyAOTAssembliesToStreamingAssets()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            string aotAssembliesSrcDir = Path.GetFullPath(
                Application.dataPath + @"\../" + SettingsUtil.GetAssembliesPostIl2CppStripDir(target));
            string aotAssembliesDstDir = Application.streamingAssetsPath;

            foreach (var dll in SettingsUtil.AOTAssemblyNames)
            {
                string srcDllPath = $"{aotAssembliesSrcDir}/{dll}.dll";
                if (!File.Exists(srcDllPath))
                {
                    Debug.LogError($"ab中添加AOT补充元数据dll:{srcDllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先构建一次游戏App后再打包。");
                    continue;
                }

                var dllByte = $"{dll}.dll.bytes";
                string dllBytesPath = $"{aotAssembliesDstDir}/{dllByte}";
                File.Copy(srcDllPath, dllBytesPath, true);
                CopyToAddressableDllPath(srcDllPath, dllByte);
                Debug.Log($"[CopyAOTAssembliesToStreamingAssets] copy AOT dll {srcDllPath} -> {dllBytesPath}");
            }
        }

        public static void CopyHotUpdateAssembliesToStreamingAssets()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;

            string hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
            string hotfixAssembliesDstDir = Application.streamingAssetsPath;
            foreach (var dll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
            {
                string dllPath = $"{hotfixDllSrcDir}/{dll}";
                var dllBytes = $"{dll}.bytes";
                string dllBytesPath = $"{hotfixAssembliesDstDir}/{dllBytes}";
                File.Copy(dllPath, dllBytesPath, true);
                Debug.Log($"[CopyHotUpdateAssembliesToStreamingAssets] copy hotfix dll {dllPath} -> {dllBytesPath}");
                var abAssetsPath = $"{AbAssetsPath}/{dllBytes}";
                File.Copy(dllPath, abAssetsPath, true);
                Debug.Log($"[CopyHotUpdateAssembliesToAbAssets] copy hotfix dll {dllPath} -> {dllBytesPath}");
                CopyToAddressableDllPath(abAssetsPath, dllBytes);
            }
        }

        // 附加路径(Addressables)
        private static void CopyToAddressableDllPath(string sourcePath, string dllBytes)
        {
            var abAssetsPath = $"{AbAssetsPath}/Dlls/{dllBytes}";
            File.Copy(sourcePath, abAssetsPath, true);
            Debug.Log($"[CopyToAddressableDllPath] copy dll {sourcePath} -> {abAssetsPath}");
        }

        public static void CopyAssetBundlesToStreamingAssets(BuildTarget target)
        {
            string streamingAssetPathDst = Application.streamingAssetsPath;
            Directory.CreateDirectory(streamingAssetPathDst);
            string outputDir = GetAssetBundleOutputDirByTarget(target);
            var abs = new string[] { };
            foreach (var ab in abs)
            {
                string srcAb = ToRelativeAssetPath($"{outputDir}/{ab}");
                string dstAb = ToRelativeAssetPath($"{streamingAssetPathDst}/{ab}");
                Debug.Log($"[CopyAssetBundlesToStreamingAssets] copy assetBundle {srcAb} -> {dstAb}");
                AssetDatabase.CopyAsset(srcAb, dstAb);
                //var srcAbPath = $"{outputDir}/{ab}";
                //var dstAbPath = $"{streamingAssetPathDst}/{ab}";
                //File.Copy(srcAbPath, dstAbPath, true);
            }
        }
    }
}
