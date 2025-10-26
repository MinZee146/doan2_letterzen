using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GoalData
{
    public GoalType Type;
    public int Amount;
    public int Coin;
    public string Description;
}

[CreateAssetMenu(fileName = "GoalPool", menuName = "ScriptableObjects/Goal Pool", order = 1)]
public class DailyGoals : ScriptableObject
{
    public List<GoalData> GoalList;

    public List<GoalData> GetDailyGoals()
    {
        List<GoalData> shuffled = new(GoalList);

        for (var i = shuffled.Count - 1; i > 0; i--)
        {
            var j = UnityEngine.Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        List<GoalData> uniqueGoals = new();
        HashSet<GoalType> usedTypes = new();

        foreach (var goal in shuffled)
        {
            if (!usedTypes.Contains(goal.Type))
            {
                uniqueGoals.Add(goal);
                usedTypes.Add(goal.Type);
                Debug.Log($"Added goal: {goal.Description}");

                if (uniqueGoals.Count >= 3)
                    break;
            }
        }

        return uniqueGoals;
    }
}

public enum GoalType
{
    UseHint,
    CompleteLadderWords,
    CompleteThemedWords,
    CompleteLevelWords
}
