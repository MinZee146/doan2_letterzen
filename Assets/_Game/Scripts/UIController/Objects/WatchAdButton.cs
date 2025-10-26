using UnityEngine;
using UnityEngine.UI;

public class WatchAdButton : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => WatchAd());
    }
    
    private void WatchAd(RewardType rewardType = RewardType.Coin)
    {
        RewardAttractor.Instance.RewardAttract(RewardType.Coin, transform,
            GameObject.FindGameObjectWithTag("Coin").transform,
            () => CoinBar.Instance.IncreaseCoin(RemoteConfigs.Instance.GameConfigs.CoinsPerAd));
    }
}
