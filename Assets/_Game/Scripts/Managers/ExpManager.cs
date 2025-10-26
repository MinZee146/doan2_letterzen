using System;
using DamageNumbersPro;
using UnityEngine;

public class ExpManager : SingletonPersistent<ExpManager>
{
    [SerializeField] private MilestoneConfigs _milestoneConfigs;

    [SerializeField] private DamageNumber _expPrefab;

    public int CurrentXP => _currentExp;

    private int _currentExp, _pendingExp, _currentMilestone;

    public void Initialize()
    {
        _currentExp = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_EXP, 0);
        _currentMilestone = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_MILESTONE, 0);
        _pendingExp = 0;
    }

    public void SpawnExp(RectTransform rect, Vector3 position, int amount)
    {
        _expPrefab.SpawnGUI(rect, position, amount);
        _pendingExp += amount;
    }

    public void IncreaseExp()
    {
        if (_pendingExp == 0) return;

        _currentExp += _pendingExp;
        _pendingExp = 0;

        SaveData();
        
        ProfileManager.Instance.PushCurrentProgress();
    }

    public void CheckForMilestoneRewards()
    {
        if (_currentMilestone >= _milestoneConfigs.Milestones.Count) return;

        var currentMilestone = _milestoneConfigs.Milestones[_currentMilestone];
        if (_currentExp >= currentMilestone.RequiredExp)
        {
            foreach (var reward in currentMilestone.Rewards)
            {
                RewardManager.Instance.AddReward(reward.Type, reward.Amount);
            }

            _currentMilestone++;
            SaveData();
        }
    }

    public (float, float, bool) CalculateIncreaseExp()
    {
        var prevExp = _currentMilestone > 0
                    ? _milestoneConfigs.Milestones[_currentMilestone - 1].RequiredExp
                    : 0;
        var currExp = _milestoneConfigs.Milestones.Count > _currentMilestone
                    ? _milestoneConfigs.Milestones[_currentMilestone].RequiredExp
                    : prevExp;
        var nextExp = _milestoneConfigs.Milestones.Count > _currentMilestone + 1
                    ? _milestoneConfigs.Milestones[_currentMilestone + 1].RequiredExp
                    : currExp;

        var range = currExp - prevExp;
        var nextRange = nextExp - currExp;
        var finalExp = _currentExp + _pendingExp;

        if (finalExp <= currExp)
        {
            var startRatio = (_currentExp - prevExp) / (float)range;
            var endRatio = (finalExp - prevExp) / (float)range;
            return (Mathf.Clamp01(startRatio), Mathf.Clamp01(endRatio), finalExp == currExp);
        }
        else
        {
            var startRatio = (_currentExp - prevExp) / (float)range;
            var endRatio = (finalExp - currExp) / (float)nextRange;
            return (Mathf.Clamp01(startRatio), Mathf.Clamp01(endRatio), true);
        }
    }

    public string GetRank()
    {
        return _milestoneConfigs.Milestones[Math.Min(_currentMilestone, _milestoneConfigs.Milestones.Count - 1)].RankName;
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(Constants.PLAYER_PREFS_CURRENT_EXP, _currentExp);
        PlayerPrefs.SetInt(Constants.PLAYER_PREFS_CURRENT_MILESTONE, _currentMilestone);
        PlayerPrefs.Save();
    }
}