using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PopUpPool : Singleton<PopUpPool>
{
    [SerializeField] private List<PopUpModel> _popUpPrefabs;

    private List<GameObject> _pool = new();

    public GameObject GetPopUpFromPool(string popUpId)
    {
        var instance = _pool.Find(x => x.name == popUpId);

        if (instance == null)
        {
            var prefab = _popUpPrefabs.Find(x => x.PopUpId == popUpId);

            if (prefab == null) return null;

            var newInstance = Instantiate(prefab.PopUpPrefab, transform, false);
            newInstance.name = popUpId;

            PlayPopUpAnimation(newInstance.transform);
            return newInstance;
        }

        _pool.Remove(instance);
        instance.SetActive(true);

        PlayPopUpAnimation(instance.transform);
        return instance;
    }

    private void PlayPopUpAnimation(Transform popupTransform)
    {
        var canvasGroup = popupTransform.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        popupTransform.localScale = Vector3.one * 0.5f;

        var sequence = DOTween.Sequence();
        sequence.Append(popupTransform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
        sequence.Join(canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutQuad));
    }

    public void PoolObject(GameObject obj)
    {
        var canvasGroup = obj.GetComponent<CanvasGroup>();
        obj.transform.DOKill(true);

        var sequence = DOTween.Sequence();
        sequence.Append(obj.transform.DOScale(Vector3.one * 0.5f, 0.25f).SetEase(Ease.InBack));
        sequence.Join(canvasGroup.DOFade(0f, 0.25f).SetEase(Ease.InQuad));
        sequence.OnComplete(() =>
        {
            obj.SetActive(false);
            _pool.Add(obj);
        });
    }
}
