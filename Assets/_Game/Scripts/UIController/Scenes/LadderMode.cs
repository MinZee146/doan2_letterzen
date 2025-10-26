using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Genix.MocaLib.Runtime.Services;
using MEC;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LadderMode : Singleton<LadderMode>
{
    [SerializeField] private RectTransform _framesRect, _tilesRect, _bottomRect;
    [SerializeField] private GameObject _framesContainer, _framePrefab;
    [SerializeField] private GameObject _tilesContainer, _tilePrefab;
    [SerializeField] private GameObject _wordSlide, _confettiPrefab;
    [SerializeField] private Button _scrambleBtn;
    [SerializeField] private Sprite _green, _yellow, _white, _grey;
    [SerializeField] private List<TextMeshProUGUI> _words;

    private List<GameObject> _tiles = new();
    private Dictionary<GameObject, bool> _tileStates = new();
    private Dictionary<GameObject, bool> _frameStates = new();
    private Dictionary<GameObject, GameObject> _frameToTile = new();
    private Dictionary<GameObject, Vector3> _tileOriginalPositions = new();

    private bool _isAnimating = true, _isAllTileRevealed, _isMatchWord;
    private LadderState _loadedState;
    private Dictionary<string, Sprite> _spriteDict;

    public bool IsAllTileRevealed => _isAllTileRevealed;
    public bool IsMatchWord => _isMatchWord;

    #region Initialize
    private void Start()
    {
        UpdatePositionBasedOnAds();
        ProgressBar.Instance.StartAuraAnimation();
    }

    public void Initialize()
    {
        LoadLadderState();
        SpawnFrames(LadderWordList.Instance.GetWord().Length);
        SpawnTiles(LadderWordList.Instance.GetSpawnWord());

        BoosterManager.Instance.UpdateRevealButtonState();
        BoosterManager.Instance.UpdateClearButtonState();
    }

    public void SpawnFrames(int frameCount)
    {
        _isAnimating = true;

        var finalSeq = DOTween.Sequence();
        for (var i = 0; i < frameCount; i++)
        {
            var obj = Instantiate(_framePrefab, _framesContainer.transform);
            _frameToTile.Add(obj, null);
            _frameStates.Add(obj, _loadedState.FrameStates != null && _loadedState.FrameStates[i]);

            var rect = obj.GetComponent<RectTransform>();
            var canvasGroup = obj.GetComponent<CanvasGroup>();

            if (_loadedState.FrameStates != null && _loadedState.FrameStates[i])
            {
                obj.GetComponent<Image>().sprite = _green;
                obj.GetComponent<Image>().color = Color.white;
                obj.GetComponentInChildren<TextMeshProUGUI>().text = LadderWordList.Instance.GetWord()[i].ToString().ToUpper();
            }

            var seq = DOTween.Sequence();
            seq.AppendInterval(i * 0.05f);
            seq.Append(rect.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
            seq.Join(canvasGroup.DOFade(_loadedState.FrameStates != null ? 0.5f : 1f, 0.5f).SetEase(Ease.OutQuad));
            finalSeq.Join(seq);
        }

        finalSeq.OnComplete(() => _isAnimating = false);
    }

    public void SpawnTiles(string word)
    {
        _tilesContainer.GetComponent<GridLayoutGroup>().enabled = true;

        var scrambled = word.ToCharArray();
        do
        {
            scrambled = word.OrderBy(_ => Random.value).ToArray();
        } while (new string(scrambled) == word);

        var finalSeq = DOTween.Sequence();
        for (var i = 0; i < scrambled.Length; i++)
        {
            var letter = scrambled[i];
            var obj = Instantiate(_tilePrefab, _tilesContainer.transform);
            obj.GetComponent<Button>().onClick.AddListener(() => ToggleTilePosition(obj));

            if (_loadedState.TileStates != null)
            {
                obj.GetComponentInChildren<TextMeshProUGUI>().text = _loadedState.TileStates[i].letter;
                obj.GetComponent<Image>().sprite = _spriteDict[_loadedState.TileStates[i].sprite];
            }
            else
            {
                obj.GetComponentInChildren<TextMeshProUGUI>().text = letter.ToString().ToUpper();
                obj.GetComponent<Image>().sprite = _white;
            }

            _tiles.Add(obj);
            _tileStates.Add(obj, false);

            var rect = obj.GetComponent<RectTransform>();
            var canvasGroup = obj.GetComponent<CanvasGroup>();

            var seq = DOTween.Sequence();
            seq.AppendInterval(i * 0.05f);
            seq.Append(rect.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
            seq.Join(canvasGroup.DOFade(1f, 0.5f).SetEase(Ease.OutQuad));
            finalSeq.Join(seq);
        }

        finalSeq.OnComplete(() => Timing.RunCoroutine(StoreTilePositions()));
    }

    private IEnumerator<float> StoreTilePositions()
    {
        yield return Timing.WaitForOneFrame;
        _isAnimating = false;
        _tilesContainer.GetComponent<GridLayoutGroup>().enabled = false;

        foreach (var tile in _tiles)
        {
            _tileOriginalPositions[tile] = tile.transform.position;
        }

        LadderSave.Instance.SaveLadder();
    }

    public void UpdatePositionBasedOnAds()
    {
        var isAdEnabled = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_IS_ADS_ENABLED, 0) == 1 &&
                          PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_LEVEL, 1) >=
                          RemoteConfigs.Instance.GameConfigs.BannerStartFromLevel;

        if (isAdEnabled)
        {
            var bannerHeight = MocaLib.Instance.AdManager.GetBannerHeight(GetComponent<Canvas>());

            _framesRect.offsetMin += new Vector2(0f, bannerHeight);
            _tilesRect.offsetMin += new Vector2(0f, bannerHeight);
            _bottomRect.sizeDelta += new Vector2(0f, bannerHeight);
        }
        else
        {
            _framesRect.offsetMin = new Vector2(0f, 600);
            _tilesRect.offsetMin = new Vector2(0f, 200);
            _bottomRect.sizeDelta = new Vector2(0f, 800);
        }
    }
    #endregion

    #region Logic
    public void BackToHome()
    {
        DOTween.KillAll();
        SceneTransitionManager.Instance.LoadScene(Constants.HOME_SCENE_KEY, () => MainMenu.Instance.Initialize());
    }

    public void ProceedToNextWord()
    {
        foreach (var tile in _tiles)
        {
            Destroy(tile);
        }

        foreach (var frame in _frameToTile.Keys)
        {
            Destroy(frame);
        }

        _tiles.Clear();
        _tileStates.Clear();
        _tileOriginalPositions.Clear();
        _frameStates.Clear();
        _frameToTile.Clear();

        Initialize();
    }

    private void CheckWord()
    {
        var currentWord = string.Join("", _frameToTile.Values
       .Select(tile => tile.GetComponentInChildren<TextMeshProUGUI>().text))
       .ToLower();

        if (!Dictionary.Instance.CheckWord(currentWord))
        {
            Timing.RunCoroutine(RemoveAllTiles());
            return;
        }

        if (currentWord == LadderWordList.Instance.GetWord())
        {
            Timing.RunCoroutine(CorrectWord());
        }
        else
        {
            IncorrectWord();
            Timing.RunCoroutine(RemoveAllTiles());
        }
    }

    private void NextWord()
    {
        _isAllTileRevealed = false;
        _isMatchWord = false;
        _scrambleBtn.interactable = true;

        UpdateWordSlide(LadderWordList.Instance.GetWord());
        LadderWordList.Instance.ProceedToNextWord();

        if (LadderWordList.Instance.EndAllGroups())
        {
            UIManager.Instance.ShowPopUp("Compliment", true, PopUpShowBehaviour.HIDE_PREVIOUS);
            WordCompletedPopup.Instance.SetPopupState(RemoteConfigs.Instance.GameConfigs.CoinsCompletedLadderGroup + 100, () =>
            {
                BackToHome();
            });
        }
        else
        {
            ProceedToNextWord();
            ProgressBar.Instance.UpdateProgressBar();
        }
    }

    public void CompleteLadderGroup()
    {
        var currentGroup = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_LADDER_GROUP, 1);

        if (currentGroup % 4 == 0)
        {
            UIManager.Instance.ShowPopUp("Compliment", true, PopUpShowBehaviour.HIDE_PREVIOUS);
            WordCompletedPopup.Instance.SetPopupState(RemoteConfigs.Instance.GameConfigs.CoinsCompletedLadderGroup + 100, () =>
            {
                UIManager.Instance.HideLastPopUp();
                AdController.Instance.ShowInterstitialAd((success, code) => NextWord(), () => NextWord());
            });
        }
        else
        {
            RewardAttractor.Instance.RewardAttract(
                RewardType.Coin,
                ProgressBar.Instance.CurrentCheckPointTransform(),
                GameObject.FindGameObjectWithTag("Coin").transform,
                () => CoinBar.Instance.IncreaseCoin(RemoteConfigs.Instance.GameConfigs.CoinsCompletedLadderGroup));

            AdController.Instance.ShowInterstitialAd((success, code) => NextWord(), () => NextWord());
        }
    }
    #endregion

    #region TilesManagement
    private void FillTileIntoSlot(GameObject tile)
    {
        var targetFrame = _frameToTile.FirstOrDefault(frame => frame.Value == null).Key;
        if (targetFrame == null) return;

        var seq = DOTween.Sequence();
        seq.Append(tile.transform.DOScale(1.1f, 0.1f).SetEase(Ease.OutQuad))
           .Join(tile.transform.DOMove(targetFrame.transform.position, 0.25f).SetEase(Ease.OutQuad))
           .Append(tile.transform.DOScale(1f, 0.1f).SetEase(Ease.InQuad));

        _tileStates[tile] = true;
        _frameToTile[targetFrame] = tile;

        if (_frameToTile.Count(frame => frame.Value != null) == LadderWordList.Instance.GetWord().Length)
        {
            DOVirtual.DelayedCall(0.3f, CheckWord);
        }
    }

    private void RemoveTileFromSlot(GameObject tile)
    {
        var targetFrame = _frameToTile.FirstOrDefault(frame => frame.Value == tile).Key;
        if (targetFrame == null) return;

        if (_tileOriginalPositions.TryGetValue(tile, out var position))
        {
            var seq = DOTween.Sequence();
            seq.Append(tile.transform.DOScale(1.1f, 0.1f).SetEase(Ease.OutQuad))
               .Join(tile.transform.DOMove(position, 0.25f).SetEase(Ease.OutQuad))
               .Append(tile.transform.DOScale(1f, 0.1f).SetEase(Ease.InQuad));
        }

        _tileStates[tile] = false;
        _frameToTile[targetFrame] = null;
    }

    private void ToggleTilePosition(GameObject tile)
    {
        if (_tileStates[tile])
        {
            RemoveTileFromSlot(tile);
        }
        else
        {
            FillTileIntoSlot(tile);
        }
    }

    private IEnumerator<float> RemoveAllTiles()
    {
        _tilesContainer.transform.DOKill();
        _tilesContainer.transform.DOShakePosition(0.5f, new Vector3(10f, 0f, 0f), 12, 0, false, true);

        yield return Timing.WaitForSeconds(0.3f);

        foreach (var tile in _tiles)
        {
            RemoveTileFromSlot(tile);
        }
    }
    #endregion

    #region Validate
    private void TileValidate(GameObject tile, Sprite sprite)
    {
        var baseImage = tile.GetComponent<Image>();
        var overlayImage = tile.transform.GetChild(0).GetComponent<Image>();
        var star = tile.transform.GetChild(2);

        overlayImage.sprite = baseImage.sprite;
        overlayImage.color = Color.white;
        baseImage.sprite = sprite;

        star.gameObject.SetActive(sprite == _green);
        overlayImage.DOFade(0f, 0.3f).SetEase(Ease.Linear).OnComplete(() => LadderSave.Instance.SaveLadder());
    }

    private void FrameValidate(GameObject frame, char letter)
    {
        var baseImage = frame.GetComponent<Image>();
        var canvasGroup = frame.GetComponent<CanvasGroup>();
        var label = frame.GetComponentInChildren<TextMeshProUGUI>();
        var overlayImage = frame.transform.GetChild(0).GetComponent<Image>();

        overlayImage.sprite = baseImage.sprite;
        overlayImage.color = new Color(1f, 1f, 1f, 0.9f);
        baseImage.sprite = _green;
        baseImage.color = Color.white;
        canvasGroup.alpha = 0.5f;
        label.text = letter.ToString().ToUpper();

        frame.transform.localScale = Vector3.one * 0.5f;
        frame.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        overlayImage.DOFade(0f, 0.3f).SetEase(Ease.Linear).OnComplete(() => LadderSave.Instance.SaveLadder());
    }

    private void IncorrectWord()
    {
        var currentWord = LadderWordList.Instance.GetWord();

        foreach (var pair in _frameToTile)
        {
            var tile = pair.Value;
            var frame = pair.Key;

            var letter = tile.GetComponentInChildren<TextMeshProUGUI>().text.ToLower();
            var tileIndex = frame.transform.GetSiblingIndex();

            if (currentWord.Contains(letter))
            {
                if (tile.GetComponent<Image>().sprite != _green)
                {
                    if (currentWord[tileIndex].ToString() != letter)
                    {
                        TileValidate(tile, _yellow);
                    }
                    else
                    {
                        _frameStates[frame] = true;
                        FrameValidate(frame, letter[0]);
                        TileValidate(tile, _green);
                    }
                }
            }
        }
    }

    private IEnumerator<float> CorrectWord()
    {
        _isAnimating = true;
        AudioManager.Instance.PlaySFX("Common_Success");

        foreach (var tile in _frameToTile.Values)
        {
            if (tile.GetComponent<Image>().sprite == _green) continue;

            TileValidate(tile, _green);
            yield return Timing.WaitForSeconds(0.3f);
        }

        foreach (var pair in _frameToTile)
        {
            var tile = pair.Value;
            var frame = pair.Key;

            var canvasGroup = tile.GetComponent<CanvasGroup>();
            var rect = tile.GetComponent<RectTransform>();

            DOTween.Sequence()
                .Append(canvasGroup.DOFade(0f, 0.3f).SetEase(Ease.InQuad))
                .Join(rect.DOScale(0f, 0.3f).SetEase(Ease.InBack));

            frame.GetComponent<CanvasGroup>().alpha = 0f;
        }

        if (LadderWordList.Instance.IsLastWordInGroup())
        {
            ExpBar.Instance.SpawnExp();
            ExpBar.Instance.AnimateIncreaseExp();
            _confettiPrefab.SetActive(true);
        }
        else
        {
            yield return Timing.WaitForSeconds(0.5f);
            NextWord();
        }
    }
    #endregion

    #region WordSlide
    public void ClearWordSlide()
    {
        foreach (var word in _words)
        {
            word.text = "";
        }
    }

    public void UpdateWordSlide(string firstWord, string secondWord = null, string thirdWord = null)
    {
        AnimateText(_words[2], thirdWord ?? _words[1].text);
        AnimateText(_words[1], secondWord ?? _words[0].text);
        AnimateText(_words[0], firstWord);
    }

    public void AnimateText(TextMeshProUGUI text, string newText)
    {
        text.transform.localScale = Vector3.zero;
        text.text = newText.ToUpper();
        text.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }
    #endregion

    #region Boosters
    private void SortTiles(List<Vector3> originalPositions)
    {
        var completedAnimations = 0;
        var totalTiles = _tiles.Count;

        for (var i = 0; i < totalTiles; i++)
        {
            var tile = _tiles[i].transform;
            tile.SetSiblingIndex(i);

            var seq = DOTween.Sequence();
            seq.Append(tile.DOScale(1.1f, 0.1f).SetEase(Ease.OutQuad))
               .Join(tile.DOMove(originalPositions[i], 0.25f).SetEase(Ease.OutQuad))
               .Append(tile.DOScale(1f, 0.15f).SetEase(Ease.InQuad))
               .OnComplete(() =>
               {
                   completedAnimations++;
                   if (completedAnimations == totalTiles)
                   {
                       Timing.RunCoroutine(StoreTilePositions());
                   }
               });
        }

        foreach (var key in _frameToTile.Keys.ToList())
        {
            _frameToTile[key] = null;
        }

        foreach (var key in _tileStates.Keys.ToList())
        {
            _tileStates[key] = false;
        }
    }

    public void ScrambleTiles()
    {
        if (_isAnimating) return;
        _isAnimating = true;

        var originalPositions = _tiles.Select(tile => _tileOriginalPositions[tile]).ToList();

        do
        {
            _tiles = _tiles.OrderBy(x => Random.value).ToList();
        } while (_tiles.SequenceEqual(_tiles.OrderBy(t => t.transform.GetSiblingIndex()).ToList()));

        SortTiles(originalPositions);
    }

    public void RevealLetter()
    {
        var revealFrame = _frameStates.FirstOrDefault(frame => !frame.Value).Key;
        if (revealFrame == null || _isAnimating) return;

        void RevealAction()
        {
            _frameStates[revealFrame] = true;
            _isAllTileRevealed = _frameStates.All(frame => frame.Value);

            BoosterManager.Instance.UpdateRevealButtonState();
            AudioManager.Instance.PlaySFX("Game_Booster");
            DailyGoalManager.Instance.UpdateProgress(GoalType.UseHint);
            FrameValidate(revealFrame, LadderWordList.Instance.GetWord()[revealFrame.transform.GetSiblingIndex()]);
        }

        if (RewardManager.Instance.Reveals > 0)
        {
            RewardManager.Instance.UseReward(RewardType.Reveal, onSuccessful: RevealAction);
        }
        else
        {
            CoinBar.Instance.DecreaseCoin(RemoteConfigs.Instance.GameConfigs.RevealPrice, onSuccessful: RevealAction);
        }
    }

    public void MatchWord()
    {
        if (_isAnimating) return;

        void ClearAction()
        {
            Timing.RunCoroutine(ClearActionCoroutine());
        }

        IEnumerator<float> ClearActionCoroutine()
        {
            _isAnimating = true;
            _scrambleBtn.interactable = false;

            var targetWord = LadderWordList.Instance.GetWord().ToLower();

            if (targetWord.Length != _tiles.Count)
            {
                _tilesContainer.GetComponent<GridLayoutGroup>().enabled = true;

                _tiles.Where(tile => !targetWord.Contains(tile.GetComponentInChildren<TextMeshProUGUI>().text.ToLower()))
                      .ToList()
                      .ForEach(tile =>
                      {
                          _tileStates.Remove(tile);
                          _tileOriginalPositions.Remove(tile);
                          _tiles.Remove(tile);
                          Destroy(tile);
                      });

                yield return Timing.WaitUntilDone(Timing.RunCoroutine(StoreTilePositions()));
            }

            var originalPositions = _tiles.Select(tile => _tileOriginalPositions[tile]).ToList();
            var sortedTiles = new List<GameObject>();

            foreach (var c in targetWord)
            {
                var matchedTile = _tiles.FirstOrDefault(t =>
                    t.GetComponentInChildren<TextMeshProUGUI>().text.ToLower() == c.ToString() && !sortedTiles.Contains(t)
                );

                sortedTiles.Add(matchedTile);
            }

            _tiles = sortedTiles;
            _isMatchWord = true;

            SortTiles(originalPositions);
            BoosterManager.Instance.UpdateClearButtonState();
            AudioManager.Instance.PlaySFX("Game_Booster");
            DailyGoalManager.Instance.UpdateProgress(GoalType.UseHint);
        }

        if (RewardManager.Instance.Clears > 0)
        {
            RewardManager.Instance.UseReward(RewardType.Clear, onSuccessful: ClearAction);
        }
        else
        {
            CoinBar.Instance.DecreaseCoin(RemoteConfigs.Instance.GameConfigs.ClearPrice, onSuccessful: ClearAction);
        }
    }
    #endregion

    #region SaveProgress
    public (List<bool>, List<TileLadder>, bool, bool) GetLadderState()
    {
        return (
            _frameStates.Values.ToList(),
            _tiles.Select(t =>
            {
                return new TileLadder
                {
                    sprite = t.GetComponent<Image>().sprite.name,
                    letter = t.GetComponentInChildren<TextMeshProUGUI>().text
                };
            }).ToList(),
            _isAllTileRevealed,
            _isMatchWord
        );
    }

    private void LoadLadderState()
    {
        _spriteDict = new Dictionary<string, Sprite>
        {
            { _white.name, _white },
            { _green.name, _green },
            { _yellow.name, _yellow },
            { _grey.name, _grey }
        };

        _loadedState = LadderSave.Instance.LoadLadderFile();
        if (_loadedState.FrameStates == null || _loadedState.TileStates == null) return;

        _isAllTileRevealed = _loadedState.IsAllTileRevealed;
        _isMatchWord = _loadedState.IsMatchWord;
        _scrambleBtn.interactable = !_isMatchWord;
    }
    #endregion
}