using System;
using System.Collections.Generic;
using MEC;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyGoalsUI : Singleton<DailyGoalsUI>
{
    [SerializeField] private GameObject _dailyGoalPrefab;
    [SerializeField] private Transform _goalContainer;
    [SerializeField] private TextMeshProUGUI _countdownText, _titleText;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Sprite _completedSprite, _inProgressSprite;
    [SerializeField] private Slider _progessBar;

    [SerializeField] private List<Image> _dayProgressList;
    [SerializeField] private List<GameObject> _dayIconList;
    [SerializeField] private List<TextMeshProUGUI> _dayTextList;

    private List<GoalDetail> _goalDetails = new();

    private void Start()
    {
        Timing.RunCoroutine(UpdateCountdown());
        UpdateTitle();
        UpdateStreak();

        foreach (var goal in DailyGoalManager.Instance.GetGoals())
        {
            var goalUI = Instantiate(_dailyGoalPrefab, _goalContainer);
            goalUI.name = goal.GoalData.Type.ToString();

            var goalDetail = goalUI.GetComponent<GoalDetail>();
            goalDetail.LoadGoalDetail();
            _goalDetails.Add(goalDetail);
        }
    }

    private void OnEnable()
    {
        UpdateGoalsProgress();
        UpdateTitle();
        UpdateStreak();
    }

    public void UpdateGoalsProgress()
    {
        foreach (var goal in _goalDetails)
        {
            goal.UpdateProgress();
        }
    }

    public void UpdateTitle()
    {
        _titleText.text = DailyGoalManager.Instance.IsAllCompleted() ?
                        "Rest up, Hero. More gifts arrive tomorrow!" :
                        "Beat today's goals and let the streak party begin!";
    }

    private IEnumerator<float> UpdateCountdown()
    {
        while (true)
        {
            var remaining = DateTime.Today.AddDays(1) - DateTime.Now;

            if (remaining.TotalSeconds <= 0f)
            {
                _countdownText.text = "Ended";
                yield break;
            }

            _countdownText.text = $"Ends in {remaining:hh\\:mm\\:ss}";

            yield return Timing.WaitForSeconds(1f);
        }
    }

    public void UpdateStreak()
    {
        var today = DateTime.Now.Date;
        var streak = DailyGoalManager.Instance.GetCurrentStreak();
        var baseDate = (streak % 7 != 0) ? today.AddDays(-(streak % 7)) : today;

        for (var i = 0; i < 7; i++)
        {
            var currentDate = baseDate.AddDays(i);
            _dayTextList[i].text = currentDate.ToString("ddd", System.Globalization.CultureInfo.InvariantCulture);

            if (currentDate < today)
            {
                _dayProgressList[i].fillAmount = 1f;
            }
            else if (currentDate == today)
            {
                var completed = DailyGoalManager.Instance.GetGoals().FindAll(g => g.IsCompleted).Count;
                var total = DailyGoalManager.Instance.GetGoals().Count;

                _dayProgressList[i].fillAmount = (float)completed / total;
            }
            else
            {
                _dayProgressList[i].fillAmount = 0f;
            }

            _dayIconList[i].SetActive(currentDate <= today || i == 6);

            if (i != 6)
            {
                _dayIconList[i].GetComponent<Image>().sprite = currentDate < today ? _completedSprite : _inProgressSprite;
            }
        }

        _progessBar.value = (float)(streak % 7) / 6;
    }
}
