using System;
using AssetKits.ParticleImage;
using UnityEngine;

public class RewardAttractor : Singleton<RewardAttractor>
{
    [SerializeField] ParticleImage _particles;
    [SerializeField] Sprite _coin, _reveal, _clear;

    public void RewardAttract(RewardType rewardType, Transform startPosition, Transform endPosition, Action onFirstParticleFinished = null)
    {
        _particles.transform.position = startPosition.position;
        _particles.attractorTarget = endPosition;

        if (rewardType == RewardType.Coin)
        {
            _particles.rateOverLifetime = 10;
            _particles.startSize = new SeparatedMinMaxCurve(75);
            _particles.sprite = _coin;
        }
        else
        {
            _particles.rateOverLifetime = 1;
            _particles.startSize = new SeparatedMinMaxCurve(125);

            if (rewardType == RewardType.Reveal)
                _particles.sprite = _reveal;
            else if (rewardType == RewardType.Clear)
                _particles.sprite = _clear;
        }

        _particles.onParticleStarted.RemoveAllListeners();
        _particles.onParticleStop.RemoveAllListeners();
        _particles.onFirstParticleFinished.RemoveAllListeners();
        _particles.onAnyParticleFinished.RemoveAllListeners();

        _particles.onParticleStarted.AddListener(() =>
        {

        });

        _particles.onParticleStop.AddListener(() =>
        {
            _particles.transform.position = Vector2.zero;
        });

        _particles.onFirstParticleFinished.AddListener(() =>
        {
            onFirstParticleFinished?.Invoke();
        });

        _particles.onAnyParticleFinished.AddListener(() =>
        {

        });

        _particles.Play();
    }
}
