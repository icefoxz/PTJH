using System;
using System.Threading.Tasks;
using UnityEngine;
using Views;

/// <summary>
/// <see cref="IView"/>标准的生成器
/// </summary>
public class UiBuilder
{
    /// <summary>
    /// 生成Ui
    /// </summary>
    /// <param name="resName">物体名字</param>
    /// <param name="buildAction">生成函数<see cref="IView"/>为传入物件</param>
    /// /// <param name="callbackAfterInit">当初始化完成后执行</param>
    /// <param name="rootCanvas"></param>
    public void Build(string resName, Action<IView> buildAction,Action callbackAfterInit,bool rootCanvas = true) =>
        Build(resName, (_, v) => buildAction(v), callbackAfterInit, rootCanvas);
    /// <summary>
    /// 生成Ui
    /// </summary>
    /// <param name="resName">物体名字</param>
    /// <param name="buildAction">生成函数<see cref="IView"/>为传入物件</param>
    /// /// <param name="callbackAfterInit">当初始化完成后执行</param>
    /// <param name="parent"></param>
    public void Build(string resName, Action<IView> buildAction,Action callbackAfterInit, Transform parent) =>
        Build(resName, (_, v) => buildAction(v), callbackAfterInit, parent);

    /// <summary>
    /// 生成Ui
    /// </summary>
    /// <param name="resName">物体名字</param>
    /// <param name="buildAction">生成函数<see cref="GameObject"/>和<see cref="IView"/>为传入物件</param>
    /// /// <param name="callbackAfterInit">当初始化完成后执行</param>
    /// <param name="rootCanvas"></param>
    private void Build(string resName, Action<GameObject, IView> buildAction, Action callbackAfterInit,
        bool rootCanvas = true) =>
        Build(resName, rootCanvas ? Game.SceneCanvas.transform : null, buildAction, callbackAfterInit);

    /// <summary>
    /// 生成Ui
    /// </summary>
    /// <param name="resName">物体名字</param>
    /// <param name="buildAction">生成函数<see cref="GameObject"/>和<see cref="IView"/>为传入物件</param>
    /// <param name="callbackAfterInit">当初始化完成后执行</param>
    /// <param name="parent"></param>
    private void Build(string resName, Action<GameObject,IView> buildAction, Action callbackAfterInit , Transform parent) =>
        Build(resName, parent, buildAction, callbackAfterInit);

    private void Build(string resName, Transform parent, Action<GameObject, IView> buildAction,Action callbackAfterInit)
    {
        GetRes(resName, parent, (go, v) =>
        {
            buildAction.Invoke(go, v);
            callbackAfterInit?.Invoke();
        });
    }    

    public void GetRes(string resName, Transform parent, Action<GameObject, IView> callback)
    {
        GetObj(resName, parent, go =>
        {
            if (go.GetComponent<View>() is not IView v)
                throw new NullReferenceException($"{go.name} is not View!");
            callback.Invoke(go, v);
        });
    }

    public async void GetObj(string resName, Transform parent, Action<GameObject> callback)
    {
        var go = await Game.Res.InstantiateAsync(resName, parent);
        if (go == null)
            throw new NullReferenceException($"{resName} is not found!");
        callback?.Invoke(go);
    }
}