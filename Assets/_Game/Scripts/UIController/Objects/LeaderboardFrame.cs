using Genix.MocaLib.Runtime.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardFrame : MonoBehaviour
{
    [SerializeField] private GameObject _medal;
    [SerializeField] private Sprite _gold, _silver, _bronze;
    [SerializeField] private TextMeshProUGUI _xpText, _rankText, _nameText, _rankNumberText;
    [SerializeField] private Image _avatar;

    public void LoadInfo(string xpText, string rankText, string nameText, string rankNumberText, string avatar, string userId)
    {
        if (userId == MocaLib.Instance.PlayerProfileManager.CurrentProfile.UserId)
        {
            _nameText.text = nameText + " <color=#FF5500>(You)</color>";
        }
        else
        {
            _nameText.text = nameText;
        }
        
        _xpText.text = xpText;
        _rankText.text = rankText;
        _rankNumberText.text = $"No.{rankNumberText}";
        
        int.TryParse(avatar[6..], out var avatarId);
        _avatar.sprite = ProfileManager.Instance.ProfileImagesList[avatarId];

        if (int.Parse(rankNumberText) <= 3)
        {
            _medal.SetActive(true);

            var rank = int.Parse(rankNumberText);
            _medal.GetComponent<Image>().sprite = rank switch
            {
                1 => _gold,
                2 => _silver,
                3 => _bronze,
                _ => null
            };
        }
        else
        {
            _medal.SetActive(false);
        }
    }
}
