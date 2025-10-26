using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinBar : Singleton<CoinBar>
{
    [SerializeField] private TextMeshProUGUI _coinText;
    [SerializeField] private Button _addButton;

    public void SetCoinText()
    {
        _coinText.text = RewardManager.Instance.Coins.ToString();
    }

    public void SetInteractable(bool interactable)
    {
        _addButton.interactable = interactable;
    }

    public void IncreaseCoin(int amount)
    {
        var startValue = RewardManager.Instance.Coins;
        RewardManager.Instance.AddReward(RewardType.Coin, amount);
        var endValue = RewardManager.Instance.Coins;

        DOTween.Kill(_coinText);
        DOTween.To(() => startValue, x =>
        {
            _coinText.text = x.ToString();
        }, endValue, 0.5f).SetTarget(_coinText);

        _coinText.transform.DOKill();
        _coinText.transform.DOPunchScale(Vector3.one * 0.25f, 0.3f, 5, 0.5f);
    }

    public void DecreaseCoin(int amount, Action onSuccessful = null)
    {
        if (RewardManager.Instance.Coins - amount < 0)
        {
            UIManager.Instance.ShowPopUp("InsufficientCoin", true, PopUpShowBehaviour.HIDE_PREVIOUS);
            AudioManager.Instance.PlaySFX("Coin_Insufficient");
        }
        else
        {
            RewardManager.Instance.UseReward(RewardType.Coin, amount);
            _coinText.text = RewardManager.Instance.Coins.ToString();

            onSuccessful?.Invoke();
        }
    }
}
