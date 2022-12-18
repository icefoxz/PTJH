using System;
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
    /// <param name="rootCanvas"></param>
    public void Build(string resName, Action<IView> buildAction, bool rootCanvas = true) =>
        Build(resName, (_, v) => buildAction(v), rootCanvas);
    /// <summary>
    /// 生成Ui
    /// </summary>
    /// <param name="resName">物体名字</param>
    /// <param name="buildAction">生成函数<see cref="IView"/>为传入物件</param>
    /// <param name="parent"></param>
    public void Build(string resName, Action<IView> buildAction, Transform parent) =>
        Build(resName, (_, v) => buildAction(v), parent);
    /// <summary>
    /// 生成Ui
    /// </summary>
    /// <param name="resName">物体名字</param>
    /// <param name="buildAction">生成函数<see cref="GameObject"/>和<see cref="IView"/>为传入物件</param>
    /// <param name="rootCanvas"></param>
    public void Build(string resName, Action<GameObject,IView> buildAction, bool rootCanvas = true) =>
        Build(resName, rootCanvas ? Game.SceneCanvas.transform : null, go =>
        {
            if (go.GetComponent<View>() is not IView v)
                throw new NullReferenceException($"{go.name} is not View!");
            buildAction(go, v);
        });
    /// <summary>
    /// 生成Ui
    /// </summary>
    /// <param name="resName">物体名字</param>
    /// <param name="buildAction">生成函数<see cref="GameObject"/>和<see cref="IView"/>为传入物件</param>
    /// <param name="parent"></param>
    public void Build(string resName, Action<GameObject,IView> buildAction, Transform parent) =>
        Build(resName, parent, go =>
        {
            if (go.GetComponent<View>() is not IView v)
                throw new NullReferenceException($"{go.name} is not View!");
            buildAction(go, v);
        });

    /// <summary>
    /// 生成Ui
    /// </summary>
    /// <param name="resName">物体名字</param>
    /// <param name="buildAction">生成函数<see cref="GameObject"/>为传入物件</param>
    /// <param name="rootCanvas"></param>
    public void Build(string resName, Action<GameObject> buildAction, bool rootCanvas = true) =>
        Build(resName, rootCanvas ? Game.SceneCanvas.transform : null, buildAction);

    private async void Build(string resName, Transform parent, Action<GameObject> buildAction)
    {
        var obj = await Game.Res.InstantiateAsync(resName, parent);
        buildAction.Invoke(obj);
    }
}