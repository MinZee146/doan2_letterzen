using DG.Tweening;
using UnityEngine;

public class LootChestManager : MonoBehaviour
{
    [SerializeField] private GameObject _cursedChest, _goldenChest, _robotChest;

    private bool _isLootCollectd;

    private void AnimateChestOpen(GameObject chest)
    {
        var lid = chest.transform.GetChild(2);

        lid.DOLocalMoveY(lid.localPosition.y + 200f, 0.4f).SetEase(Ease.OutQuad);
        chest.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.3f, 5, 0.5f);
    }

    public void ChooseCursedChest()
    {
        if (_isLootCollectd) return;
        AnimateChestOpen(_cursedChest);
        _isLootCollectd = true;
    }

    public void ChooseGoldenChest()
    {
        if (_isLootCollectd) return;
        AnimateChestOpen(_goldenChest);
        _isLootCollectd = true;
    }

    public void ChooseRobotChest()
    {
        if (_isLootCollectd) return;
        AnimateChestOpen(_robotChest);
        _isLootCollectd = true;
    }
}
