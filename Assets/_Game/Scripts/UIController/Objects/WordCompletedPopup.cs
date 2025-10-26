using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class WordCompletedPopup : Singleton<WordCompletedPopup>
{
    [SerializeField] private GameObject _chest;

    private Transform _top, _coin;
    private Tween _idleShakeTween;
    private Action _onComplete;
    private bool _isChestOpened;
    private int _coinAmount;

    private void OnEnable()
    {
        _top = _chest.transform.GetChild(2);
        _coin = _chest.transform.GetChild(3);

        _isChestOpened = false;
        _top.localPosition = new(0f, 30f, 0f);
        _coin.localPosition = Vector3.zero;
        _coin.localScale = Vector3.zero;
    }

    public void SetPopupState(int coinAmount, Action onComplete = null)
    {
        _coinAmount = coinAmount;
        _onComplete = onComplete;

        _chest.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        _idleShakeTween?.Kill();

        _idleShakeTween = DOTween.Sequence()
        .Append(_chest.transform.DOLocalRotate(new Vector3(0f, 0f, 7.5f), 0.2f).SetEase(Ease.InOutSine))
        .Append(_chest.transform.DOLocalRotate(new Vector3(0f, 0f, -7.5f), 0.2f).SetEase(Ease.InOutSine))
        .Append(_chest.transform.DOLocalRotate(new Vector3(0f, 0f, 5f), 0.2f).SetEase(Ease.InOutSine))
        .Append(_chest.transform.DOLocalRotate(Vector3.zero, 0.2f).SetEase(Ease.OutSine))
        .AppendInterval(0.5f)
        .SetLoops(-1);
    }

    public void OpenChest()
    {
        if (_isChestOpened) return;

        _isChestOpened = true;
        _idleShakeTween?.Kill();
        _chest.transform.DOLocalRotate(Vector3.zero, 0.25f).SetEase(Ease.OutSine).OnComplete(() => AnimateOpenChestAndReward());
    }

    private void AnimateOpenChestAndReward()
    {
        _coin.GetComponentInChildren<TextMeshProUGUI>().text = _coinAmount.ToString();

        var seq = DOTween.Sequence();
        seq.Append(_top.DOLocalMoveY(_top.localPosition.y + 40f, 0.5f).SetEase(Ease.OutBack));
        seq.Join(_chest.transform.DOPunchScale(new Vector3(0.25f, 0.25f, 0f), 0.5f, 4, 0.6f));

        seq.AppendInterval(0.25f);
        seq.Append(_coin.DOScale(1f, 0.5f).From(0f).SetEase(Ease.OutBack));
        seq.Join(_coin.DOLocalJump(new Vector3(0f, -85f, 0f), 100f, 1, 0.5f).SetEase(Ease.OutCubic));

        seq.AppendInterval(0.25f);
        seq.AppendCallback(() => RewardAttractor.Instance.RewardAttract(RewardType.Coin, _coin,
                                 GameObject.FindGameObjectWithTag("Coin").transform,
                                 () => CoinBar.Instance.IncreaseCoin(_coinAmount)));

        seq.AppendInterval(2f);
        seq.OnComplete(() =>
        {
            _onComplete?.Invoke();
        });
    }
}
