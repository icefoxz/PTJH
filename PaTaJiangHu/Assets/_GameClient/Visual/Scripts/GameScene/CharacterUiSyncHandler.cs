using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 角色Ui同步器, 用于同步角色在游戏中位置与UI画布上以实现一些需要跟着角色移动的ui元素
/// </summary>
public class CharacterUiSyncHandler : MonoBehaviour
{
    [SerializeField] private RectTransform prefab;
    [SerializeField] private Transform _targetTransform;
    private Dictionary<ISceneObj, RectTransform> _uiMap = new Dictionary<ISceneObj, RectTransform>();
    public IReadOnlyDictionary<ISceneObj, RectTransform> UiMap => _uiMap;
    private IGame2DLand GameLand { get; set; }
    private Canvas SceneCanvas { get; set; }
    private RectTransform SceneCanvasRect { get; set; }
    private RectTransform TargetRect { get; set; }
    private Camera MainCamera { get; set; }

    public void Init(IGame2DLand land, Canvas sceneCanvas, Camera mainCamera)
    {
        GameLand = land;
        SceneCanvas = sceneCanvas;
        MainCamera = mainCamera;
        SceneCanvasRect = sceneCanvas.transform as RectTransform;
        TargetRect = _targetTransform as RectTransform;
    }

    public void AssignObjToUi(ISceneObj obj)
    {
        var uiRect = InstanceUiObj(obj);
        _uiMap.Add(obj, uiRect);
    }

    public RectTransform GetObjRect(ISceneObj obj) => _uiMap[obj];

    private RectTransform InstanceUiObj(ISceneObj obj)
    {
        var colliderSize = obj.Collider.bounds.size;
        var colliderScale = obj.Collider.transform.lossyScale;

        var worldWidth = Mathf.Abs(colliderSize.x * colliderScale.x);
        var worldHeight = Mathf.Abs(colliderSize.y * colliderScale.y);

        // Convert world space size to screen space size
        var screenSpaceSize =
            RectTransformUtility.WorldToScreenPoint(MainCamera, new Vector3(worldWidth, worldHeight, 0)) -
            RectTransformUtility.WorldToScreenPoint(MainCamera, Vector3.zero);

        var uiRect = Instantiate(prefab, SceneCanvas.transform);
        uiRect.pivot = new Vector2(0.5f, 0.5f);
        uiRect.sizeDelta = screenSpaceSize;
        uiRect.SetParent(_targetTransform, true);
        uiRect.gameObject.SetActive(true);
        return uiRect;
    }

    public void RemoveObjFromUi(ISceneObj obj)
    {
        if (_uiMap.TryGetValue(obj, out var uiObj))
        {
            _uiMap.Remove(obj);
            Destroy(uiObj.gameObject);
        }
    }

    public void ClearAll()
    {
        foreach (var obj in _uiMap.Keys.ToArray())
        {
            var uiRect = _uiMap[obj];
            _uiMap.Remove(obj);
            Destroy(uiRect.gameObject);
        }
    }

    void Update()
    {
        foreach (var (obj, ui) in _uiMap)
            ui.transform.localPosition =
                GameLand.ConvertWorldPosToCanvasPos(SceneCanvasRect, TargetRect, obj.Collider.transform.position);
    }
}