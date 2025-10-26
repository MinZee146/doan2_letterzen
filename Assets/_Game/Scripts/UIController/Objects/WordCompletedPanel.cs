using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordCompletedPanel : Singleton<WordCompletedPanel>
{
    [SerializeField] private GameObject _wordPanel, _background, _expBar, _progressBar, _chest, _confetti;
    [SerializeField] private Image _compliment;
    [SerializeField] private List<Sprite> _complimentList;
    [SerializeField] private List<GameObject> _progressMilestones;

    private GameObject _currentWord;

    private void OnEnable()
    {
        UIManager.Instance.ShowCoinBar();

        var word = Board.Instance.GetCurrentRow();
        var backgroundRect = _background.GetComponent<RectTransform>();
        var wordRect = word.GetComponent<RectTransform>();

        var targetHeight = wordRect.sizeDelta.y + 75f;
        var targetWidth = wordRect.sizeDelta.x + 100f;

        backgroundRect.sizeDelta = new(backgroundRect.sizeDelta.x, targetHeight);
        backgroundRect.DOSizeDelta(new(targetWidth, targetHeight), 0.5f).SetEase(Ease.OutBack).OnComplete(() => AnimateSpawnRow(word));
    }

    private void AnimateSpawnRow(GameObject word)
    {
        var rowInstance = Instantiate(word, _wordPanel.transform.position, Quaternion.identity, _wordPanel.transform);
        var row = rowInstance.GetComponent<Row>();

        _currentWord = rowInstance;
        row.InitializeCompletedTiles();

        var finalSeq = DOTween.Sequence();
        for (var i = 0; i < row.GetTiles().Count; i++)
        {
            var tile = row.GetTiles()[i];
            var rect = tile.GetComponent<RectTransform>();
            var canvasGroup = tile.GetComponent<CanvasGroup>();

            rect.localScale = Vector3.zero;
            canvasGroup.alpha = 0;

            var seq = DOTween.Sequence();
            seq.AppendInterval(i * 0.05f);
            seq.Append(rect.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
            seq.Join(canvasGroup.DOFade(1f, 0.5f).SetEase(Ease.OutQuad));

            finalSeq.Join(seq);
        }

        finalSeq.OnComplete(() =>
        {
            AnimateCompliment();
        });
    }

    private void AnimateCompliment()
    {
        AudioManager.Instance.PlaySFX("Common_Success");

        _confetti.SetActive(true);
        _compliment.sprite = _complimentList[Board.Instance.CurrentRowIndex];
        _compliment.transform.localScale = Vector3.zero;
        _compliment.color = new Color(_compliment.color.r, _compliment.color.g, _compliment.color.b, 0);

        var seq = DOTween.Sequence();
        seq.Append(_compliment.transform.DOScale(1.1f, 0.5f).SetEase(Ease.OutBack));
        seq.Join(_compliment.DOFade(1f, 0.5f).SetEase(Ease.OutQuad));
    }

    public void AnimateIncreaseExp()
    {
        _compliment.transform.DOScale(1f, 1f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

        var (startValue, endValue, isMilestoneReached) = ExpManager.Instance.CalculateIncreaseExp();
        var rank = _expBar.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
        var expSlider = _expBar.transform.GetChild(1).GetComponentInChildren<Slider>();

        rank.text = ExpManager.Instance.GetRank();
        expSlider.value = startValue;
        ExpManager.Instance.IncreaseExp();

        var duration = isMilestoneReached ? (1f - startValue) : (endValue - startValue);
        var seq = DOTween.Sequence();

        seq.AppendInterval(0.25f);
        seq.Append(_expBar.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
        seq.AppendInterval(0.25f);
        
        if (isMilestoneReached)
        {
            seq.Append(expSlider.DOValue(1f, duration * 3f).SetEase(Ease.OutQuad))
               .AppendCallback(() =>
               {
                   expSlider.value = endValue < 1f ? 0f : 1f;
                   ExpManager.Instance.CheckForMilestoneRewards();
               })

               .Append(rank.transform.DOScale(Vector3.one * 1.5f, 0.25f).SetEase(Ease.InQuad))
               .Join(rank.DOFade(0f, 0.25f).SetEase(Ease.InQuad))
               .AppendCallback(() => rank.text = ExpManager.Instance.GetRank())
               .Append(rank.DOFade(1f, 0.25f).SetEase(Ease.OutQuad))
               .Join(rank.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutQuad))
               .Append(expSlider.DOValue(endValue, endValue != 1 ? endValue * 3f : 0).SetEase(Ease.OutQuad));
        }
        else
        {
            seq.Append(expSlider.DOValue(endValue, duration * 3f).SetEase(Ease.OutQuad));
        }

        seq.OnComplete(() => AnimateProgressBar());
    }

    private void AnimateProgressBar()
    {
        var completedWord = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_COMPLETED_WORD_COUNT_LOOP, 0);
        var progress = _progressBar.transform.GetChild(0).GetComponent<Image>();
        progress.fillAmount = completedWord / 3f;

        for (int i = 0; i < _progressMilestones.Count; i++)
        {
            _progressMilestones[i].SetActive(i < completedWord);
            _progressMilestones[i].transform.localScale = i < completedWord ? Vector3.one : Vector3.zero;
        }

        _progressMilestones[completedWord].SetActive(true);

        var seq = DOTween.Sequence();
        seq.AppendInterval(0.25f);
        seq.Append(_progressBar.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));

        seq.AppendInterval(0.25f);
        seq.Append(_progressMilestones[completedWord].transform.DOScale(1.2f, 0.25f).SetEase(Ease.OutBack).OnComplete(() => AudioManager.Instance.PlaySFX("Game_Star")));
        seq.Append(_progressMilestones[completedWord].transform.DOScale(1f, 0.25f).SetEase(Ease.InOutSine));

        seq.AppendInterval(0.1f);

        seq.Append(progress.DOFillAmount((completedWord + 1) / 3f, 1.5f).SetEase(Ease.OutQuad).OnPlay(() => AudioManager.Instance.PlaySideAudio("Game_Increase")));
        seq.OnComplete(() =>
        {
            completedWord = (completedWord + 1) % 3;
            PlayerPrefs.SetInt(Constants.PLAYER_PREFS_COMPLETED_WORD_COUNT_LOOP, completedWord);
            PlayerPrefs.Save();

            if (completedWord == 0)
            {
                AnimateReceiveChest();
                return;
            }

            Reset();
        });
    }

    private void AnimateReceiveChest()
    {
        AudioManager.Instance.PlaySFX("Common_ChestOpen");

        var seq = DOTween.Sequence();
        var top = _chest.transform.GetChild(2);
        var coin = _chest.transform.GetChild(3);
        coin.GetComponentInChildren<TextMeshProUGUI>().text = RemoteConfigs.Instance.GameConfigs.CoinsCompletedWords.ToString();

        seq.AppendInterval(0.25f);
        seq.Append(top.DOLocalMoveY(top.localPosition.y + 40f, 0.5f).SetEase(Ease.OutBack));
        seq.Join(_chest.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0f), 0.5f, 3, 0.5f));

        seq.AppendInterval(0.25f);
        seq.Append(coin.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
        seq.Join(coin.DOLocalJump(new Vector3(0f, -250f, 0f), 200f, 1, 0.5f).SetEase(Ease.OutCubic));

        seq.AppendInterval(0.25f);
        seq.AppendCallback(() => RewardAttractor.Instance.RewardAttract(RewardType.Coin, coin,
                                 GameObject.FindGameObjectWithTag("Coin").transform,
                                 () => CoinBar.Instance.IncreaseCoin(RemoteConfigs.Instance.GameConfigs.CoinsCompletedWords)));
        seq.OnComplete(() =>
        {
            Reset();
        });
    }

    private void Reset()
    {
        void Reset()
        {
            _expBar.transform.localScale = Vector3.zero;
            _progressBar.transform.localScale = Vector3.zero;
            _background.GetComponent<RectTransform>().sizeDelta = Vector3.zero;

            _compliment.transform.localScale = Vector3.zero;
            _compliment.color = new(_compliment.color.r, _compliment.color.g, _compliment.color.b, 0);
            _compliment.transform.DOKill();

            _chest.transform.GetChild(2).transform.localPosition = new(0f, 30f, 0f);
            _chest.transform.GetChild(3).transform.localPosition = Vector3.zero;
            _chest.transform.GetChild(3).transform.localScale = Vector3.zero;

            Destroy(_currentWord);
            UIManager.Instance.HideAllPopUps();
            UIManager.Instance.HideCoinBar();
            Board.Instance.ProceedToNextWord();
        }

        var seq = DOTween.Sequence();
        seq.AppendInterval(1.5f);
        seq.Append(gameObject.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).SetEase(Ease.InQuad));
        seq.OnComplete(() =>
        {
            AdController.Instance.ShowInterstitialAd((success, code) => Reset(), () => Reset());
        });
    }
}