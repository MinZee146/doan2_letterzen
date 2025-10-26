using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Spin : MonoBehaviour
{
    [SerializeField] private GameObject _cover, _counter;
    [SerializeField] private Button _spinButton;
    [SerializeField] private TextMeshProUGUI _countDownText;
    [SerializeField] private PickerWheel _wheel;

    private DateTime _nextAvailableDate;
    private bool _hasSpunToday;
    private int _maxRv, _spunCounter;

    private void Start()
    {
        _maxRv = RemoteConfigs.Instance.GameConfigs.MaxRvSpin;
        _spunCounter = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_SPUN_COUNT, 0);

        _hasSpunToday = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_HAS_SPUN, 0) == 1;
        _nextAvailableDate = DailySpin.Instance.NextAvailableDate;

        _spinButton.interactable = !_hasSpunToday || _spunCounter < _maxRv;
        _countDownText.gameObject.SetActive(_hasSpunToday && _spunCounter == _maxRv);
        _counter.SetActive(_hasSpunToday);
        _counter.GetComponentInChildren<TextMeshProUGUI>().text = $"{_maxRv - _spunCounter}";

        _spinButton.GetComponentInChildren<TextMeshProUGUI>().text = _hasSpunToday && _spunCounter < _maxRv ? "Watch an ad for one more spin!" : "SPIN";

        if (_hasSpunToday)
        {
            _spinButton.onClick.AddListener(GetExtraSpin);
        }
        else
        {
            _spinButton.onClick.AddListener(DoSpin);
        }

        if (_hasSpunToday && _spunCounter == _maxRv)
        {
            InvokeRepeating(nameof(UpdateCountdown), 0f, 1f);
        }
    }

    private void UpdateCountdown()
    {
        var now = DateTime.Now;

        if (now >= _nextAvailableDate)
        {
            _countDownText.gameObject.SetActive(false);
            CancelInvoke(nameof(UpdateCountdown));
            return;
        }

        var timeLeft = _nextAvailableDate - now;
        _countDownText.text = $"Next spin in: {timeLeft:hh\\:mm\\:ss}";
    }

    public void GetExtraSpin()
    {
        AdController.Instance.ShowRewardedAd((success, code) =>
        {
            if (success)
            {
                DoSpin();
                _spunCounter++;
                PlayerPrefs.SetInt(Constants.PLAYER_PREFS_SPUN_COUNT, _spunCounter);
                PlayerPrefs.Save();

                _counter.GetComponentInChildren<TextMeshProUGUI>().text = $"{_maxRv - _spunCounter}";
            }
            else
            {
                Debug.LogWarning($"Rewarded ad failed or was skipped. Code: {code}");
            }
        });
    }

    private void DoSpin()
    {
        _cover.SetActive(true);
        _spinButton.interactable = false;

        _wheel.OnSpinEnd(piece =>
        {
            if (_spunCounter < _maxRv)
            {
                _spinButton.onClick.RemoveAllListeners();
                _spinButton.onClick.AddListener(GetExtraSpin);

                _spinButton.interactable = true;
                _spinButton.GetComponentInChildren<TextMeshProUGUI>().text = "Watch an ad for one more spin!";
            }
            else
            {
                _spinButton.GetComponentInChildren<TextMeshProUGUI>().text = "SPIN";
                _countDownText.gameObject.SetActive(true);
            }

            DailySpin.Instance.DisableSpin();

            _nextAvailableDate = DailySpin.Instance.NextAvailableDate;

            _cover.SetActive(false);
            _counter.SetActive(true);

            InvokeRepeating(nameof(UpdateCountdown), 0f, 1f);

            AudioManager.Instance.PlaySFX("Common_Completed");

            switch (piece.Label)
            {
                case "Coin":
                    RewardAttractor.Instance.RewardAttract(RewardType.Coin, _wheel.transform, GameObject.FindGameObjectWithTag("Coin").transform, () => CoinBar.Instance.IncreaseCoin(piece.Amount));
                    break;

                case "Reveal":
                    RewardAttractor.Instance.RewardAttract(RewardType.Reveal, _wheel.transform, GameObject.FindGameObjectWithTag("Avatar").transform);
                    RewardManager.Instance.AddReward(RewardType.Reveal, piece.Amount);
                    break;

                case "Clear":
                    RewardAttractor.Instance.RewardAttract(RewardType.Clear, _wheel.transform, GameObject.FindGameObjectWithTag("Avatar").transform);
                    RewardManager.Instance.AddReward(RewardType.Clear, piece.Amount);
                    break;
            }
        });

        _wheel.Spin();
    }
}
