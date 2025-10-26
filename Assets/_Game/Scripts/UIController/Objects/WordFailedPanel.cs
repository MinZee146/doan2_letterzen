using DG.Tweening;
using Genix.MocaLib.Runtime.Services;
using TMPro;
using UnityEngine;

public class WordFailedPanel : Singleton<WordFailedPanel>
{
    [SerializeField] private GameObject _wordPanel, _background, _title, _retry;
    [SerializeField] private GameObject _freeRetry, _paidRetry;
    [SerializeField] private TextMeshProUGUI _retryPrice;

    private GameObject _currentWord;

    public void ShowWordFailed(Canvas canvas)
    {
        Reset();

        var rect = gameObject.GetComponent<RectTransform>();
        var canvasGroup = gameObject.GetComponent<CanvasGroup>();

        rect.anchoredPosition = new Vector2(0, -rect.sizeDelta.y);
        canvasGroup.alpha = 0;

        var isAdEnabled = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_IS_ADS_ENABLED, 0) == 1 &&
                          PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_LEVEL, 1) >=
                          RemoteConfigs.Instance.GameConfigs.BannerStartFromLevel;

        var seq = DOTween.Sequence();
        seq.Append(rect.DOAnchorPosY(isAdEnabled ?
            MocaLib.Instance.AdManager.GetBannerHeight(canvas) : 0, 0.5f).SetEase(Ease.OutCubic))
           .Join(canvasGroup.DOFade(0.975f, 0.5f).SetEase(Ease.OutCubic))
           .OnComplete(() => DisplayCurrentWord());
    }

    public void DisplayCurrentWord()
    {
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
        row.InitializeFailedTiles();

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
            _freeRetry.SetActive(Board.Instance.TimesGuessed == 1);
            _paidRetry.SetActive(Board.Instance.TimesGuessed > 1);
            _retryPrice.text = RemoteConfigs.Instance.GameConfigs.RetryPrice.ToString();

            var seq = DOTween.Sequence();
            seq.Append(_title.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack))
               .Join(_retry.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
        });
    }

    public void Reset()
    {
        _title.transform.localScale = Vector3.zero;
        _retry.transform.localScale = Vector3.zero;
        _background.GetComponent<RectTransform>().sizeDelta = Vector3.zero;
        Destroy(_currentWord);
    }

    public void Retry()
    {
        void RetryLogic()
        {
            var rect = GetComponent<RectTransform>();
            var canvasGroup = GetComponent<CanvasGroup>();

            var seq = DOTween.Sequence();
            seq.Append(rect.DOAnchorPosY(-rect.sizeDelta.y, 0.5f).SetEase(Ease.InCubic))
               .Join(canvasGroup.DOFade(0f, 0.5f).SetEase(Ease.InCubic))
               .OnComplete(() =>
               {
                   BoardSave.Instance.DeleteBoardSave();
                   BoosterManager.Instance.SetLettersRevealed();
                   Board.Instance.ResetBoard(true);
                   Board.Instance.SpawnRow(Board.Instance.CurrentWord.Length);
                   BoardSave.Instance.SaveBoard();
                   UIManager.Instance.HideAllPopUps();
               });
        }

        if (Board.Instance.TimesGuessed > 1)
        {
            CoinBar.Instance.DecreaseCoin(RemoteConfigs.Instance.GameConfigs.RetryPrice, onSuccessful: RetryLogic);
        }
        else
        {
            AdController.Instance.ShowInterstitialAd((success, code) => RetryLogic(), () => RetryLogic());
        }
    }
}