using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIRaycastHole : MonoBehaviour, ICanvasRaycastFilter
{
    [NonSerialized] public RectTransform HoleTarget;
    [NonSerialized] public Vector2 Padding = Vector2.zero;

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (HoleTarget == null) return true;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(HoleTarget, sp, eventCamera, out var localPos);
        var rect = HoleTarget.rect;
        rect.min -= Padding;
        rect.max += Padding;

        return !rect.Contains(localPos);
    }
}
