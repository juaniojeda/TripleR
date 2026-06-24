using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(ClassificationContainer))]
public class ConfettiManager : MonoBehaviour
{
    [Header("Configuración Visual")]
    [SerializeField] private PooledParticle confettiPrefab;
    [SerializeField] private int defaultPoolSize = 3;
    [SerializeField] private int maxPoolSize = 10;

    private ClassificationContainer container;
    private IObjectPool<PooledParticle> particlePool;

    private void Awake()
    {
        container = GetComponent<ClassificationContainer>();

        particlePool = new ObjectPool<PooledParticle>(
            createFunc: CreateParticle,
            actionOnGet: (p) => p.gameObject.SetActive(true),
            actionOnRelease: (p) => p.gameObject.SetActive(false),
            actionOnDestroy: (p) => Destroy(p.gameObject),
            collectionCheck: false,
            defaultCapacity: defaultPoolSize,
            maxSize: maxPoolSize
        );
    }

    private void OnEnable()
    {
        container.OnCorrectClassification += SpawnConfetti;
    }

    private void OnDisable()
    {
        container.OnCorrectClassification -= SpawnConfetti;
    }

    private PooledParticle CreateParticle()
    {
        PooledParticle instance = Instantiate(confettiPrefab, transform);
        instance.Initialize(particlePool);
        return instance;
    }

    private void SpawnConfetti(Vector3 spawnPosition)
    {
        PooledParticle particle = particlePool.Get();
        particle.PlayAt(spawnPosition);
    }
}