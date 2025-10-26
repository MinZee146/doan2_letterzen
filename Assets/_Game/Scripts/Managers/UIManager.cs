using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonPersistent<UIManager>
{
    [SerializeField] private GameObject _background, _coinBar;

    private List<GameObject> _popUpsList = new();
    private List<string> _popUpsNames = new();
    private List<string> _needCoinPopup = new() { "ThemePurchase", "InsufficientCoin" };

    public void ShowPopUp(string popUpId, bool showCoinBar = false, PopUpShowBehaviour behaviour = PopUpShowBehaviour.KEEP_PREVIOUS)
    {
        if (_popUpsNames.Contains(popUpId)) return;
        AudioManager.Instance.PlaySFX("PopUp_Toggle");

        var popUp = PopUpPool.Instance.GetPopUpFromPool(popUpId);
        if (popUp != null)
        {
            if (behaviour == PopUpShowBehaviour.HIDE_PREVIOUS && AnyPopUpShowing())
            {
                var lastPopUp = _popUpsList.Last();
                lastPopUp.SetActive(false);
            }

            _popUpsList.Add(popUp);
            _popUpsNames.Add(popUpId);

            if (popUpId != "WordFailed")
            {
                _background.SetActive(true);
                _background.GetComponent<Image>().DOFade(0.95f, 0.25f).SetEase(Ease.OutQuad);
            }
            else
            {
                WordFailedPanel.Instance.ShowWordFailed(GetComponent<Canvas>());
            }

            popUp.transform.SetSiblingIndex(popUp.transform.parent.childCount - 1);
            _coinBar.transform.SetSiblingIndex(showCoinBar ? popUp.transform.parent.childCount - 1 : 0);
            CoinBar.Instance.SetInteractable(_needCoinPopup.Contains(popUpId));
        }
        else
        {
            Debug.LogWarning($"Pop up with id {popUpId} not found");
        }
    }

    public void HideLastPopUp()
    {
        if (!AnyPopUpShowing()) return;
        AudioManager.Instance.PlaySFX("PopUp_Toggle");

        var lastPopUp = _popUpsList.Last();
        _popUpsList.Remove(lastPopUp);
        _popUpsNames.Remove(lastPopUp.name);

        PopUpPool.Instance.PoolObject(lastPopUp);
        CoinBar.Instance.SetInteractable(_popUpsList.All(p => _needCoinPopup.Contains(p.name)));

        if (!AnyPopUpShowing() || _popUpsList.Any(p => p.name == "WordFailed"))
        {
            _background.SetActive(false);
            _background.GetComponent<Image>().DOFade(0f, 0.25f).SetEase(Ease.InQuad);
            return;
        }

        //Reshow the last popup if it was hidden by behaviour hide_previous
        lastPopUp = _popUpsList.Last();
        if (lastPopUp != null && !lastPopUp.activeInHierarchy)
        {
            lastPopUp.SetActive(true);
        }
    }

    public void HideAllPopUps()
    {
        AudioManager.Instance.PlaySFX("PopUp_Toggle");

        foreach (var popUp in _popUpsList)
        {
            PopUpPool.Instance.PoolObject(popUp);
        }

        _popUpsList.Clear();
        _popUpsNames.Clear();

        CoinBar.Instance.SetInteractable(true);
        _background.SetActive(false);
        _background.GetComponent<Image>().DOFade(0f, 0.25f).SetEase(Ease.InQuad);
    }

    private bool AnyPopUpShowing()
    {
        return _popUpsList.Any();
    }

    public void ShowCoinBar()
    {
        _coinBar.SetActive(true);
        _coinBar.transform.localScale = Vector3.one;
        CoinBar.Instance.SetCoinText();
    }

    public void HideCoinBar()
    {
        if (PlayerPrefs.GetInt(Constants.PLAYER_PREFS_IS_TUTORIAL_COMPLETED, 0) == 0)
        {
            _coinBar.transform.localScale = Vector3.zero;
        }
    }
}
