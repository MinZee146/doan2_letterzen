using UnityEngine;

using Genix.MocaLib.Runtime.Services;
using Genix.MocaLib.Runtime.Services.Internal;

public class FIAMPopupHandler : IFIAMPopupHandler
{
    public void Process(PopupButton? act)
    {
        if (act == null) return;

        var action = (PopupButton) act;
        switch (action.ActionType)
        {
            case PopupButtonActionType.Dismiss:
                ProcessDismiss(action.Data);
                break;
            case PopupButtonActionType.OpenScreen:
                ProcessOpenScreen(action.Data);
                break;
            case PopupButtonActionType.OpenURL:
                ProcessOpenURL(action.Data);
                break;
            case PopupButtonActionType.RateApp:
                ProcessRateApp(action.Data);
                break;
            case PopupButtonActionType.Purchase:
                ProcessPurchase(action.Data);
                break;
            case PopupButtonActionType.ShowInterstitial:
                ProcessShowInterstitial(action.Data);
                break;
            case PopupButtonActionType.ShowRewardedVideo:
                ProcessShowRewardedVideo(action.Data);
                break;
        }
    }

    private void ProcessDismiss(string data)
    {
    }

    private void ProcessOpenScreen(string data)
    {
    }

    private void ProcessOpenURL(string data)
    {
        Application.OpenURL(data);
    }

    private void ProcessRateApp(string data)
    {
    }

    private void ProcessPurchase(string data)
    {
    }

    private void ProcessShowInterstitial(string data)
    {
    }

    private void ProcessShowRewardedVideo(string data)
    {
    }
}
