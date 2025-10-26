using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThemeDetail : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _themeName, _themePrice;
    [SerializeField] private Image _themeIcon;
    [SerializeField] private GameObject _themeProgress, _purchaseButton;
    [SerializeField] private CanvasGroup _canvasGroup;

    public void LoadThemeDetail(string themeName, string themePrice, Sprite themeIcon)
    {
        _themeName.text = themeName;
        _themePrice.text = themePrice;
        _themeIcon.sprite = themeIcon;

        var isUnlocked = PlayerPrefs.GetInt($"{themeName}Unlocked", 0) == 1;
        var isCompleted = PlayerPrefs.GetInt($"{themeName}Completed", 0) == 1;

        if (isUnlocked)
        {
            _themeProgress.SetActive(true);
            _purchaseButton.SetActive(false);

            ShowProgress(themeName);
        }
        else
        {
            _purchaseButton.GetComponentInChildren<TextMeshProUGUI>().text = themePrice;
        }

        if (isCompleted)
        {
            _canvasGroup.alpha = 0.5f;
            return;
        }

        GetComponent<Button>().onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySFX("PopUp_Toggle");

            if (isUnlocked)
            {
                SelectTheme();
            }
            else
            {
                UIManager.Instance.ShowPopUp("ThemePurchase", true, PopUpShowBehaviour.HIDE_PREVIOUS);
                ThemePurchase.Instance.LoadThemeInfo(themeName, themePrice, themeIcon);
            }
        });
    }

    private void SelectTheme()
    {
        AudioManager.Instance.PlaySFX("PopUp_Toggle");

        ThemedWordList.Instance.SelectTheme(_themeName.text);
        ThemedMode.Instance.UpdateThemeInfo(_themeName.text, _themeIcon.sprite);
        ThemedMode.Instance.ToggleGameUI(true);

        var currentWord = ThemedWordList.Instance.GetWord();
        Board.Instance.SetWord(currentWord);
        Board.Instance.SpawnRow(currentWord.Length);
    }

    public void Unlock(string themeName)
    {
        _themeProgress.SetActive(true);
        _purchaseButton.SetActive(false);

        ShowProgress(themeName);

        var button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(SelectTheme);
    }

    private void ShowProgress(string themeName)
    {
        var progress = PlayerPrefs.GetInt($"{themeName}Progress", 0);
        var total = ThemedWordList.Instance.GetThemeWordCount(themeName);

        _themeProgress.GetComponent<Slider>().value = progress / total;
        _themeProgress.GetComponentInChildren<TextMeshProUGUI>().text = Math.Abs(progress - total) < 1 ? "Completed" : $"{progress}/{total}";
    }
}
