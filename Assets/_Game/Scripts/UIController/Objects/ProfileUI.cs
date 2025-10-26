using System.Collections.Generic;
using Genix.MocaLib.Runtime.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nameInputField;
    [SerializeField] private Image _profileImage;

    private List<Sprite> _profileImagesList;
    private int _currentKey;

    private void Start()
    {
        _currentKey = MocaLib.Instance.PlayerProfileManager.GetLocalAvatarId();
        _profileImagesList = ProfileManager.Instance.ProfileImagesList;
        _profileImage.sprite = ProfileManager.Instance.PlayerAvatar;
        _nameInputField.text = ProfileManager.Instance.PlayerName;
    }

    public void ToPrevious()
    {
        AudioManager.Instance.PlaySFX("Common_Swoosh");

        if (_currentKey > 0)
        {
            _currentKey--;
        }
        else
        {
            _currentKey = _profileImagesList.Count - 1;
        }

        _profileImage.sprite = _profileImagesList[_currentKey];
    }

    public void ToNext()
    {
        AudioManager.Instance.PlaySFX("Common_Swoosh");

        if (_currentKey < _profileImagesList.Count - 1)
        {
            _currentKey++;
        }
        else
        {
            _currentKey = 0;
        }

        _profileImage.sprite = _profileImagesList[_currentKey];
    }

    public void Save()
    {
        MocaLib.Instance.PlayerProfileManager.SetDisplayName(_nameInputField.text);
        MocaLib.Instance.PlayerProfileManager.SetLocalAvatar(_currentKey);
        MocaLib.Instance.PlayerProfileManager.SaveProfile(
            onSuccess: () =>
            {
                Debug.Log("Profile Saved");
            },
            onError: Debug.LogError);

        MocaLib.Instance.LeaderboardManager.UpdatePlayerProfile(
            leaderboardName: Constants.LEADERBOARD_NAME,
            scoreName: Constants.SCORE_NAME,
            displayName: MocaLib.Instance.PlayerProfileManager.CurrentProfile.DisplayName,
            avatar: MocaLib.Instance.PlayerProfileManager.CurrentProfile.Avatar,
            onSuccess: () => Debug.Log("Score submitted successfully"),
            onError: (ex) => Debug.LogError($"Failed to submit score: {ex}"));

        ProfileManager.Instance.UpdateProfile();
        MainMenu.Instance.UpdateAvatar();
        AudioManager.Instance.PlaySFX("Common_Completed");
    }
}
