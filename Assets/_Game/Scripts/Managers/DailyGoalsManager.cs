using System;
using System.Collections.Generic;
using UnityEngine;

public class DailyGoalManager : SingletonPersistent<DailyGoalManager>
{
    [SerializeField] private DailyGoals _goalPool;

    private DailyGoalsProgress _todayGoals;

    public void SaveGoals()
    {
        var json = JsonUtility.ToJson(_todayGoals);
        PlayerPrefs.SetString(Constants.DAILY_GOALS_KEY, json);
        PlayerPrefs.Save();
    }

    public void Initialize()
    {
        var today = DateTime.Now.ToString("yyyy-MM-dd");

        if (PlayerPrefs.HasKey(Constants.DAILY_GOALS_KEY))
        {
            var json = PlayerPrefs.GetString(Constants.DAILY_GOALS_KEY);
            _todayGoals = JsonUtility.FromJson<DailyGoalsProgress>(json);

            if (_todayGoals.Date == today)
                return;

            var yesterday = DateTime.Now.AddDays(-1);
            if (_todayGoals.Date == yesterday.ToString("yyyy-MM-dd") && IsAllCompleted())
            {
                GenerateNewGoals(today, _todayGoals.Streak + 1);
            }
            else
            {
                GenerateNewGoals(today, 0);
            }

            return;
        }

        GenerateNewGoals(today, 0);
    }

    private void GenerateNewGoals(string date, int streak)
    {
        _todayGoals = new()
        {
            Date = date,
            Streak = streak
        };

        var randomGoals = _goalPool.GetDailyGoals();

        foreach (var goal in randomGoals)
        {
            _todayGoals.Goals.Add(new()
            {
                GoalData = goal,
                Progress = 0,
                IsCompleted = false,
                IsClaimed = false
            });
        }

        SaveGoals();
    }

    public void UpdateProgress(GoalType type)
    {
        foreach (var goal in _todayGoals.Goals)
        {
            if (goal.GoalData.Type == type)
            {
                if (goal.IsCompleted)
                    return;

                goal.Progress++;

                if (goal.Progress == goal.GoalData.Amount)
                {
                    goal.IsCompleted = true;
                }

                break;
            }
        }

        SaveGoals();
    }

    public List<GoalProgress> GetGoals()
    {
        return _todayGoals.Goals;
    }

    public int GetCurrentStreak()
    {
        return _todayGoals.Streak;
    }

    public bool IsAllCompleted()
    {
        foreach (var goal in _todayGoals.Goals)
        {
            if (!goal.IsCompleted)
                return false;
        }

        return true;
    }
}

[Serializable]
public class DailyGoalsProgress
{
    public string Date;
    public int Streak;
    public List<GoalProgress> Goals = new();
}

[Serializable]
public class GoalProgress
{
    public GoalData GoalData;
    public int Progress;
    public bool IsCompleted;
    public bool IsClaimed;
}
