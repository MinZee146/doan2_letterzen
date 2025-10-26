using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ExpBar : Singleton<ExpBar>
{
    [SerializeField] Slider _slider;

    public void AnimateIncreaseExp()
    {
        var (startValue, endValue, isMilestoneReached) = ExpManager.Instance.CalculateIncreaseExp();
        _slider.value = startValue;

        ExpManager.Instance.IncreaseExp();

        var duration = isMilestoneReached ? (1f - startValue) : (endValue - startValue);
        var seq = DOTween.Sequence();

        if (isMilestoneReached)
        {
            seq.Append(_slider.DOValue(1f, duration * 3f).SetEase(Ease.OutQuad))
               .AppendCallback(() =>
               {
                   _slider.value = endValue < 1f ? 0f : 1f;
                   ExpManager.Instance.CheckForMilestoneRewards();
               })
               .Append(_slider.DOValue(endValue, endValue * 3f).SetEase(Ease.OutQuad));
        }
        else
        {
            seq.Append(_slider.DOValue(endValue, duration * 3f).SetEase(Ease.OutQuad));
        }
    }

    public void SpawnExp()
    {
        ExpManager.Instance.SpawnExp(GetComponent<RectTransform>(), transform.position, 500);
    }
}
