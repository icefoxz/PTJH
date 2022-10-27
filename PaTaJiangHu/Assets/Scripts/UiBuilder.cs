using System;
using System.Collections;
using UnityEngine;
using Views;

public class UiBuilder
{
    public void Build(string resName, Action<IView> onCompleteAction, bool rootCanvas = true) =>
        Build(resName, (_, v) => onCompleteAction(v), rootCanvas);

    public void Build(string resName, Action<GameObject,IView> onCompleteAction, bool rootCanvas = true) =>
        Build(resName, rootCanvas ? Game.SceneCanvas.transform : null, go =>
        {
            var v = go.GetComponent<View>() as IView;
            onCompleteAction(go, v);
        });

    public void Build(string resName, Action<GameObject> onCompleteAction, bool rootCanvas = true) =>
        Build(resName, rootCanvas ? Game.SceneCanvas.transform : null, onCompleteAction);

    private async void Build(string resName, Transform parent, Action<GameObject> onCompleteAction)
    {
        var obj = await Game.Res.InstantiateAsync(resName, parent);
        onCompleteAction.Invoke(obj);
    }
}