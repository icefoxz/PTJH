using UnityEngine;

namespace Test
{
    public class TestRes : MonoBehaviour
    {
        public void TestWindowLoad() =>
            Game.Res.Instantiate("TestWindow.prefab", Game.SceneCanvas.transform, obj => Debug.Log($"{obj}已加载！"));

        public void TestResourcesLoad()
        {
            Game.Res.Initialize(() => Debug.Log($"{nameof(TestResourcesLoad)}!"));
        }

        //public void HotFixPack()
        //{
        //    string assetBundleDirectory = "Assets/AssetBundles";
        //    if (!Directory.Exists(assetBundleDirectory))
        //    {
        //        Directory.CreateDirectory(assetBundleDirectory);
        //    }

        //    var manifest =
        //        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.Android);
        //}
    }
}
