using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Theme
{
    public string Name;
    public Sprite Sprite;
    public int Price;
}

public class ThemeSelectManager : Singleton<ThemeSelectManager>
{
    [SerializeField] private List<Theme> _themesList;
    [SerializeField] private GameObject _themePanelPrefab;

    private Dictionary<string, ThemeDetail> _themeInstancesDictionary = new();

    public void LoadThemes()
    {
        RegisterTheme();
    }

    private void RegisterTheme()
    {
        foreach (var theme in _themesList)
        {
            var themeInstance = Instantiate(_themePanelPrefab, transform);
            themeInstance.name = theme.Name;

            var themeDetail = themeInstance.GetComponent<ThemeDetail>();
            themeDetail.LoadThemeDetail(theme.Name, theme.Price.ToString(), theme.Sprite);
            _themeInstancesDictionary.Add(theme.Name, themeDetail);
        }
    }

    public void UnlockTheme(string themeName)
    {
        var themeInstance = _themeInstancesDictionary[themeName];
        themeInstance.Unlock(themeName);
    }
}
