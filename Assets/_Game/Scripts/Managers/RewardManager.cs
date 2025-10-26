using System;
using UnityEngine;

public class RewardManager : SingletonPersistent<RewardManager>
{
    public int Coins { get; private set; }
    public int Reveals { get; private set; }
    public int Clears { get; private set; }

    public void Initialize()
    {
        Coins = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_COIN, 2000);
        Reveals = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_REVEAL, 1);
        Clears = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_CLEAR, 1);
    }

    public void AddReward(RewardType reward, int amount = 1)
    {
        switch (reward)
        {
            case RewardType.Coin:
                Coins += amount;
                break;

            case RewardType.Reveal:
                Reveals += amount;
                break;

            case RewardType.Clear:
                Clears += amount;
                break;
        }

        SaveRewards();
    }

    public void UseReward(RewardType reward, int amount = 1, Action onSuccessful = null)
    {
        switch (reward)
        {
            case RewardType.Coin:
                Coins -= amount;
                onSuccessful?.Invoke();
                break;

            case RewardType.Reveal:
                Reveals -= amount;
                onSuccessful?.Invoke();
                break;

            case RewardType.Clear:
                Clears -= amount;
                onSuccessful?.Invoke();
                break;
        }

        SaveRewards();
    }

    private void SaveRewards()
    {
        PlayerPrefs.SetInt(Constants.PLAYER_PREFS_CURRENT_COIN, Coins);
        PlayerPrefs.SetInt(Constants.PLAYER_PREFS_CURRENT_REVEAL, Reveals);
        PlayerPrefs.SetInt(Constants.PLAYER_PREFS_CURRENT_CLEAR, Clears);
        PlayerPrefs.Save();
    }
}

public enum RewardType
{
    Coin,
    Reveal,
    Clear
}
