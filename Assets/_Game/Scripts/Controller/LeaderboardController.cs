using System.Collections.Generic;
using Genix.MocaLib.Runtime.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardController : MonoBehaviour
{
    [SerializeField] private GameObject _frame;
    [SerializeField] private GameObject _framesContainer;
    [SerializeField] private MilestoneConfigs _milestoneConfigs;
    [SerializeField] private TextMeshProUGUI _playerName, _playerScore, _playerRank, _playerRankNumber;
    [SerializeField] private Image _playerAvatar;

    private List<GameObject> _frames = new();

    private void OnEnable()
    {
        ClearLeaderboard();
        FetchLeaderboard();
        LoadPlayerFrame();
    }

    private string GetRankName(int xp)
    {
        var currentRank = "Beginner I";
        var lastMilestone = 0;

        foreach (var milestone in _milestoneConfigs.Milestones)
        {
            if (xp > lastMilestone)
            {
                if (xp < milestone.RequiredExp)
                {
                    currentRank = milestone.RankName;
                }
                
                lastMilestone = milestone.RequiredExp;
            }
            else
            {
                break;
            }
        }

        return currentRank;
    }

    private void HandleTopScoresResult(List<LeaderboardEntry> topScores)
    {
        foreach (var leaderboardEntry in topScores)
        {
            var frame = Instantiate(_frame, _framesContainer.transform);
            _frames.Add(frame);

            var infoLoader = frame.GetComponent<LeaderboardFrame>();
            infoLoader.LoadInfo(leaderboardEntry.Score.ToString(), GetRankName(leaderboardEntry.Score), leaderboardEntry.DisplayName,
                (topScores.IndexOf(leaderboardEntry) + 1).ToString(), leaderboardEntry.Avatar, leaderboardEntry.UserId);
        }
    }

    private void FetchLeaderboard()
    {
        MocaLib.Instance.LeaderboardManager.GetTopScores(
            leaderboardName: Constants.LEADERBOARD_NAME,
            scoreName: Constants.SCORE_NAME,
            topCount: 10,
            onSuccess: HandleTopScoresResult,
            onError: (ex) =>
            {
                Debug.LogError($"Leaderboard error: {ex}");
            });
    }

    private void ClearLeaderboard()
    {
        foreach (var frame in _frames)
        {
            Destroy(frame);
        }
    }

    private void LoadPlayerFrame()
    {
        _playerName.text = ProfileManager.Instance.PlayerName;
        _playerAvatar.sprite = ProfileManager.Instance.PlayerAvatar;
        _playerScore.text = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_EXP, 0).ToString();
        _playerRank.text = GetRankName(PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_EXP, 0));

        MocaLib.Instance.LeaderboardManager.GetPlayerRank(
            leaderboardName: Constants.LEADERBOARD_NAME,
            scoreName: Constants.SCORE_NAME,
            PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_EXP, 0),
            onSuccess: (rank) =>
            {
                _playerRankNumber.text = $"No.{rank}";
            },
            onError: (ex) =>
            {
                Debug.LogError($"Leaderboard error: {ex}");
            }
        );
    }
}
