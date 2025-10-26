using System;
using UnityEngine;

public class DailySpin : Singleton<DailySpin>
{
    [SerializeField] private GameObject _notifier;
    
    public DateTime NextAvailableDate { get; private set; }

    private void Start()
    {
        GrantDailySpin();
    }

    private void GrantDailySpin()
    {
        var lastRewardTimestamp = PlayerPrefs.GetString(Constants.PLAYER_PREFS_LAST_REWARD, "");

        if (!DateTime.TryParse(lastRewardTimestamp, out var lastRewardDate))
        {
            lastRewardDate = DateTime.MinValue;
        }

        var now = DateTime.Now;
        var isNewDay = lastRewardDate.Date < now.Date;

        if (isNewDay || !PlayerPrefs.HasKey(Constants.PLAYER_PREFS_HAS_SPUN))
        {
            PlayerPrefs.SetString(Constants.PLAYER_PREFS_LAST_REWARD, now.ToString());
            PlayerPrefs.SetInt(Constants.PLAYER_PREFS_HAS_SPUN, 0);
        }

        var hasSpunToday = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_HAS_SPUN, 0) == 1;

        _notifier.SetActive(!hasSpunToday);
        NextAvailableDate = lastRewardDate.Date.AddDays(1);
    }

    public void DisableSpin()
    {
        NextAvailableDate = DateTime.Now.Date.AddDays(1);
        _notifier.SetActive(false);
        PlayerPrefs.SetInt(Constants.PLAYER_PREFS_HAS_SPUN, 1);
    }
}