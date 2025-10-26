using System.Collections.Generic;
using Genix.MocaLib.Runtime.Services;
using MEC;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ProfileManager : SingletonPersistent<ProfileManager>
{
    private List<Sprite> _profileImagesList;
    private Sprite _playerAvatar;
    private string _playerName;

    public List<Sprite> ProfileImagesList => _profileImagesList;
    public Sprite PlayerAvatar => _playerAvatar;
    public string PlayerName => _playerName;

    public void Initialize()
    {
        Timing.RunCoroutine(LoadImages());
    }

    private IEnumerator<float> LoadImages()
    {
        var handle = Addressables.LoadAssetsAsync<Sprite>(Constants.AVATAR_KEY);

        while (!handle.IsDone)
            yield return Timing.WaitForOneFrame;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _profileImagesList = new List<Sprite>(handle.Result);
            LoadProfile();
        }
        else
        {
            Debug.LogError("Failed to load profile images.");
        }
    }

    public void UpdateProfile()
    {
        _playerAvatar = _profileImagesList[MocaLib.Instance.PlayerProfileManager.GetLocalAvatarId()];
        _playerName = MocaLib.Instance.PlayerProfileManager.CurrentProfile.DisplayName;
    }

    public void PushCurrentProgress()
    {
        MocaLib.Instance.LeaderboardManager.SubmitScore(
            leaderboardName: Constants.LEADERBOARD_NAME,
            scoreName: Constants.SCORE_NAME,
            newScore: ExpManager.Instance.CurrentXP,
            onSuccess: () => Debug.Log("Score submitted successfully"),
            onError: (ex) => Debug.LogError($"Failed to submit score: {ex}")
        );
    }

    private void LoadProfile()
    {
        if (string.IsNullOrEmpty(MocaLib.Instance.PlayerProfileManager.CurrentProfile.UserId)) return;
        MocaLib.Instance.PlayerProfileManager.LoadProfile(
            onSuccess: UpdateProfile,
            onError: (ex) =>
            {
                Debug.LogError($"Failed to load profile: {ex}");
            });
    }
}
