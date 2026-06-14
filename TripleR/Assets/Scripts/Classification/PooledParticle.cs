using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(ParticleSystem))]
public class PooledParticle : MonoBehaviour
{
    private IObjectPool<PooledParticle> pool;
    private ParticleSystem particleSys;

    public void Initialize(IObjectPool<PooledParticle> parentPool)
    {
        pool = parentPool;
        particleSys = GetComponent<ParticleSystem>();

        var main = particleSys.main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    public void PlayAt(Vector3 position)
    {
        transform.position = position;
        particleSys.Play();
    }

    private void OnParticleSystemStopped()
    {
        pool.Release(this);
    }
}