using UnityEngine;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private ToggleSwitch _sfx, _music;

    private void Start()
    {
        _sfx.GetPrefs(PlayerPrefs.GetInt(Constants.PLAYER_PREFS_SFX_ACTIVE, 1) == 1);
        _music.GetPrefs(PlayerPrefs.GetInt(Constants.PLAYER_PREFS_MUSIC_ACTIVE, 1) == 1);
    }

    public void OnSfxToggle(bool isOn)
    {
        AudioManager.Instance.ToggleSFX(isOn);
    }

    public void OnMusicToggle(bool isOn)
    {
        AudioManager.Instance.ToggleMusic(isOn);
    }
}
