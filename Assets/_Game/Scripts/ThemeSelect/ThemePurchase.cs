using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThemePurchase : Singleton<ThemePurchase>
{
    [SerializeField] private TextMeshProUGUI _themeName, _themePrice;
    [SerializeField] private Image _themeIcon;
    [SerializeField] private Button _purchaseButton;
    [SerializeField] private GameObject _radialShine;

    private Tween _shineTween;

    public void LoadThemeInfo(string themeName, string price, Sprite icon)
    {
        _themeName.text = themeName;
        _themePrice.text = price;
        _themeIcon.sprite = icon;

        _purchaseButton.onClick.RemoveAllListeners();
        _purchaseButton.onClick.AddListener(() =>
        {
            CoinBar.Instance.DecreaseCoin(int.Parse(price), onSuccessful: () =>
            {
                PlayerPrefs.SetInt($"{themeName}Unlocked", 1);
                PlayerPrefs.Save();
                
                UIManager.Instance.HideLastPopUp();
                ThemeSelectManager.Instance.UnlockTheme(themeName);
                AudioManager.Instance.PlaySFX("Common_CashOut");
            });
        });

        _radialShine.transform.rotation = Quaternion.identity;

        if (_shineTween != null && _shineTween.IsActive())
        {
            _shineTween.Kill();
        }

        _shineTween = _radialShine.transform.DORotate(
                new Vector3(0f, 0f, 360f), 10f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart); // use Restart instead of Incremental
    }
}
