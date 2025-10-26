using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Definition : Singleton<Definition>
{
    [SerializeField] private TextMeshProUGUI _defText, _priceText;
    [SerializeField] private RectTransform _defRectTransform;
    [SerializeField] private GameObject _unlockDefButton;

    private float _originalWidth;

    private void Start()
    {
        SetDefinitionPrice();
        _originalWidth = _defRectTransform.sizeDelta.x;
        _unlockDefButton.GetComponent<Button>().onClick.AddListener(BoardSave.Instance.SaveBoard);
    }

    public void SetDefinitionPrice()
    {
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.LevelMode &&
            !TutorialController.Instance.IsTutorialCompleted)
        {
            _priceText.text = "Free";
        }
        else
        {
            _priceText.text = RemoteConfigs.Instance.GameConfigs.DefinitionPrice.ToString();
        }
    }

    public bool IsUnlocked()
    {
        return _unlockDefButton.activeSelf == false;
    }

    public void UnlockDefinition(bool wasSaved = false)
    {
        if (string.IsNullOrEmpty(Board.Instance.CurrentWord)) return;

        var isFree = wasSaved || (GameManager.Instance.CurrentGameMode == GameManager.GameMode.LevelMode && !TutorialController.Instance.IsTutorialCompleted);
        CoinBar.Instance.DecreaseCoin(isFree ? 0 : RemoteConfigs.Instance.GameConfigs.DefinitionPrice, onSuccessful: () =>
        {
            AudioManager.Instance.PlaySFX("Game_Definition");

            _unlockDefButton.SetActive(false);

            var def = GameManager.Instance.CurrentGameMode == GameManager.GameMode.LevelMode ?
                    LevelWordList.Instance.GetDefinition(Board.Instance.CurrentWord) :
                    ThemedWordList.Instance.GetDefinition();
            _defText.text = def;

            var targetWidth = Mathf.Min(_defText.preferredWidth + 40, 900);
            var duration = Mathf.Clamp(targetWidth * 0.0003f, 0.1f, 0.3f);
            _defText.text = "";

            _defRectTransform.DOSizeDelta(new Vector2(targetWidth, _defRectTransform.sizeDelta.y), duration).SetEase(Ease.OutCubic);
            DOVirtual.DelayedCall(duration / 1.75f, () =>
            {
                _defText.text = def;
            });
        });
    }

    public void ClearDefinition()
    {
        var duration = Mathf.Clamp(_defText.preferredWidth * 0.0002f, 0.1f, 0.3f);

        _defText.text = "";
        _defRectTransform.DOSizeDelta(new Vector2(_originalWidth, _defRectTransform.sizeDelta.y), duration)
                         .SetEase(Ease.InCubic)
                         .OnComplete(() =>
                         {
                             _unlockDefButton.SetActive(true);
                             _unlockDefButton.GetComponent<Button>().interactable = true;
                         });
    }

    public void DisableDefinition()
    {
        _unlockDefButton.GetComponent<Button>().interactable = false;
    }
}
