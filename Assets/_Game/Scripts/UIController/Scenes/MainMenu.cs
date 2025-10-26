using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : Singleton<MainMenu>
{
    [SerializeField] private List<GameObject> _gameModeTransforms;
    [SerializeField] private GameObject _selectIndicator;
    [SerializeField] private TextMeshProUGUI _gameModeText, _rankText, _expText;
    [SerializeField] private Image _currentAvatar;

    private int _currentIndex;

    public void Initialize()
    {
        UpdateAvatar();
        UpdateRankText();

        foreach (var button in _gameModeTransforms)
        {
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySFX("Common_Swoosh");

                _selectIndicator.transform.DOMove(button.transform.position, 0.1f);


                var index = _gameModeTransforms.IndexOf(button);
                _currentIndex = index;
                _gameModeText.text = index switch 
                {
                    0 => "Classic Wordle",
                    1 => "Ladder Puzzle",
                    2 => "Theme Based Puzzle",
                    _ => throw new ArgumentOutOfRangeException()
                };
            });
        }
    }

    public void UpdateAvatar()
    {
        _currentAvatar.sprite = ProfileManager.Instance.PlayerAvatar;
    }

    public void UpdateRankText()
    {
        _rankText.text = ExpManager.Instance.GetRank();
        _expText.text = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_EXP, 0).ToString();
    }

    public void Play()
    {
        switch (_currentIndex)
        {
            case 0:
                GameManager.Instance.CurrentGameMode = GameManager.GameMode.LevelMode;
                SceneTransitionManager.Instance.LoadScene(Constants.LEVEL_SCENE_KEY, () => LevelWordList.Instance.Initialize());

                break;
            case 1:
                GameManager.Instance.CurrentGameMode = GameManager.GameMode.LadderMode;
                SceneTransitionManager.Instance.LoadScene(Constants.LADDER_SCENE_KEY, () => LadderWordList.Instance.Initialize());

                break;
            case 2:
                GameManager.Instance.CurrentGameMode = GameManager.GameMode.ThemedMode;
                SceneTransitionManager.Instance.LoadScene(Constants.THEMED_SCENE_KEY, () => ThemedWordList.Instance.Initialize());

                break;
        }
    }
}