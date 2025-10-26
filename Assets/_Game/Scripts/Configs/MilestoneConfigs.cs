using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MilestoneConfigs", menuName = "ScriptableObjects/Milestone Configs", order = 1)]
public class MilestoneConfigs : ScriptableObject
{
    [Serializable]
    public class Reward
    {
        public RewardType Type;
        public int Amount;
    }

    [Serializable]
    public class Milestone
    {
        public string RankName;
        public int RequiredExp;
        public List<Reward> Rewards;
    }

    public List<Milestone> Milestones = new();
}