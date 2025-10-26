using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LadderWordList : Singleton<LadderWordList>
{
    private Dictionary<int, List<string>> _wordGroups;
    private int _currentGroupIndex, _currentIndex;

    public void Initialize()
    {
        if (!PlayerPrefs.HasKey(Constants.PLAYER_PREFS_CURRENT_LADDER_GROUP))
        {
            PlayerPrefs.SetInt(Constants.PLAYER_PREFS_CURRENT_LADDER_GROUP, 1);
        }

        if (!PlayerPrefs.HasKey(Constants.PLAYER_PREFS_CURRENT_LADDER_INDEX))
        {
            PlayerPrefs.SetInt(Constants.PLAYER_PREFS_CURRENT_LADDER_INDEX, 1);
        }

        _currentGroupIndex = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_LADDER_GROUP);
        _currentIndex = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_LADDER_INDEX, 1);
        PlayerPrefs.Save();
        Addressables.LoadAssetAsync<TextAsset>(Constants.LADDER_MODE_KEY).Completed += OnJsonLoaded;
    }

    private void OnJsonLoaded(AsyncOperationHandle<TextAsset> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            var jsonFile = handle.Result;
            var tempDict = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(jsonFile.text);
            _wordGroups = tempDict.ToDictionary(kvp => int.Parse(kvp.Key), kvp => kvp.Value);

            if (EndAllGroups()) return;

            var words = GetWordsByGroup(_currentGroupIndex);
            var firstWord = words[_currentIndex - 1];
            var secondWord = _currentIndex > 1 ? words[_currentIndex - 2] : null;
            var thirdWord = _currentIndex > 2 ? words[_currentIndex - 3] : null;

            LadderMode.Instance.UpdateWordSlide(firstWord, secondWord, thirdWord);
            LadderMode.Instance.Initialize();
            ProgressBar.Instance.SetupProgressBar();
            ExpBar.Instance.AnimateIncreaseExp();

            Debug.Log("Ladder word list loaded and processed.");
        }
        else
        {
            Debug.LogError("Failed to load ladder word list asset from Addressables.");
        }
    }

    private List<string> GetWordsByGroup(int groupId)
    {
        if (_wordGroups != null && _wordGroups.TryGetValue(groupId, out var group))
        {
            return group;
        }

        return null;
    }

    public bool IsLastWordInGroup()
    {
        return _currentIndex == _wordGroups[_currentGroupIndex].Count - 1;
    }

    public string GetWord()
    {
        return GetWordsByGroup(_currentGroupIndex)[_currentIndex];
    }

    public string GetSpawnWord()
    {
        var word = GetWord();

        if (IsLastWordInGroup() && !LadderMode.Instance.IsMatchWord)
        {
            var availableChars = Enumerable.Range('a', 26)
                                .Select(i => (char)i)
                                .Where(c => !word.Contains(c))
                                .ToList();

            var randomChar = availableChars[Random.Range(0, availableChars.Count)];
            word += randomChar;
        }

        return word;
    }

    public void ProceedToNextGroup()
    {
        _currentGroupIndex++;
        PlayerPrefs.SetInt(Constants.PLAYER_PREFS_CURRENT_LADDER_GROUP, _currentGroupIndex);
        PlayerPrefs.Save();
    }

    public void ProceedToNextWord()
    {
        LadderSave.Instance.DeleteLadderSave();
        DailyGoalManager.Instance.UpdateProgress(GoalType.CompleteLadderWords);

        if (_currentIndex >= _wordGroups[_currentGroupIndex].Count - 1)
        {
            ProceedToNextGroup();

            if (EndAllGroups())
            {
                return;
            }

            LadderMode.Instance.ClearWordSlide();
            LadderMode.Instance.UpdateWordSlide(GetWordsByGroup(_currentGroupIndex)[0]);

            _currentIndex = 1;
            PlayerPrefs.SetInt(Constants.PLAYER_PREFS_CURRENT_LADDER_INDEX, _currentIndex);
            PlayerPrefs.Save();

            return;
        }

        _currentIndex++;
        PlayerPrefs.SetInt(Constants.PLAYER_PREFS_CURRENT_LADDER_INDEX, _currentIndex);
        PlayerPrefs.Save();
    }

    public bool EndAllGroups()
    {
        return _currentGroupIndex > _wordGroups.Count;
    }
}
