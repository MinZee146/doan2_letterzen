using UnityEngine;

//Attach this to a button to toggle correspond pop up
public class TogglePopUpButton : MonoBehaviour
{
    [SerializeField] private string _popUpId;
    [SerializeField] private bool _showCoinBar = false;
    [SerializeField] private PopUpShowBehaviour _popUpShowBehaviour;

    public void DoShowPopUp()
    {
        UIManager.Instance.ShowPopUp(_popUpId, _showCoinBar, _popUpShowBehaviour);
    }

    public void DoHidePopUp()
    {
        UIManager.Instance.HideLastPopUp();
    }
}
