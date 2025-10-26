using System.IO;
using UnityEngine;

public class GameManager : SingletonPersistent<GameManager>
{
    public enum GameMode
    {
        LevelMode,
        LadderMode,
        ThemedMode,
    }

    public GameMode CurrentGameMode;

    public void Initialize()
    {
        AdController.Instance.Initialize();
        ProfileManager.Instance.Initialize();
        Dictionary.Instance.Initialize();
        DailyGoalManager.Instance.Initialize();
        AudioManager.Instance.Initialize();
        RewardManager.Instance.Initialize();
        ExpManager.Instance.Initialize();

        ResetAllDataIfPrefsCleared();
        InitializeAdSettings();
    }

    public void ResetAllDataIfPrefsCleared()
    {
        if (!PlayerPrefs.HasKey(Constants.PLAYER_PREFS_INIT_FLAG))
        {
            var path = Application.persistentDataPath;
            var files = Directory.GetFiles(path);

            foreach (var file in files)
            {
                File.Delete(file);
            }

            Debug.Log("PlayerPrefs was cleared. All saver files deleted.");
            PlayerPrefs.SetInt(Constants.PLAYER_PREFS_INIT_FLAG, 1);
            PlayerPrefs.Save();
        }
    }

    public void InitializeAdSettings()
    {
        if (!PlayerPrefs.HasKey(Constants.PLAYER_PREFS_IS_ADS_ENABLED))
        {
            PlayerPrefs.SetInt(Constants.PLAYER_PREFS_IS_ADS_ENABLED, 1);
        }
    }
}
