using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using Newtonsoft.Json;

public class LevelWordList : Singleton<LevelWordList>
{
    private Dictionary<string, string> _wordDefinitions = new();
    private List<string> _wordList = new();
    private TextAsset _dictText;
    private int _currentWordIndex;

    public void Initialize()
    {
        if (!PlayerPrefs.HasKey(Constants.PLAYER_PREFS_CURRENT_LEVEL))
        {
            PlayerPrefs.SetInt(Constants.PLAYER_PREFS_CURRENT_LEVEL, 0);
        }

        _currentWordIndex = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_LEVEL);
        LevelMode.Instance.UpdateLevelText();

        PlayerPrefs.Save();
        Addressables.LoadAssetAsync<TextAsset>(Constants.LEVEL_MODE_KEY).Completed += OnDictionaryLoaded;
    }

    public string GetWord()
    {
        if (_wordList.Count != 0 && _currentWordIndex < _wordList.Count)
        {
            return _wordList[_currentWordIndex];
        }

        Debug.LogWarning("Word list is not loaded yet or empty.");
        return null;
    }

    public void ProceedToNextWord()
    {
        _currentWordIndex++;

        DailyGoalManager.Instance.UpdateProgress(GoalType.CompleteLevelWords);
        PlayerPrefs.SetInt(Constants.PLAYER_PREFS_CURRENT_LEVEL, _currentWordIndex);
        PlayerPrefs.Save();

        BoardSave.Instance.DeleteBoardSave();
        Board.Instance.SetWord(GetWord());
        LevelMode.Instance.UpdateLevelText();

        if (PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_LEVEL, 1) == RemoteConfigs.Instance.GameConfigs.BannerStartFromLevel)
        {
            LevelMode.Instance.UpdatePositionBasedOnAds();
            AdController.Instance.ShowBanner();
        }

        if (_currentWordIndex == _wordList.Count)
        {
            UIManager.Instance.ShowPopUp("Compliment", true, PopUpShowBehaviour.HIDE_PREVIOUS);
            WordCompletedPopup.Instance.SetPopupState(RemoteConfigs.Instance.GameConfigs.CoinsCompletedWords, () =>
            {
                SceneTransitionManager.Instance.LoadScene(Constants.HOME_SCENE_KEY, () => MainMenu.Instance.Initialize());
            });
        }
    }

    private void OnDictionaryLoaded(AsyncOperationHandle<TextAsset> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _dictText = handle.Result;
            LoadDictionary(_dictText.text);
            Debug.Log("Level word list loaded and processed.");

            Board.Instance.SetWord(GetWord());
            if (!string.IsNullOrEmpty(GetWord())) Board.Instance.SpawnRow(GetWord().Length);
        }
        else
        {
            Debug.LogError("Failed to load level word list asset from Addressables.");
        }
    }

    private void LoadDictionary(string jsonText)
    {
        try
        {
            _wordDefinitions = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonText);
            _wordList = new List<string>(_wordDefinitions.Keys);
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing dictionary JSON: " + e.Message);
        }
    }

    public string GetDefinition(string word)
    {
        return _wordDefinitions.GetValueOrDefault(word.ToLower(), "Definition not found.");
    }

    public void Unload()
    {
        Addressables.Release(_dictText);
        _wordDefinitions.Clear();
        _wordList.Clear();
    }
}
