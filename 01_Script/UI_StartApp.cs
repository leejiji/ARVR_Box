using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_StartApp : MonoBehaviour
{
    public void PanelAni(CanvasGroup _panel)
    {
        _panel.alpha = 1f;

        if (_panel.blocksRaycasts == false)
        {
            _panel.transform.DOLocalMoveX(0, 0.3f).SetEase(Ease.OutQuad);
        }
        else
        {
            _panel.transform.DOLocalMoveX(-400f, 0.3f).SetEase(Ease.OutQuad);
        }

        StartCoroutine(panelActive(_panel));
    }

    IEnumerator panelActive(CanvasGroup _panel)
    {
        _panel.interactable = !_panel.interactable;
        _panel.blocksRaycasts = !_panel.blocksRaycasts;
        yield return new WaitForSeconds(1f);
        _panel.alpha = _panel.blocksRaycasts == true ? 1f : 0f;
    }
}
