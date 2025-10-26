using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoalDetail : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _goalDescription, _goalProgress, _coinText;
    [SerializeField] private Image _goalImage, _progressImage;
    [SerializeField] private GameObject _claimButton, _coins;
    [SerializeField] private Sprite _go, _claim;

    private GoalProgress _goalData => DailyGoalManager.Instance.GetGoals().Find(g => g.GoalData.Type.ToString() == name);

    public void LoadGoalDetail()
    {
        _goalDescription.text = _goalData.GoalData.Description;
        _coinText.text = _goalData.GoalData.Coin.ToString();

        UpdateProgress();
    }

    public void UpdateProgress()
    {
        _goalProgress.text = $"{_goalData.Progress}/{_goalData.GoalData.Amount}";
        _progressImage.fillAmount = (float)_goalData.Progress / _goalData.GoalData.Amount;

        _claimButton.GetComponentInChildren<TextMeshProUGUI>().text = _goalData.IsCompleted ? "CLAIM" : "GO";
        _claimButton.GetComponent<Image>().sprite = _goalData.IsCompleted ? _claim : _go;
        _claimButton.GetComponent<Button>().interactable = !_goalData.IsClaimed;
    }

    public void ClaimCoins()
    {
        if (!_goalData.IsCompleted)
        {
            NavigateToGoal();
            return;
        }

        if (!_goalData.IsClaimed)
        {
            _goalData.IsClaimed = true;
            _claimButton.GetComponent<Button>().interactable = false;

            DailyGoalManager.Instance.SaveGoals();
            RewardAttractor.Instance.RewardAttract(RewardType.Coin, _coins.transform, GameObject.FindGameObjectWithTag("Coin").transform, () =>
            {
                CoinBar.Instance.IncreaseCoin(_goalData.GoalData.Coin);
            });
        }
    }

    public void NavigateToGoal()
    {
        if (_goalData.GoalData.Type == GoalType.UseHint || _goalData.GoalData.Type == GoalType.CompleteLevelWords)
        {
            GameManager.Instance.CurrentGameMode = GameManager.GameMode.LevelMode;
            SceneTransitionManager.Instance.LoadScene(Constants.LEVEL_SCENE_KEY, () => LevelWordList.Instance.Initialize());
        }
        else if (_goalData.GoalData.Type == GoalType.CompleteLadderWords)
        {
            GameManager.Instance.CurrentGameMode = GameManager.GameMode.LadderMode;
            SceneTransitionManager.Instance.LoadScene(Constants.LADDER_SCENE_KEY, () => LadderWordList.Instance.Initialize());
        }
        else if (_goalData.GoalData.Type == GoalType.CompleteThemedWords)
        {
            GameManager.Instance.CurrentGameMode = GameManager.GameMode.ThemedMode;
            SceneTransitionManager.Instance.LoadScene(Constants.THEMED_SCENE_KEY, () => ThemedWordList.Instance.Initialize());
        }
    }
}