using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Newtonsoft.Json;
using System.Linq;

public class ThemedWordList : Singleton<ThemedWordList>
{
    private Dictionary<string, Dictionary<string, string>> _themeWords = new();
    private TextAsset _themeText;
    private string _currentTheme;
    private int _currentWordIndex;

    public string CurrentTheme => _currentTheme;

    public void Initialize()
    {
        Addressables.LoadAssetAsync<TextAsset>(Constants.THEMED_MODE_KEY).Completed += OnThemesLoaded;
    }

    private void OnThemesLoaded(AsyncOperationHandle<TextAsset> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _themeText = handle.Result;
            LoadThemes(_themeText.text);
            Debug.Log("Theme word list loaded and processed.");

            ThemeSelectManager.Instance.LoadThemes();
        }
        else
        {
            Debug.LogError("Failed to load theme word list asset from Addressables.");
        }
    }

    private void LoadThemes(string json)
    {
        try
        {
            _themeWords = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to parse themed word list: " + e.Message);
        }
    }

    public void SelectTheme(string themeName)
    {
        _currentTheme = themeName;
        _currentWordIndex = PlayerPrefs.GetInt($"{themeName}Progress", 0);
    }

    public string GetWord()
    {
        if (!_themeWords.ContainsKey(_currentTheme) || _themeWords[_currentTheme].Count == 0)
        {
            Debug.LogWarning("Theme not found or word list is empty.");
            return null;
        }

        var words = _themeWords[_currentTheme];
        return _currentWordIndex < words.Count ? words.ElementAt(_currentWordIndex).Key : null;
    }

    public string GetDefinition()
    {
        if (!_themeWords.ContainsKey(_currentTheme) || _themeWords[_currentTheme].Count == 0)
        {
            Debug.LogWarning("Theme not found or word list is empty.");
            return null;
        }

        var words = _themeWords[_currentTheme];
        return _currentWordIndex < words.Count ? words.ElementAt(_currentWordIndex).Value : null;
    }

    public void ProceedToNextWord()
    {
        _currentWordIndex++;

        DailyGoalManager.Instance.UpdateProgress(GoalType.CompleteThemedWords);
        PlayerPrefs.SetInt($"{_currentTheme}Progress", _currentWordIndex);
        PlayerPrefs.Save();

        BoardSave.Instance.DeleteBoardSave();
        Board.Instance.SetWord(GetWord());

        if (_currentWordIndex == GetThemeWordCount(_currentTheme))
        {
            PlayerPrefs.SetInt($"{_currentTheme}Completed", 1);
            PlayerPrefs.Save();

            UIManager.Instance.ShowPopUp("Compliment", true, PopUpShowBehaviour.HIDE_PREVIOUS);
            WordCompletedPopup.Instance.SetPopupState(RemoteConfigs.Instance.GameConfigs.CoinsCompletedTheme, () =>
            {
                SceneTransitionManager.Instance.LoadScene(Constants.THEMED_SCENE_KEY, () => Instance.Initialize());
            });
        }
    }

    public int GetThemeWordCount(string themeName)
    {
        return _themeWords[themeName].Count;
    }

    public void Unload()
    {
        Addressables.Release(_themeText);
        _themeWords.Clear();
        _currentWordIndex = 0;
    }
}
