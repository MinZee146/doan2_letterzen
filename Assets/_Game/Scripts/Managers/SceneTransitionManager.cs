using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using DG.Tweening;
using System;

public class SceneTransitionManager : SingletonPersistent<SceneTransitionManager>
{
    [SerializeField] private GameObject _blocker;
    [SerializeField] private GameObject _transitionGroup;
    [SerializeField] private Image _gameLogo;
    [SerializeField] private RectTransform _revealCircle;

    [SerializeField] private float _logoBounceDuration = 0.5f;
    [SerializeField] private float _maskZoomDuration = 0.5f;
    [SerializeField] private float _maskMaxScale = 50f;

    public void LoadScene(string sceneKey, Action onComplete = null)
    {
        _transitionGroup.SetActive(true);
        _blocker.SetActive(true);

        _gameLogo.DOFade(1f, 0.1f);
        _gameLogo.rectTransform.DOScale(Vector3.one, _logoBounceDuration).SetEase(Ease.OutBounce);

        AudioManager.Instance.PlaySFX("Common_Transition");

        DOVirtual.DelayedCall(_logoBounceDuration * 0.5f, () =>
        {
            _revealCircle.DOScale(_maskMaxScale, _maskZoomDuration).SetEase(Ease.InOutCubic).OnComplete(() =>
            {
                Addressables.LoadSceneAsync(sceneKey).Completed += handle =>
                {
                    DOVirtual.DelayedCall(0.5f, () =>
                    {
                        onComplete?.Invoke();
                        UIManager.Instance.HideAllPopUps();
                    });

                    var hideSeq = DOTween.Sequence();
                    hideSeq.Join(_revealCircle.DOScale(0f, _maskZoomDuration).SetEase(Ease.InBack));
                    hideSeq.Join(_gameLogo.rectTransform.DOScale(Vector3.zero, _logoBounceDuration).SetEase(Ease.InBack));
                    hideSeq.OnComplete(() =>
                    {
                        _transitionGroup.SetActive(false);
                        _blocker.SetActive(false);
                    });
                };
            });
        });
    }
}