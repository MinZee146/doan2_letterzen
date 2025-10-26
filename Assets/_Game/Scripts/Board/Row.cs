using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Row : MonoBehaviour
{
    [SerializeField] private Sprite _yellow, _green, _white, _grey;
    [SerializeField] private GameObject _tilePrefab;

    private List<GameObject> _tiles = new();
    private int _currentIndex;
    private bool _isShaking;

    public void InitializeCompletedTiles()
    {
        _tiles = new List<GameObject>();

        foreach (Transform child in transform)
        {
            _tiles.Add(child.gameObject);
        }
    }

    public void InitializeFailedTiles()
    {
        _tiles = new List<GameObject>();

        for (var i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);

            _tiles.Add(child.gameObject);
            child.GetComponent<CanvasGroup>().alpha = 1f;

            if (BoosterManager.Instance.MatchedPositions[i])
            {
                child.GetComponent<Image>().sprite = _green;
                child.GetComponentInChildren<TextMeshProUGUI>().text = Board.Instance.CurrentWord[i].ToString().ToUpper();
            }
            else
            {
                child.GetComponent<Image>().sprite = _white;
                child.GetComponentInChildren<TextMeshProUGUI>().text = "";
            }
        }
    }

    public void SpawnTiles(int numberOfTiles)
    {
        for (var i = 0; i < numberOfTiles; i++)
        {
            var tile = Instantiate(_tilePrefab, transform);
            _tiles.Add(tile);
        }
    }

    public void SetCurrentRow(bool state)
    {
        foreach (var tile in _tiles)
        {
            tile.GetComponent<CanvasGroup>().DOFade(state ? 1f : 0.3f, 0.25f);
        }
    }

    public void SetCurrentTile(bool state)
    {
        _tiles[0].transform.GetChild(0).gameObject.SetActive(state);
    }

    public int CurrentIndex()
    {
        return _currentIndex;
    }

    public List<GameObject> GetTiles()
    {
        return _tiles;
    }

    public void Reset()
    {
        _currentIndex = 0;
        _tiles[_currentIndex].transform.GetChild(0).gameObject.SetActive(true);

        foreach (var c in _tiles)
        {
            c.GetComponentInChildren<TextMeshProUGUI>().text = "";
            c.GetComponent<Image>().sprite = _white;
        }
    }

    public void EnterNextChar(char letter)
    {
        if (_currentIndex >= _tiles.Count) return;

        SetTileState(BoosterManager.Instance.LettersRevealed[_currentIndex] && letter == Board.Instance.CurrentWord[_currentIndex] ? "correct" : "default");

        _tiles[_currentIndex].GetComponentInChildren<TextMeshProUGUI>().text = letter.ToString();
        _tiles[_currentIndex].transform.GetChild(0).gameObject.SetActive(false);
        _currentIndex++;

        if (_currentIndex < _tiles.Count)
        {
            _tiles[_currentIndex].transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    public void DeleteChar()
    {
        if (_currentIndex <= 0) return;

        if (_currentIndex < _tiles.Count)
        {
            _tiles[_currentIndex].transform.GetChild(0).gameObject.SetActive(false);
        }

        _currentIndex--;
        _tiles[_currentIndex].transform.GetChild(0).gameObject.SetActive(true);
        _tiles[_currentIndex].GetComponentInChildren<TextMeshProUGUI>().text = "";

        SetTileState(BoosterManager.Instance.LettersRevealed[_currentIndex] ? "reveal" : "default");
    }

    public string GetCurrentWord()
    {
        return string.Concat(_tiles
            .Where(t => t.GetComponent<CanvasGroup>().alpha == 1)
            .Select(t => t.GetComponentInChildren<TextMeshProUGUI>().text));
    }

    public void Validate()
    {
        var guessedWord = GetCurrentWord();
        var correctWord = Board.Instance.CurrentWord;

        var correctLetterCounts = new Dictionary<char, int>();

        // Increase char count or add if not existed.
        foreach (var letter in correctWord.Where(letter => !correctLetterCounts.TryAdd(letter, 1)))
        {
            correctLetterCounts[letter]++;
        }

        var tileColors = new Sprite[_tiles.Count];
        var isCorrectPosition = new bool[_tiles.Count];

        //Check for correct letter first.
        for (var i = 0; i < guessedWord.Length; i++)
        {
            if (guessedWord[i] != correctWord[i]) continue;

            tileColors[i] = _green;
            correctLetterCounts[guessedWord[i]]--;
            isCorrectPosition[i] = true;

            BoosterManager.Instance.MatchedPositions[i] = true;
            BoosterManager.Instance.LettersRevealed[i] = false;
        }

        //Check for correct letter but incorrect position.
        for (var i = 0; i < guessedWord.Length; i++)
        {
            if (isCorrectPosition[i]) continue;

            if (correctLetterCounts.ContainsKey(guessedWord[i]) && correctLetterCounts[guessedWord[i]] > 0)
            {
                tileColors[i] = _yellow;
                correctLetterCounts[guessedWord[i]]--;
            }
            else
            {
                tileColors[i] = _grey;
            }
        }

        // Animation
        var finalSeq = DOTween.Sequence();
        var delayBetweenTiles = 0.1f;

        for (var i = 0; i < _tiles.Count; i++)
        {
            var tileTransform = _tiles[i].transform;
            tileTransform.DOKill();

            var index = i;
            var seq = DOTween.Sequence();
            seq.Append(tileTransform.DOScale(Vector3.zero, 0.15f).SetDelay(index * delayBetweenTiles));
            seq.AppendCallback(() =>
            {
                _tiles[index].GetComponent<Image>().sprite = tileColors[index];
                _tiles[index].transform.GetChild(2).gameObject.SetActive(tileColors[index] == _green);
            });

            seq.Append(tileTransform.DOScale(Vector3.one, 0.15f));
            finalSeq.Join(seq);
        }

        finalSeq.OnComplete(() =>
        {
            Board.Instance.CheckWord();
            BoardSave.Instance.SaveBoard();
            BoosterManager.Instance.UpdateRevealButtonState();
        });
    }

    public void SetTileState(string state)
    {
        switch (state)
        {
            case "reveal":
                _tiles[_currentIndex].GetComponent<Image>().sprite = _green;
                _tiles[_currentIndex].GetComponent<CanvasGroup>().alpha = 0.75f;
                _tiles[_currentIndex].GetComponentInChildren<TextMeshProUGUI>().text = Board.Instance.CurrentWord[_currentIndex].ToString();
                break;
            case "correct":
                _tiles[_currentIndex].GetComponent<Image>().sprite = _green;
                _tiles[_currentIndex].GetComponent<CanvasGroup>().alpha = 1f;
                break;
            default:
                _tiles[_currentIndex].GetComponent<Image>().sprite = _white;
                _tiles[_currentIndex].GetComponent<CanvasGroup>().alpha = 1f;
                break;
        }
    }

    public void RevealLetter(int index, char letter)
    {
        var tile = _tiles[index];
        var image = tile.GetComponent<Image>();
        var canvasGroup = tile.GetComponent<CanvasGroup>();
        var text = tile.GetComponentInChildren<TextMeshProUGUI>();

        var seq = DOTween.Sequence();
        seq.Append(tile.transform.DOScale(0f, 0.25f).SetEase(Ease.InBack))
           .AppendCallback(() =>
           {
               image.sprite = _green;
               canvasGroup.alpha = string.IsNullOrEmpty(text.text) ? 0.75f : 1f;
               text.text = letter.ToString();
           })
           .Append(tile.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack))
           .OnComplete(() =>
           {
               Board.Instance.UpdateSubmitButtonState();
           });
    }

    public void AnimateInvalidWord()
    {
        if (_isShaking) return;
        _isShaking = true;

        transform.DOKill();
        transform.DOShakePosition(0.5f, new Vector3(10f, 0f, 0f), 12, 0, false, true)
        .OnComplete(() =>
        {
            _isShaking = false;
        });
    }
}
