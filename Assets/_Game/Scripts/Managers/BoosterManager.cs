using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoosterManager : Singleton<BoosterManager>
{
    [SerializeField] private Button _clearButton, _revealButton;
    [SerializeField] private TextMeshProUGUI _clearText, _revealText, _clearAmount, _revealAmount;
    [SerializeField] private GameObject _clearCoin, _revealCoin, _clearRemain, _revealRemain;

    [NonSerialized] public bool[] MatchedPositions;
    [NonSerialized] public bool[] LettersRevealed;

    public void Reset()
    {
        MatchedPositions = new bool[Board.Instance.CurrentWord.Length];
        LettersRevealed = new bool[Board.Instance.CurrentWord.Length];

        UpdateClearButtonState();
        UpdateRevealButtonState();
    }

    public void DisableBoosters()
    {
        _clearButton.interactable = false;
        _revealButton.interactable = false;
    }

    public void SetLettersRevealed()
    {
        if (MatchedPositions == null || LettersRevealed == null)
            return;

        for (var i = 0; i < Board.Instance.CurrentWord.Length; i++)
        {
            LettersRevealed[i] = MatchedPositions[i] || LettersRevealed[i];
        }
    }

    public void ClearKeyboard()
    {
        if (string.IsNullOrEmpty(Board.Instance.CurrentWord)) return;

        void ClearAction()
        {
            AudioManager.Instance.PlaySFX("Game_Booster");
            DailyGoalManager.Instance.UpdateProgress(GoalType.UseHint);
            Keyboard.Instance.DisableInvalidKeys();
        }

        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.LevelMode &&
           !TutorialController.Instance.IsTutorialCompleted)
        {
            RewardManager.Instance.UseReward(RewardType.Clear, 0, onSuccessful: ClearAction);
            return;
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

    public void RevealLetter()
    {
        if (string.IsNullOrEmpty(Board.Instance.CurrentWord)) return;

        for (var i = 0; i < MatchedPositions.Length; i++)
        {
            if (MatchedPositions[i]) continue;

            void RevealAction()
            {
                MatchedPositions[i] = true;
                LettersRevealed[i] = true;

                UpdateRevealButtonState();

                AudioManager.Instance.PlaySFX("Game_Booster");
                DailyGoalManager.Instance.UpdateProgress(GoalType.UseHint);

                Board.Instance.RevealLetter(i);
                BoardSave.Instance.SaveBoard();
            }

            if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.LevelMode &&
                !TutorialController.Instance.IsTutorialCompleted)
            {
                RewardManager.Instance.UseReward(RewardType.Reveal, 0, onSuccessful: RevealAction);
                return;
            }

            if (RewardManager.Instance.Reveals > 0)
            {
                RewardManager.Instance.UseReward(RewardType.Reveal, onSuccessful: RevealAction);
            }
            else
            {
                CoinBar.Instance.DecreaseCoin(RemoteConfigs.Instance.GameConfigs.RevealPrice, onSuccessful: RevealAction);
            }

            return;
        }
    }

    public void UpdateRevealButtonState()
    {
        if (GameManager.Instance.CurrentGameMode != GameManager.GameMode.LadderMode)
        {
            _revealButton.interactable = !MatchedPositions.All(x => x);
        }
        else
        {
            _revealButton.interactable = !LadderMode.Instance.IsAllTileRevealed;
        }

        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.LevelMode &&
            !TutorialController.Instance.IsTutorialCompleted)
        {
            return;
        }

        _revealCoin.SetActive(!(RewardManager.Instance.Reveals > 0));
        _revealRemain.SetActive(RewardManager.Instance.Reveals > 0);

        if (RewardManager.Instance.Reveals > 0)
        {
            _revealAmount.text = RewardManager.Instance.Reveals.ToString();
            _revealText.text = "Free";
        }
        else
        {
            _revealText.text = RemoteConfigs.Instance.GameConfigs.RevealPrice.ToString();
        }
    }

    public void UpdateClearButtonState()
    {
        if (GameManager.Instance.CurrentGameMode != GameManager.GameMode.LadderMode)
        {
            _clearButton.interactable = !Keyboard.Instance.IsAllGrey();
        }
        else
        {
            _clearButton.interactable = !LadderMode.Instance.IsMatchWord;
        }

        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.LevelMode &&
           !TutorialController.Instance.IsTutorialCompleted)
        {
            return;
        }

        _clearCoin.SetActive(!(RewardManager.Instance.Clears > 0));
        _clearRemain.SetActive(RewardManager.Instance.Clears > 0);

        if (RewardManager.Instance.Clears > 0)
        {
            _clearAmount.text = RewardManager.Instance.Clears.ToString();
            _clearText.text = "Free";
        }
        else
        {
            _clearText.text = RemoteConfigs.Instance.GameConfigs.ClearPrice.ToString();
        }
    }
}
