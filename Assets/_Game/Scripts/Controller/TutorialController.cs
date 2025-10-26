using MEC;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public enum HoleShape { Circle, Rectangle }

public class TutorialController : Singleton<TutorialController>
{
    [SerializeField] private Material _holeMaterial;
    [SerializeField] private GameObject _overlayImage, _topInstruction, _botInstruction;
    [SerializeField] private float _paddingFactor = 1.1f;

    private CoroutineHandle _highlightHandle;
    private Tween _highlightTween;
    private bool _isHighlighting;
    private string _forcedWord;

    public bool IsTutorialCompleted { get; private set; }

    public void Initialize()
    {
        IsTutorialCompleted = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_IS_TUTORIAL_COMPLETED, 0) == 1;
    }

    #region Tutorial
    public void Tutorial()
    {
        if (IsTutorialCompleted) return;

        LevelMode.Instance.UpdateUIState();
        Timing.CallDelayed(0f, () =>
        {
            var currentLevel = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_LEVEL, 0) + 1;
            switch (currentLevel)
            {
                case 1: RevealLetter(1); break;
                case 3: HandleButtonTutorial(LevelMode.Instance.GetRevealBtn(), HoleShape.Circle); break;
                case 4: HandleButtonTutorial(LevelMode.Instance.GetClearBtn(), HoleShape.Circle); break;
                case 5: HandleButtonTutorial(LevelMode.Instance.GetDefinitionBtn(), HoleShape.Rectangle); break;
                case 6:
                    IsTutorialCompleted = true;
                    PlayerPrefs.SetInt(Constants.PLAYER_PREFS_IS_TUTORIAL_COMPLETED, 1);
                    PlayerPrefs.Save();

                    BoosterManager.Instance.Reset();
                    Definition.Instance.SetDefinitionPrice();
                    UIManager.Instance.ShowCoinBar();
                    break;
            }

            ShowInstruction(0);
        });
    }

    public void ShowInstruction(int index, bool isCompletedWord = false)
    {
        if (IsTutorialCompleted) return;

        var instructions = GetInstructionsForLevel(PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_LEVEL, 0) + 1);
        if (instructions == null || index >= instructions.Count || isCompletedWord)
        {
            HideInstruction();
            return;
        }

        var (topText, topY, botText, botY, highlight) = instructions[index];
        SetInstructionState(_topInstruction, topY, topText);
        SetInstructionState(_botInstruction, botY, botText);

        Keyboard.Instance.HighlightWord(highlight);
        _forcedWord = highlight;
    }

    private List<(string top, int topY, string bottom, int botY, string word)> GetInstructionsForLevel(int level)
    {
        return level switch
        {
            1 => new()
            {
                ("Guess a meaningful 4-letter word!", -215, "L is in the right place.\nType <wave a=0.1><b><color=#FA3232>LEAD</color></b></> to see what changes!", 575, "LEAD"),
                ("Guess a meaningful 4-letter word!", -215, "E is in the word but in the wrong spot.\nType <wave a=0.1><b><color=#FA3232>LIVE</color></b></> to move E around!", 575, "LIVE"),
                ("Guess a meaningful 4-letter word!", -215, "With L, V, and E, what else could it be?\nAll signs point to <wave a=0.1><b><color=#FA3232>LOVE</color></b></>!", 575, "LOVE"),
            },
            2 => new()
            {
                ("Guess a meaningful 4-letter word!", -215, "Start with a bit of luck!\nTry guessing <wave a=0.1><b><color=#FA3232>POET</color></b></>!", 575, "POET"),
                ("Guess a meaningful 4-letter word!", -215, "Looks like E and T are in the word!\nNow try <wave a=0.1><b><color=#FA3232>TEAM</color></b></>!", 575, "TEAM"),
                ("Guess a meaningful 4-letter word!", -215, "Great! T is in the correct spot.\nLet's try <wave a=0.1><b><color=#FA3232>TIME</color></b></> next!", 575, "TIME"),
            },
            3 or 4 or 5 => BoostersInstruction(level),
            _ => null
        };
    }

    private List<(string, int, string, int, string)> BoostersInstruction(int level)
    {
        var word = Board.Instance.CurrentWord;
        var toolInstruction = level switch
        {
            3 => "Hit the Magnifying icon to\n show a letter from the word!",
            4 => "Click the Bulb icon to\nclear wrong letters from the keyboard!",
            5 => "Tap the Definition button to\nget a clue about the word!",
            _ => ""
        };

        return new()
        {
            (level == 5 ? toolInstruction : "", level == 5 ? -325 : 0, level != 5 ? toolInstruction : "", 725, ""),
            ("", 0, "", 0, ""),
            ("", 0, "", 0, ""),
            ("", 0, "", 0, ""),
            ("", 0, "", 0, ""),
            ("", 0, $"I can see it - You were so close!\nThe word is <wave a=0.1><b><color=#FA3232>{word}</color></b></>! Give it a shot!", 520, word),
        };
    }

    private void SetInstructionState(GameObject instruction, int y, string text = "")
    {
        var hasText = !string.IsNullOrEmpty(text);
        var isActive = instruction.activeSelf;
        var textComponent = instruction.GetComponentInChildren<TextMeshProUGUI>();

        if (hasText == isActive)
        {
            if (hasText) textComponent.text = text;
            return;
        }

        var canvasGroup = instruction.GetComponent<CanvasGroup>();
        var rectTransform = instruction.GetComponent<RectTransform>();

        if (hasText)
        {
            instruction.SetActive(true);
            textComponent.text = text;
            rectTransform.anchoredPosition = new Vector3(instruction.transform.localPosition.x, y, instruction.transform.localPosition.z);

            canvasGroup.alpha = 0f;
            instruction.transform.localScale = Vector3.zero;

            canvasGroup.DOFade(1f, 0.5f).SetEase(Ease.OutQuad);
            instruction.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
        }
        else
        {
            canvasGroup.DOFade(0f, 0.5f).SetEase(Ease.InQuad);
            instruction.transform.DOScale(0f, 0.5f).SetEase(Ease.InBack).OnComplete(() => instruction.SetActive(false));
        }
    }

    private void HideInstruction()
    {
        AnimateHide(_topInstruction);
        AnimateHide(_botInstruction);
        Keyboard.Instance.HighlightWord();
    }

    private void AnimateHide(GameObject instruction)
    {
        if (!instruction.activeSelf) return;

        var canvasGroup = instruction.GetComponent<CanvasGroup>();
        canvasGroup.DOFade(0f, 0.2f).SetEase(Ease.InQuad);
        instruction.transform.DOScale(0f, 0.5f).SetEase(Ease.InBack).OnComplete(() => instruction.SetActive(false));
    }

    public bool IsForcedWord(string word)
    {
        return !IsTutorialCompleted && !string.IsNullOrEmpty(_forcedWord) && word != _forcedWord;
    }

    private void RevealLetter(int numberOfLetters)
    {
        if (Board.Instance.CurrentRowIndex != 0) return;

        for (int i = 0; i < numberOfLetters; i++)
        {
            BoosterManager.Instance.LettersRevealed[0] = true;
        }

        Board.Instance.RevealLetter();
    }
    #endregion

    #region Highlight
    private void HandleButtonTutorial(GameObject button, HoleShape shape)
    {
        if (!button) return;

        var rect = button.GetComponent<RectTransform>();
        var btn = button.GetComponent<Button>();

        ShowHighlight(rect, shape);

        void btnAction()
        {
            HideHighlight();
            ShowInstruction(Board.Instance.CurrentWord.Length);
            btn.onClick.RemoveListener(btnAction);
        }

        btn.onClick.AddListener(btnAction);
    }

    private void ShowHighlight(RectTransform target, HoleShape shape = HoleShape.Circle)
    {
        _overlayImage.SetActive(true);
        var raycast = _overlayImage.GetComponent<UIRaycastHole>();
        raycast.HoleTarget = target;
        raycast.Padding = new Vector2(target.rect.width, target.rect.height) * ((_paddingFactor - 1f) / 2f);

        if (_isHighlighting)
        {
            Timing.KillCoroutines(_highlightHandle);
            _highlightTween?.Kill();
        }

        _highlightHandle = Timing.RunCoroutine(AnimateShowHighlight(target, shape));
    }

    private void HideHighlight()
    {
        _highlightTween?.Kill();
        if (_isHighlighting) Timing.KillCoroutines(_highlightHandle);
        _highlightHandle = Timing.RunCoroutine(AnimateHideHighlight());
    }

    private IEnumerator<float> AnimateShowHighlight(RectTransform target, HoleShape shape)
    {
        _isHighlighting = true;

        var screenPos = RectTransformUtility.WorldToScreenPoint(null, target.position);
        var viewPos = new Vector2(screenPos.x / Screen.width, screenPos.y / Screen.height);
        var done = false;

        _holeMaterial.SetVector("_HoleCenter", new Vector4(viewPos.x, viewPos.y, 0, 0));
        _holeMaterial.SetInt("_HoleShape", shape == HoleShape.Circle ? 0 : 1);

        if (shape == HoleShape.Circle)
        {
            var radius = 1.2f;
            var targetRadius = CalcRadius(target);

            _highlightTween = DOTween.To(() => radius, r =>
            {
                radius = r;
                _holeMaterial.SetFloat("_HoleRadius", radius);
            }, targetRadius, 0.5f).SetEase(Ease.OutCubic).OnComplete(() => done = true);
        }
        else
        {
            var size = new Vector2(1.2f, 1.2f);
            var targetSize = CalcSize(target);

            _highlightTween = DOTween.To(() => size, s =>
            {
                size = s;
                _holeMaterial.SetVector("_HoleSize", new Vector4(s.x, s.y, 0, 0));
            }, targetSize, 0.5f).SetEase(Ease.OutCubic).OnComplete(() => done = true);
        }

        while (!done) yield return 0f;
        _isHighlighting = false;
    }

    private IEnumerator<float> AnimateHideHighlight()
    {
        _isHighlighting = true;

        var shape = _holeMaterial.GetInt("_HoleShape");
        var done = false;

        if (shape == 0)
        {
            var radius = _holeMaterial.GetFloat("_HoleRadius");

            _highlightTween = DOTween.To(() => radius, r =>
            {
                radius = r;
                _holeMaterial.SetFloat("_HoleRadius", radius);
            }, 1.2f, 0.5f).SetEase(Ease.InCubic).OnComplete(() =>
            {
                _overlayImage.SetActive(false);
                done = true;
            });
        }
        else
        {
            var size = new Vector2(_holeMaterial.GetVector("_HoleSize").x, _holeMaterial.GetVector("_HoleSize").y);

            _highlightTween = DOTween.To(() => size, s =>
            {
                size = s;
                _holeMaterial.SetVector("_HoleSize", new Vector4(s.x, s.y, 0, 0));
            }, new(1.2f, 1.2f), 0.5f).SetEase(Ease.InCubic).OnComplete(() =>
            {
                _overlayImage.SetActive(false);
                done = true;
            });
        }

        while (!done) yield return 0f;
        _isHighlighting = false;
    }

    private float CalcRadius(RectTransform target)
    {
        var w = target.rect.width / Screen.width;
        var h = target.rect.height / Screen.height;
        return Mathf.Max(w, h) * 0.5f * _paddingFactor;
    }

    private Vector2 CalcSize(RectTransform target)
    {
        var aspect = (float)Screen.width / Screen.height;
        var halfW = target.rect.width * 0.5f * _paddingFactor / Screen.width;
        var halfH = target.rect.height * 0.5f * _paddingFactor / Screen.height;
        return new(halfW * aspect, halfH);
    }

    #endregion
}
