using System;
using System.Collections.Generic;
using Genix.MocaLib.Runtime.Services;
using TMPro;
using UnityEngine;

public class LevelMode : Singleton<LevelMode>
{
    [SerializeField] private CanvasGroup _backBtn, _definitionBtn, _dailyBtn;
    [SerializeField] private GameObject _revealBtn, _clearBtn;
    [SerializeField] private RectTransform _boardRect, _keyboardRect, _boostersRect;
    [SerializeField] private TextMeshProUGUI _levelText;

    private void Start()
    {
        TutorialController.Instance.Initialize();
        Definition.Instance.SetDefinitionPrice();
        
        UpdateUIState();
        UpdatePositionBasedOnAds();
    }

    public void UpdateUIState()
    {
        if (TutorialController.Instance.IsTutorialCompleted) return;

        var currentLevel = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_LEVEL, 0) + 1;
        var uiActions = new Dictionary<int, Action>
        {
            [1] = () => SetUIVisibility(false, false, false, false, false),
            [2] = () => SetUIVisibility(false, false, false, false, false),
            [3] = () => SetUIVisibility(false, false, false, true, false),
            [4] = () => SetUIVisibility(false, false, false, true, true),
            [5] = () => SetUIVisibility(false, false, true, true, true)
        };

        if (uiActions.TryGetValue(currentLevel, out var action))
        {
            action.Invoke();
        }
        else
        {
            SetUIVisibility(true, true, true, true, true);
        }
    }

    private void SetUIVisibility(bool backVisible, bool dailyVisible, bool definitionVisible, bool revealVisible, bool clearVisible)
    {
        SetCanvasGroupState(_backBtn, backVisible);
        SetCanvasGroupState(_dailyBtn, dailyVisible);
        SetCanvasGroupState(_definitionBtn, definitionVisible);

        _revealBtn.SetActive(revealVisible);
        _clearBtn.SetActive(clearVisible);
    }

    private void SetCanvasGroupState(CanvasGroup group, bool state)
    {
        group.alpha = state ? 1f : 0f;
        group.interactable = state;
        group.blocksRaycasts = state;
    }

    public void BackToHome()
    {
        SceneTransitionManager.Instance.LoadScene(Constants.HOME_SCENE_KEY, () => MainMenu.Instance.Initialize());
    }

    public void UpdateLevelText()
    {
        var level = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_LEVEL, 0);
        _levelText.text = $"Level {level + 1}";
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
            _keyboardRect.anchoredPosition += new Vector2(0f, bannerHeight);
            _boostersRect.anchoredPosition += new Vector2(0f, bannerHeight);
        }
        else
        {
            _boardRect.offsetMin = new Vector2(0f, 575);
            _keyboardRect.anchoredPosition = new Vector2(0f, 425);
            _boostersRect.anchoredPosition = new Vector2(0f, 525);
        }
    }

    public GameObject GetRevealBtn() => _revealBtn;
    public GameObject GetClearBtn() => _clearBtn;
    public GameObject GetDefinitionBtn() => _definitionBtn.transform.GetChild(2).gameObject;
}