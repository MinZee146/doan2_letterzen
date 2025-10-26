using System.ComponentModel;
using UnityEngine;

public partial class SROptions
{
    [Category("Debug")]
    public void GetWord()
    {
        Debug.Log("Current word: " + Board.Instance.CurrentWord);
    }

    [Category("Debug")]
    public void SkipTutorial()
    {
        PlayerPrefs.SetInt(Constants.PLAYER_PREFS_CURRENT_LEVEL, 5);
        TutorialController.Instance.Initialize();
    }

    [Category("Save")]
    public void DeleteBoardSave()
    {
        BoardSave.Instance.DeleteBoardSave();
    }

    [Category("Save")]
    public void DeleteLadderSave()
    {
        LadderSave.Instance.DeleteLadderSave();
    }

    [Category("Cheats")]
    public void IncreaseCoin()
    {
        CoinBar.Instance.IncreaseCoin(5000);
    }

    [Category("Cheats")]
    public void IncreaseReveal()
    {
        RewardManager.Instance.AddReward(RewardType.Reveal);
    }

    [Category("Cheats")]
    public void IncreaseClear()
    {
        RewardManager.Instance.AddReward(RewardType.Clear);
    }

    [Category("Ads")]
    public void EnableAds()
    {
        PlayerPrefs.SetInt(Constants.PLAYER_PREFS_IS_ADS_ENABLED, 1);
        PlayerPrefs.Save();
        AdController.Instance.ShowBanner();
    }

    [Category("Ads")]
    public void DisableAds()
    {
        PlayerPrefs.SetInt(Constants.PLAYER_PREFS_IS_ADS_ENABLED, 0);
        PlayerPrefs.Save();
        AdController.Instance.HideBanner();
    }
}
