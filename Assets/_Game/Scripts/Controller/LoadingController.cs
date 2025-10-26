using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Genix.MocaLib.Runtime.Common;
using Genix.MocaLib.Runtime.Services;
using Genix.MocaLib.Runtime.Startup;

public class LoadingController : BaseLoadingController
{
    protected override IEnumerator StartLoadingScreen()
    {
        var loadOperation = SceneManager.LoadSceneAsync(PlayerPrefs.GetInt(Constants.PLAYER_PREFS_IS_TUTORIAL_COMPLETED, 0) == 1 ? 1 : 2);
        loadOperation.allowSceneActivation = false;

        var step = 0;

        while (!_loadingDone)
        {
            _loadingTime += Time.deltaTime;

            switch (step)
            {
                case 0:
                case 1:
                case 2:
                    step++;
                    break;

                case 3:
                    SetVersionText();

                    step++;
                    break;

                case 4:
                    RegisterRemoteConfigFetchCompletedEvent();

                    MocaLib.Instance.Initialize(() =>
                    {
                        FIAMPopup.RegisterHandler(new FIAMPopupHandler());
                        RegisterOnProfileLoadedEvent();
                    });

                    step++;
                    break;

                case 5:
                    GameManager.Instance.Initialize();

                    step++;
                    break;

                case 6:
                    if (_loadingTime >= _maxLoadingTime)
                    {
                        _loadingTime = _maxLoadingTime;
                        _loadingDone = true;
                    }

                    break;
            }

            UpdateLoadingBarProgress(_loadingTime);
            yield return new WaitForEndOfFrame();
        }

        loadOperation.allowSceneActivation = true;
        loadOperation.completed += OnSceneLoadCompleted;
    }

    private void SetVersionText()
    {
#if UNITY_ANDROID
        _versionText.text = $"Version: {GameVersionInfo.BUILD_VERSION}";
#elif UNITY_IOS
        _versionText.text = $"Version: {GameVersionInfo.BUILD_VERSION} ({GameVersionInfo.BUILD_NUMBER})";
#endif
    }

    private void RegisterRemoteConfigFetchCompletedEvent()
    {
        MocaLib.Instance.OnRemoteConfigFetchCompleted += (succes) =>
        {
            if (succes)
            {
                //Ads
                RemoteConfigs.Instance.GameConfigs.MaxRvSpin = MocaLib.Instance.RemoteConfigManager.GetRemoteInt(
                    Constants.REMOTE_CONFIG_MAX_RV_SPINS, RemoteConfigs.Instance.GameConfigs.MaxRvSpin);

                RemoteConfigs.Instance.GameConfigs.BannerStartFromLevel =
                    MocaLib.Instance.RemoteConfigManager.GetRemoteInt(
                        Constants.REMOTE_CONFIG_BANNER_START_FROM_LEVEL,
                        RemoteConfigs.Instance.GameConfigs.BannerStartFromLevel);

                RemoteConfigs.Instance.GameConfigs.InterStartFromLevel =
                    MocaLib.Instance.RemoteConfigManager.GetRemoteInt(
                        Constants.REMOTE_CONFIG_INTER_START_FROM_LEVEL,
                        RemoteConfigs.Instance.GameConfigs.InterStartFromLevel);

                RemoteConfigs.Instance.GameConfigs.CoinsPerAd = MocaLib.Instance.RemoteConfigManager.GetRemoteInt(
                    Constants.REMOTE_CONFIG_COINS_PER_AD, RemoteConfigs.Instance.GameConfigs.CoinsPerAd);

                //Boosters Price
                RemoteConfigs.Instance.GameConfigs.RevealPrice = MocaLib.Instance.RemoteConfigManager.GetRemoteInt(
                    Constants.REMOTE_CONFIG_REVEAL_PRICE, RemoteConfigs.Instance.GameConfigs.RevealPrice);

                RemoteConfigs.Instance.GameConfigs.ClearPrice = MocaLib.Instance.RemoteConfigManager.GetRemoteInt(
                    Constants.REMOTE_CONFIG_CLEAR_PRICE, RemoteConfigs.Instance.GameConfigs.ClearPrice);

                RemoteConfigs.Instance.GameConfigs.DefinitionPrice = MocaLib.Instance.RemoteConfigManager.GetRemoteInt(
                    Constants.REMOTE_CONFIG_DEFINITION_PRICE, RemoteConfigs.Instance.GameConfigs.DefinitionPrice);

                RemoteConfigs.Instance.GameConfigs.RetryPrice = MocaLib.Instance.RemoteConfigManager.GetRemoteInt(
                    Constants.REMOTE_CONFIG_RETRY_PRICE, RemoteConfigs.Instance.GameConfigs.RetryPrice);

                //Received Coins
                RemoteConfigs.Instance.GameConfigs.CoinsCompletedWords =
                    MocaLib.Instance.RemoteConfigManager.GetRemoteInt(
                        Constants.REMOTE_CONFIG_COINS_COMPLETED_WORDS,
                        RemoteConfigs.Instance.GameConfigs.CoinsCompletedWords);

                RemoteConfigs.Instance.GameConfigs.CoinsCompletedTheme =
                    MocaLib.Instance.RemoteConfigManager.GetRemoteInt(
                        Constants.REMOTE_CONFIG_COINS_COMPLETED_THEME,
                        RemoteConfigs.Instance.GameConfigs.CoinsCompletedTheme);

                RemoteConfigs.Instance.GameConfigs.CoinsCompletedLadderGroup =
                    MocaLib.Instance.RemoteConfigManager.GetRemoteInt(
                        Constants.REMOTE_CONFIG_COINS_COMPLETED_LADDER_GROUP,
                        RemoteConfigs.Instance.GameConfigs.CoinsCompletedLadderGroup);
            }
        };
    }

    private void OnSceneLoadCompleted(AsyncOperation operation)
    {
        UIManager.Instance.ShowCoinBar();

        if (PlayerPrefs.GetInt(Constants.PLAYER_PREFS_IS_TUTORIAL_COMPLETED, 0) == 0)
        {
            UIManager.Instance.HideCoinBar();

            if (PlayerPrefs.GetInt(Constants.PLAYER_PREFS_HAS_LOADING_BEFORE, 0) == 0)
            {
                UIManager.Instance.ShowPopUp("CMInstruction", false, PopUpShowBehaviour.HIDE_PREVIOUS);
            }
            else
            {
                LevelWordList.Instance.Initialize();
            }
        }
        else
        {
            MainMenu.Instance.Initialize();
            AdController.Instance.ShowBanner();
        }
    }

    private void RegisterOnProfileLoadedEvent()
    {
        Debug.Log("RegisterOnProfileLoadedEvent");
        MocaLib.Instance.PlayerProfileManager.RegisterOnProfileLoadedEvent((success, message) =>
        {
            if (success)
            {
                ProfileManager.Instance.UpdateProfile();
                ProfileManager.Instance.PushCurrentProgress();
            }
            else
            {
                Debug.LogError(message);
            }
        });
    }
}
