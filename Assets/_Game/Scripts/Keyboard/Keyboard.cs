using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Keyboard : Singleton<Keyboard>
{
    [SerializeField] private List<Key> _keys;
    [SerializeField] private GameObject _submitButton;
    [SerializeField] private Color _yellowKey, _greenKey, _greyKey;

    public void Validate(string currentWord, string guessedWord)
    {
        var seq = DOTween.Sequence();
        var evaluatedLetters = new HashSet<char>();

        foreach (var letter in guessedWord)
        {
            if (evaluatedLetters.Contains(letter)) continue;
            evaluatedLetters.Add(letter);

            var key = _keys.Find(k => k.name == letter.ToString());
            if (key == null) continue;

            var currentColor = key.GetComponent<Image>().color;
            var bestColor = _greyKey;

            for (var i = 0; i < guessedWord.Length; i++)
            {
                if (guessedWord[i] == letter && currentWord[i] == letter)
                {
                    bestColor = _greenKey;
                    break;
                }
            }

            if (bestColor != _greenKey && currentWord.Contains(letter))
            {
                bestColor = _yellowKey;
            }

            var canUpdate =
                (bestColor == _greenKey && currentColor != _greenKey) ||
                (bestColor == _yellowKey && currentColor != _greenKey && currentColor != _yellowKey) ||
                (bestColor == _greyKey && currentColor != _greenKey && currentColor != _yellowKey);

            if (canUpdate)
            {
                seq.Join(key.Validate(bestColor));
            }
        }

        seq.OnComplete(() =>
        {
            BoosterManager.Instance.UpdateClearButtonState();
        });
    }

    public void Reset()
    {
        foreach (var key in _keys)
        {
            key.Reset();
        }
    }

    public void ToggleSubmitButtonState(string state)
    {
        var btnColor = Color.white;
        var btnTextColor = Color.white;

        switch (state)
        {
            case "Submit":
                ColorUtility.TryParseHtmlString("#66FF30", out btnColor);
                ColorUtility.TryParseHtmlString("#476475", out btnTextColor);
                _submitButton.GetComponent<Button>().interactable = true;
                _submitButton.GetComponentInChildren<TextMeshProUGUI>().text = "SUBMIT";

                break;
            case "Erase":
                ColorUtility.TryParseHtmlString("#FF7F00", out btnColor);
                _submitButton.GetComponent<Button>().interactable = true;
                _submitButton.GetComponentInChildren<TextMeshProUGUI>().text = "ERASE";

                break;
            case "Disable":
                ColorUtility.TryParseHtmlString("#476475", out btnTextColor);
                _submitButton.GetComponent<Button>().interactable = false;
                _submitButton.GetComponentInChildren<TextMeshProUGUI>().text = "SUBMIT";

                break;
        }

        _submitButton.GetComponent<Image>().DOColor(btnColor, 0.2f);
        _submitButton.GetComponentInChildren<TextMeshProUGUI>().DOColor(btnTextColor, 0.2f);
    }

    public void DisableInvalidKeys()
    {
        var keys = _keys
        .FindAll(k => k.GetComponent<Image>().color == Color.white && !Board.Instance.CurrentWord.Contains(k.name))
        .OrderBy(k => Random.value)
        .ToList();

        var seq = DOTween.Sequence();
        foreach (var keyObj in keys)
        {
            var key = keyObj.GetComponent<Key>();
            seq.Join(key.Validate(_greyKey));
        }

        seq.OnComplete(() =>
        {
            BoardSave.Instance.SaveBoard();
            BoosterManager.Instance.UpdateClearButtonState();
        });
    }

    public bool IsAllGrey()
    {
        return _keys.Where(k => !Board.Instance.CurrentWord.Contains(k.name))
                    .All(k => k.GetComponent<Image>().color == _greyKey);
    }

    public List<KeyState> GetKeyStates()
    {
        var keyStates = new List<KeyState>();

        foreach (var key in _keys)
        {
            var keyImage = key.GetComponent<Image>();
            var color = keyImage.color;
            var hex = ColorUtility.ToHtmlStringRGB(color);

            keyStates.Add(new KeyState
            {
                Key = key.name.ToLower(),
                ColorHex = "#" + hex
            });
        }

        return keyStates;
    }

    public void LoadKeyStates(List<KeyState> states)
    {
        var seq = DOTween.Sequence();
        foreach (var state in states)
        {
            var key = _keys.Find(k => k.name.ToLower() == state.Key);
            if (key == null) continue;


            if (ColorUtility.TryParseHtmlString(state.ColorHex, out var color))
            {
                seq.Join(key.Validate(color));
            }
        }

        seq.OnComplete(() =>
        {
            BoosterManager.Instance.UpdateClearButtonState();
        });
    }

    public void HighlightWord(string words = "")
    {
        foreach (var keyObj in _keys)
        {
            var key = keyObj.GetComponent<Key>();

            if (words.Contains(key.name) || words == "")
            {
                key.SetInteractable(true);
            }
            else
            {
                key.SetInteractable(false);
                key.Reset();
            }
        }
    }
}