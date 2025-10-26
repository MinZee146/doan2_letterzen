using System.Collections.Generic;
using DG.Tweening;
using MEC;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Board : Singleton<Board>
{
    [SerializeField] private GameObject _row;
    [SerializeField] private Sprite _white, _green, _yellow, _grey;
    [SerializeField] private VerticalLayoutGroup _layoutGroup;

    private Dictionary<string, Sprite> _spriteDict;
    private List<Row> _rows = new();

    private BoardState _loadedState;
    private int _currentRowIndex, _timesGuessed = 1;
    private string _currentWord;
    private bool _isAnimating;

    public int TimesGuessed => _timesGuessed;
    public int CurrentRowIndex => _currentRowIndex;
    public string CurrentWord => _currentWord;

    #region Initialize
    public List<Row> GetRows()
    {
        return _rows;
    }

    public void ResetBoard(bool isRetry = false)
    {
        foreach (var row in _rows)
        {
            Destroy(row.gameObject);
        }

        _currentRowIndex = 0;
        _currentWord = isRetry ? _currentWord : null;
        _timesGuessed = isRetry ? _timesGuessed + 1 : 1;
        _rows.Clear();
    }

    public void SpawnRow(int numberOfTiles)
    {
        _layoutGroup.enabled = true;
        LoadBoard();

        var sequence = DOTween.Sequence();
        for (var i = 0; i < 6; i++)
        {
            var rowInstance = Instantiate(_row, transform, false);
            var row = rowInstance.GetComponent<Row>();
            var canvasGroup = rowInstance.GetComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
            row.SpawnTiles(numberOfTiles);
            row.SetCurrentRow(i <= _currentRowIndex);
            row.SetCurrentTile(i == _currentRowIndex && _currentRowIndex < 6);
            _rows.Add(row);

            LoadRowFromState(i);
            sequence.AppendCallback(() =>
            {
                rowInstance.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
                rowInstance.transform.GetComponent<CanvasGroup>().DOFade(1f, 0.25f).SetEase(Ease.OutBack);
                AudioManager.Instance.PlaySFX("Game_Spawn");
            });

            sequence.AppendInterval(0.05f);
        }

        sequence.OnComplete(() =>
        {
            RevealLetter();

            if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.LevelMode)
            {
                TutorialController.Instance.Tutorial();
            }

            if (_loadedState.IsUnlockedDefinition)
            {
                Definition.Instance.UnlockDefinition(wasSaved: true);
            }

            if (_currentRowIndex == 6)
            {
                UIManager.Instance.ShowPopUp("WordFailed", true, PopUpShowBehaviour.HIDE_PREVIOUS);
            }
        });
    }

    private IEnumerator<float> DespawnUnusedRows()
    {
        _layoutGroup.enabled = false;

        for (var i = _rows.Count - 1; i > _currentRowIndex; i--)
        {
            AudioManager.Instance.PlaySFX("Common_Pop");

            var row = _rows[i];
            var rowTransform = row.GetComponent<RectTransform>();
            var canvasGroup = row.GetComponent<CanvasGroup>();

            var rowHeight = rowTransform.rect.height;
            var slideAmount = (rowHeight + 5f) / 2f;
            var tweenCompleted = false;
            var expPerRow = _timesGuessed == 1 ? 100 : 50;

            ExpManager.Instance.SpawnExp(GetComponent<RectTransform>(), rowTransform.localPosition, expPerRow);
            DOTween.Sequence()
            .Join(canvasGroup.DOFade(0f, 0.5f).SetEase(Ease.InOutSine))
            .Join(rowTransform.DOScale(0f, 0.5f).SetEase(Ease.InBack))
            .OnComplete(() => tweenCompleted = true);

            for (var j = 0; j < i; j++)
            {
                var rtf = _rows[j].GetComponent<RectTransform>();
                rtf.DOAnchorPos(rtf.anchoredPosition - new Vector2(0, slideAmount), 0.5f)
                   .SetEase(Ease.InOutSine);
            }

            while (!tweenCompleted) yield return Timing.WaitForOneFrame;

            Destroy(row.gameObject);
            _rows.RemoveAt(i);
        }
    }

    public GameObject GetCurrentRow()
    {
        return _currentRowIndex >= _rows.Count ? _rows[_currentWord.Length - 1].gameObject : _rows[_currentRowIndex].gameObject;
    }
    #endregion

    #region Gameplay
    public void SetWord(string word)
    {
        if (word == null)
        {
            _currentWord = null;
            return;
        }

        _currentWord = word.ToUpper();
        BoosterManager.Instance.Reset();
    }

    private IEnumerator<float> HandleCompletedWord()
    {
        yield return Timing.WaitUntilDone(Timing.RunCoroutine(DespawnUnusedRows()));
        UIManager.Instance.ShowPopUp("WordCompleted", true, PopUpShowBehaviour.HIDE_PREVIOUS);

        _isAnimating = false;
    }

    private void HandleFailedWord()
    {
        UIManager.Instance.ShowPopUp("WordFailed", true, PopUpShowBehaviour.HIDE_PREVIOUS);
        _currentRowIndex++;
    }

    public void EnterNextChar(char c)
    {
        if (string.IsNullOrEmpty(_currentWord)) return;
        if (_currentRowIndex >= _rows.Count || _isAnimating) return;

        _rows[_currentRowIndex].EnterNextChar(c);
        UpdateSubmitButtonState();
    }

    private void EnterNextRow()
    {
        _currentRowIndex++;
        _rows[_currentRowIndex].SetCurrentRow(true);
        _rows[_currentRowIndex].SetCurrentTile(true);

        RevealLetter();
    }

    public void DeleteChar()
    {
        if (string.IsNullOrEmpty(_currentWord)) return;
        if (_rows[_currentRowIndex].CurrentIndex() <= 0 || _isAnimating) return;

        _rows[_currentRowIndex].DeleteChar();
    }

    public void SubmitWord()
    {
        if (_isAnimating) return;

        var word = _rows[_currentRowIndex].GetCurrentWord();
        var mode = GameManager.Instance.CurrentGameMode;

        if (mode == GameManager.GameMode.LevelMode && TutorialController.Instance.IsForcedWord(word))
        {
            _rows[_currentRowIndex].Reset();
            return;
        }

        var isValid = mode == GameManager.GameMode.ThemedMode || Dictionary.Instance.CheckWord(word);
        if (isValid)
        {
            _isAnimating = true;
            _rows[_currentRowIndex].Validate();

            if (word == _currentWord)
            {
                AudioManager.Instance.PlaySFX("Correct_Word");
            }
            else
            {
                AudioManager.Instance.PlaySFX("Valid_Word");
            }

            Keyboard.Instance.Validate(_currentWord, word);
        }
        else
        {
            _rows[_currentRowIndex].Reset();
            RevealLetter();
        }
    }

    public void CheckWord()
    {
        var submittedWord = _rows[_currentRowIndex].GetCurrentWord();
        if (submittedWord == _currentWord)
        {
            if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.LevelMode)
            {
                TutorialController.Instance.ShowInstruction(_currentRowIndex + 1, true);
            }

            BoosterManager.Instance.DisableBoosters();
            Definition.Instance.DisableDefinition();
            Timing.RunCoroutine(HandleCompletedWord());
        }
        else
        {
            _isAnimating = false;

            if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.LevelMode)
            {
                TutorialController.Instance.ShowInstruction(_currentRowIndex + 1);
            }

            if (_currentRowIndex < _rows.Count - 1)
            {
                EnterNextRow();
            }
            else
            {
                HandleFailedWord();
            }
        }
    }

    public void ProceedToNextWord()
    {
        Keyboard.Instance.Reset();
        Definition.Instance.ClearDefinition();
        ResetBoard();

        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.LevelMode)
        {
            LevelWordList.Instance.ProceedToNextWord();
        }
        else
        {
            ThemedWordList.Instance.ProceedToNextWord();
        }

        if (!string.IsNullOrEmpty(_currentWord)) SpawnRow(_currentWord.Length);
    }
    #endregion

    #region Boosters
    public void RevealLetter(int index = -1)
    {
        if (index == -1)
        {
            for (var i = 0; i < _currentWord.Length; i++)
            {
                if (BoosterManager.Instance.LettersRevealed[i]) RevealLetter(i);
            }
        }
        else
        {
            if (_currentRowIndex < 6)
            {
                _rows[_currentRowIndex].RevealLetter(index, _currentWord[index]);
            }
        }
    }

    public void UpdateSubmitButtonState()
    {
        var word = _rows[_currentRowIndex].GetCurrentWord();
        var mode = GameManager.Instance.CurrentGameMode;

        if (word.Length != _currentWord.Length)
        {
            Keyboard.Instance.ToggleSubmitButtonState("Disable");
            return;
        }

        if (mode == GameManager.GameMode.LevelMode && TutorialController.Instance.IsForcedWord(word))
        {
            _rows[_currentRowIndex].AnimateInvalidWord();
            Keyboard.Instance.ToggleSubmitButtonState("Erase");
            AudioManager.Instance.PlaySFX("Invalid_Word");
            return;
        }

        if (mode == GameManager.GameMode.ThemedMode || Dictionary.Instance.CheckWord(word))
        {
            Keyboard.Instance.ToggleSubmitButtonState("Submit");
        }
        else
        {
            _rows[_currentRowIndex].AnimateInvalidWord();
            Keyboard.Instance.ToggleSubmitButtonState("Erase");
            AudioManager.Instance.PlaySFX("Invalid_Word");
        }
    }
    #endregion

    #region BoardSave
    private void LoadRowFromState(int rowIndex)
    {
        if (_loadedState.Rows == null || rowIndex >= _loadedState.Rows.Count || (rowIndex == _currentRowIndex && _currentRowIndex < 6))
        {
            return;
        }

        var row = _rows[rowIndex];
        var rowState = _loadedState.Rows[rowIndex];

        for (var j = 0; j < rowState.Tiles.Count; j++)
        {
            var tile = row.GetTiles()[j];
            var tileState = rowState.Tiles[j];

            var img = tile.GetComponent<Image>();
            var text = tile.GetComponentInChildren<TextMeshProUGUI>();
            var canvasGroup = tile.GetComponent<CanvasGroup>();

            img.sprite = _spriteDict[tileState.SpriteName];
            text.text = tileState.Text;
            canvasGroup.alpha = tileState.Alpha;
        }
    }

    private void LoadBoard()
    {
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.LevelMode &&
            !TutorialController.Instance.IsTutorialCompleted) return;

        _spriteDict = new Dictionary<string, Sprite>
        {
            { _white.name, _white },
            { _green.name, _green },
            { _yellow.name, _yellow },
            { _grey.name, _grey }
        };

        _loadedState = BoardSave.Instance.LoadBoardFile();
        if (_loadedState.Rows == null) return;

        _timesGuessed = _loadedState.TimesGuessed;
        _currentRowIndex = _loadedState.CurrentRowIndex;

        BoosterManager.Instance.MatchedPositions = _loadedState.MatchedPositions.ToArray();
        BoosterManager.Instance.LettersRevealed = _loadedState.LettersRevealed.ToArray();
        BoosterManager.Instance.UpdateRevealButtonState();
        Keyboard.Instance.LoadKeyStates(_loadedState.Keyboard);
    }
    #endregion
}
