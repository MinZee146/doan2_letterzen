using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Key : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private RectTransform _rect;

    public void Start()
    {
        _button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        AudioManager.Instance.PlaySFX("Keyboard_Click");

        _rect.DOKill();
        _rect.localScale = Vector3.one;
        _rect.DOScale(0.75f, 0.1f).OnComplete(() =>
        {
            _rect.DOScale(1f, 0.15f).SetEase(Ease.OutBack);
        });

        if (gameObject.name.Length == 1)
        {
            Board.Instance.EnterNextChar(gameObject.name[0]);
        }
        else switch (gameObject.name)
            {
                case "Backspace":
                    Board.Instance.DeleteChar();
                    Keyboard.Instance.ToggleSubmitButtonState("Disable");
                    break;
                case "Submit":
                    Board.Instance.SubmitWord();
                    Keyboard.Instance.ToggleSubmitButtonState("Disable");
                    break;
            }
    }

    public Tween Validate(Color color)
    {
        return GetComponent<Image>().DOColor(color, 0.25f);
    }

    public void Reset()
    {
        GetComponent<Image>().DOColor(Color.white, 0.25f);
    }

    public void SetInteractable(bool isInteractable)
    {
        GetComponent<Button>().interactable = isInteractable;
    }
}
