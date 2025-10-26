using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : Singleton<ProgressBar>
{
    [SerializeField] private GameObject _aura;
    [SerializeField] private Slider _progressBar;
    [SerializeField] private GameObject[] _checkPoints;

    private Color _defaultColor, _completedColor;
    private GameObject _currentCheckPoint;

    public void SetupProgressBar()
    {
        ColorUtility.TryParseHtmlString("#224A5D", out _defaultColor);
        ColorUtility.TryParseHtmlString("#27C133", out _completedColor);

        var currentGroup = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_LADDER_GROUP);
        var currentIndex = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_LADDER_INDEX);

        var targetValue = CalculateProgressValue(currentGroup, currentIndex);
        _progressBar.DOValue(targetValue, 0.5f).SetEase(Ease.OutQuad);

        var baseIndex = (currentGroup - 1) / 4 * 4 + 1;

        for (var i = 0; i < _checkPoints.Length - 1; i++)
        {
            var pointValue = baseIndex + i;
            var isCurrent = pointValue == currentGroup;
            var isCompleted = pointValue < currentGroup;

            var checkPoint = _checkPoints[i];
            SetupCheckPoint(checkPoint, pointValue, isCurrent, isCompleted);
        }
    }

    private void SetupCheckPoint(GameObject checkPoint, int pointValue, bool isCurrent, bool isCompleted)
    {
        var image = checkPoint.GetComponent<Image>();
        var icon = checkPoint.transform.GetChild(1).gameObject;
        var label = checkPoint.GetComponentInChildren<TextMeshProUGUI>();

        if (isCurrent)
        {
            _currentCheckPoint = checkPoint;
            AnimateCheckPoint(checkPoint, _completedColor, false, pointValue.ToString(), 1.25f);
        }
        else
        {
            checkPoint.transform.localScale = Vector3.one;
            image.color = _defaultColor;
        }

        icon.SetActive(isCompleted);
        label.text = isCompleted ? "" : pointValue.ToString();
    }

    public void UpdateProgressBar()
    {
        var currentGroup = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_LADDER_GROUP);
        var currentIndex = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_LADDER_INDEX);

        var targetValue = CalculateProgressValue(currentGroup, currentIndex);
        _progressBar.DOValue(targetValue, 0.5f).SetEase(Ease.OutQuad);

        if (currentIndex % 6 == 1)
        {
            UpdateCheckPoints();
        }
    }

    private void UpdateCheckPoints()
    {
        var currentGroup = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_CURRENT_LADDER_GROUP);
        var baseIndex = (currentGroup - 1) / 4 * 4 + 1;

        for (var i = 0; i < _checkPoints.Length - 1; i++)
        {
            var pointValue = baseIndex + i;
            var checkPoint = _checkPoints[i];

            if (baseIndex != currentGroup && pointValue < currentGroup - 1)
                continue;

            if (pointValue == currentGroup)
            {
                _currentCheckPoint = checkPoint;
                AnimateCheckPoint(checkPoint, _completedColor, false, pointValue.ToString(), 1.25f);

                if (baseIndex != currentGroup)
                {
                    return;
                }
            }
            else if (pointValue < currentGroup)
            {
                AnimateCheckPoint(checkPoint, _defaultColor, true, "", 1f);

            }
            else
            {
                AnimateCheckPoint(checkPoint, _defaultColor, false, pointValue.ToString(), 1f);
            }
        }
    }

    private void AnimateCheckPoint(GameObject checkPoint, Color color, bool showIcon, string labelText, float scale)
    {
        var image = checkPoint.GetComponent<Image>();
        var icon = checkPoint.transform.GetChild(1).gameObject;
        var label = checkPoint.GetComponentInChildren<TextMeshProUGUI>();

        checkPoint.transform.DOScale(0f, 0.25f).SetEase(Ease.InBack).OnComplete(() =>
        {
            image.color = color;
            icon.SetActive(showIcon);
            label.text = labelText;
            checkPoint.transform.DOScale(scale, 0.25f).SetEase(Ease.OutBack);
        });
    }

    private float CalculateProgressValue(int currentGroup, int currentIndex)
    {
        var groupOffset = (currentGroup - 1) % 4 * 10;

        return (currentIndex % 6 == 1) ? groupOffset / 40f : (groupOffset + currentIndex + 1) / 40f;
    }

    public void StartAuraAnimation()
    {
        _aura.transform.DORotate(
            new Vector3(0f, 0f, 360f),
            10f,
            RotateMode.FastBeyond360
        )
        .SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Incremental);
    }

    public Transform CurrentCheckPointTransform()
    {
        return _currentCheckPoint.transform;
    }
}