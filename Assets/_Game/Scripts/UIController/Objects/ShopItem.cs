using TMPro;
using UnityEngine;

public class ShopItem : MonoBehaviour
{
    public bool IsBestValue;
    
    [SerializeField] private TextMeshProUGUI _price, _bonusPercentage, _value;
    [SerializeField] private GameObject _bestIcon;

    public void LoadInfo(string price, string bonusPercentage, string value)
    {
        _price.text = price;
        _bonusPercentage.text = bonusPercentage;
        _value.text = value;
        
        _bestIcon.SetActive(IsBestValue);
    }
}
