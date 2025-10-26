using System.Collections.Generic;
using DG.Tweening;
using Febucci.UI;
using UnityEngine;

public class InstructionPopUp : MonoBehaviour
{
    [SerializeField] private GameObject _closeButton;
    [SerializeField] private TypewriterByCharacter _title1, _title2;
    [SerializeField] private List<CanvasGroup> _greenTileGroups;
    [SerializeField] private List<CanvasGroup> _yellowTileGroups;
    [SerializeField] private List<CanvasGroup> _greyTileGroups;

    void OnEnable()
    {
        Reset();

        _title1.ShowText("HOW TO PLAY");
        _title2.ShowText("Guess the hidden words in 6 tries.\nThe tile's color indicates how close you are.");
    }

    void OnDisable()
    {
        if (PlayerPrefs.GetInt(Constants.PLAYER_PREFS_HAS_LOADING_BEFORE, 0) == 0)
        {
            LevelWordList.Instance.Initialize();
            PlayerPrefs.SetInt(Constants.PLAYER_PREFS_HAS_LOADING_BEFORE, 1);
            PlayerPrefs.Save();
        }
    }

    public void AnimateShowInstruction()
    {
        var seq = DOTween.Sequence();

        foreach (var cg in _greenTileGroups)
        {
            seq.Join(cg.DOFade(1f, 1f).SetEase(Ease.OutQuad));
        }

        seq.AppendInterval(0.5f);

        foreach (var cg in _yellowTileGroups)
        {
            seq.Join(cg.DOFade(1f, 1f).SetEase(Ease.OutQuad));
        }

        seq.AppendInterval(0.5f);

        foreach (var cg in _greyTileGroups)
        {
            seq.Join(cg.DOFade(1f, 1f).SetEase(Ease.OutQuad));
        }

        seq.AppendInterval(0.5f);
        seq.Append(_closeButton.GetComponent<CanvasGroup>().DOFade(1f, 0.5f).SetEase(Ease.OutQuad));
        seq.Join(_closeButton.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
    }

    public void Reset()
    {
        foreach (var cg in _greenTileGroups)
        {
            cg.alpha = 0f;
        }

        foreach (var cg in _yellowTileGroups)
        {
            cg.alpha = 0f;
        }

        foreach (var cg in _greyTileGroups)
        {
            cg.alpha = 0f;
        }

        _closeButton.GetComponent<CanvasGroup>().alpha = 0f;
        _closeButton.transform.localScale = Vector3.zero;
    }
}
