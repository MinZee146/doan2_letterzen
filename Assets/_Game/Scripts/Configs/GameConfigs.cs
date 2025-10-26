using UnityEngine;

[CreateAssetMenu(fileName = "GameConfigs", menuName = "ScriptableObjects/Game Configs")]
public class GameConfigs : ScriptableObject
{

    [Header("Ads")]
    public int BannerStartFromLevel;
    public int InterStartFromLevel;

    public int MaxRvSpin;
    public int CoinsPerAd;

    [Header("Boosters Price")]
    public int RevealPrice;
    public int ClearPrice;
    public int DefinitionPrice;
    public int RetryPrice;

    [Header("Received Coins")]
    public int CoinsCompletedWords;
    public int CoinsCompletedTheme;
    public int CoinsCompletedLadderGroup;
}
