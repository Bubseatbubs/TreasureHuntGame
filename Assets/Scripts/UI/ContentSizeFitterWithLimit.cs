using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContentSizeFitterWithLimit : ContentSizeFitter
{
    const float MAX_HORIZONTAL_WIDTH = 140f;

    public override void SetLayoutHorizontal()
    {
        base.SetLayoutHorizontal();
        
        var rectTransform = transform as RectTransform;
        var sizeDelta = rectTransform.sizeDelta;

        sizeDelta.x = Mathf.Clamp(sizeDelta.x, 0, MAX_HORIZONTAL_WIDTH);

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sizeDelta.x);

        ChatMessagesDisplay.instance.ResetScrollbar();
    }
}

