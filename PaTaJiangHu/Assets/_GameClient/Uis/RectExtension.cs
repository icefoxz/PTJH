using System;
using UnityEngine;

public static class RectExtension
{
    public static float GetSize(this Rect rect, RectTransform.Axis axis) => axis switch
    {
        RectTransform.Axis.Horizontal => rect.width,
        RectTransform.Axis.Vertical => rect.height,
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
    };
    
    public static void SetSize(this RectTransform rectTran,float size ,RectTransform.Axis axis)
    {
        var rect = rectTran.rect;
        switch (axis)
        {
            case RectTransform.Axis.Horizontal:
                rectTran.rect.Set(rect.x, rect.y, size, rect.height);
                break;
            case RectTransform.Axis.Vertical:
                rectTran.rect.Set(rect.x, rect.y, rect.width, size);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
        }
    }

    /// <summary>
    /// Set width of RectTransform with sizeDelta.x
    /// </summary>
    public static void SetWidth(this RectTransform transform, float width)
        => transform.sizeDelta = transform.sizeDelta.SetX(width);

    /// <summary>
    /// Set height of RectTransform with sizeDelta.y
    /// </summary>
    public static void SetHeight(this RectTransform transform, float height)
        => transform.sizeDelta = transform.sizeDelta.SetY(height);


    #region Set X/Y/Z

    // Set X

    public static Vector3 SetX(this Vector3 vector, float x) => new Vector3(x, vector.y, vector.z);

    public static Vector2 SetX(this Vector2 vector, float x) => new Vector2(x, vector.y);

    public static void SetX(this Transform transform, float x) => transform.position = transform.position.SetX(x);
    public static void SetLocalX(this Transform transform, float x) => transform.localPosition = transform.localPosition.SetX(x);

    // Set Y

    public static Vector3 SetY(this Vector3 vector, float y) => new Vector3(vector.x, y, vector.z);

    public static Vector2 SetY(this Vector2 vector, float y) => new Vector2(vector.x, y);

    public static void SetY(this Transform transform, float y) => transform.position = transform.position.SetY(y);
    public static void SetLocalY(this Transform transform, float y) => transform.localPosition = transform.localPosition.SetY(y);

    // Set Z

    public static Vector3 SetZ(this Vector3 vector, float z) => new Vector3(vector.x, vector.y, z);

    public static void SetZ(this Transform transform, float z) => transform.position = transform.position.SetZ(z);
    public static void SetLocalZ(this Transform transform, float z) => transform.localPosition = transform.localPosition.SetZ(z);

    // Set XY

    public static Vector3 SetXY(this Vector3 vector, float x, float y) => new Vector3(x, y, vector.z);

    public static void SetXY(this Transform transform, float x, float y) => transform.position = transform.position.SetXY(x, y);
    public static void SetLocalXY(this Transform transform, float x, float y) => transform.localPosition = transform.localPosition.SetXY(x, y);

    // Set XZ

    public static Vector3 SetXZ(this Vector3 vector, float x, float z) => new Vector3(x, vector.y, z);

    public static void SetXZ(this Transform transform, float x, float z) => transform.position = transform.position.SetXZ(x, z);
    public static void SetLocalXZ(this Transform transform, float x, float z) => transform.localPosition = transform.localPosition.SetXZ(x, z);

    // Set YZ

    public static Vector3 SetYZ(this Vector3 vector, float y, float z) => new Vector3(vector.x, y, z);

    public static void SetYZ(this Transform transform, float y, float z) => transform.position = transform.position.SetYZ(y, z);
    public static void SetLocalYZ(this Transform transform, float y, float z) => transform.localPosition = transform.localPosition.SetYZ(y, z);

    #endregion

}