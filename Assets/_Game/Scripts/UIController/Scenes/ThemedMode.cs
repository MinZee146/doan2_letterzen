using Genix.MocaLib.Runtime.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThemedMode : Singleton<ThemedMode>
{
    [SerializeField] private RectTransform _boardRect, _keyboardRect, _boostersRect, _scrollViewRect;
    [SerializeField] private GameObject _themeSelectUI, _gameUI;
    [SerializeField] private TextMeshProUGUI _themeName;
    [SerializeField] private Image _themeIcon;

    private void Start()
    {
        UpdatePositionBasedOnAds();
    }

    public void BackToHome()
    {
        if (_gameUI.activeSelf)
        {
            SceneTransitionManager.Instance.LoadScene(Constants.THEMED_SCENE_KEY, () => ThemedWordList.Instance.Initialize());
        }
        else
        {
            SceneTransitionManager.Instance.LoadScene(Constants.HOME_SCENE_KEY, () => MainMenu.Instance.Initialize());
        }
    }

    public void ToggleGameUI(bool active)
    {
        _themeSelectUI.SetActive(!active);
        _gameUI.SetActive(active);
    }

    public void UpdateThemeInfo(string themeName, Sprite themeIcon)
    {
        _themeName.text = themeName;
        _themeIcon.sprite = themeIcon;
    }

    public void UpdatePositionBasedOnAds()
    {
        var isAdEnabled = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_IS_ADS_ENABLED, 0) == 1 &&
                          PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_LEVEL, 1) >=
                          RemoteConfigs.Instance.GameConfigs.BannerStartFromLevel;

        if (isAdEnabled)
        {
            var bannerHeight = MocaLib.Instance.AdManager.GetBannerHeight(GetComponent<Canvas>());

            _boardRect.offsetMin += new Vector2(0f, bannerHeight);
            _scrollViewRect.offsetMin += new Vector2(0f, bannerHeight);
            _keyboardRect.anchoredPosition += new Vector2(0f, bannerHeight);
            _boostersRect.anchoredPosition += new Vector2(0f, bannerHeight);
        }
        else
        {
            _boardRect.offsetMin = new Vector2(0f, 575);
            _scrollViewRect.offsetMin = new Vector2(0f, 75);
            _keyboardRect.anchoredPosition = new Vector2(0f, 425);
            _boostersRect.anchoredPosition = new Vector2(0f, 525);
        }
    }
}
