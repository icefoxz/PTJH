using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HybridCLR;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AOT
{
    public class LoadDll
    {
        private string _assemblyName = "HotUpdate";

        public LoadDll()
        {
        
        }
        public LoadDll(string assemblyName)
        {
            _assemblyName = assemblyName;
        }
        private async void LoadHotReload<T>(string key,Action<T> callbackAction)
        {
            var obj = await LoadHotReloadAsync<T>(key);
            callbackAction?.Invoke(obj);
        }
        private static async Task<T> LoadHotReloadAsync<T>(string key)
        {
            var handle = Addressables.LoadAssetAsync<T>(key);
            return await handle.Task;
        }

        public AsyncOperationHandle<TextAsset> LoadAssetTask() => Addressables.LoadAssetAsync<TextAsset>(_assemblyName);

        public void StartHotReloadAssembly(string className,string methodName)
        {
            //注意!!!! Editor环境下，HotUpdate.dll.bytes已经被自动加载，不需要加载，重复加载反而会出问题。
#if UNITY_EDITOR
            // Editor下无需加载，直接查找获得HotUpdate程序集
            Assembly hotUpdateAss =
                AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == _assemblyName);
            InvokeAssembly(hotUpdateAss, className, methodName);
#else
        LoadHotReload<TextAsset>(_assemblyName, o =>
        {
            Assembly hotUpdateAss = Assembly.Load(o.bytes);
            InvokeAssembly(hotUpdateAss, className, methodName);
        });
#endif
        }

        private static void InvokeAssembly(Assembly hotUpdateAss, string className, string methodName)
        {
            Type type = hotUpdateAss.GetTypes().FirstOrDefault(t => t.Name == className) ??
                        throw new NullReferenceException($"Type({className}) not found from [{hotUpdateAss.FullName}]");
            var method = type.GetMethod(methodName) ??
                         throw new NullReferenceException($"Method({methodName}) not found from [{className}]");
            method.Invoke(null, null);
        }

        private static bool IsAOTMetaDataLoaded = false;
        public static bool IsMetadataForAOTAssembliesLoaded { get; private set; }
        /// <summary>
        /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
        /// </summary>
        public static async void LoadMetadataForAOTAssemblies()
        {
            if (IsAOTMetaDataLoaded) return; // 只加载一次
            IsAOTMetaDataLoaded = true;
            List<string> aotMetaAssemblyFiles = new List<string>()
            {
                "AOT.dll",
                "GameClient.dll",
                "System.Core.dll",
                "UnityEngine.CoreModule.dll",
                "mscorlib.dll",
            };
            /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
            /// 
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            foreach (var aotDllName in aotMetaAssemblyFiles)
            {
                byte[] dllBytes = await ReadBytesFromStreamingAssets(aotDllName);
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
            }

            IsMetadataForAOTAssembliesLoaded = true;
            async Task<byte[]> ReadBytesFromStreamingAssets(string file)
            {
                // Android平台不支持直接读取StreamingAssets下文件，请自行修改实现
#if UNITY_EDITOR
                return await File.ReadAllBytesAsync($"{Application.streamingAssetsPath}/{file}.bytes");
#else
            var key = file.Replace(".dll", string.Empty);
            var asset = await LoadHotReloadAsync<TextAsset>(key);
            return asset.bytes;
#endif
            }
        }
    }
}