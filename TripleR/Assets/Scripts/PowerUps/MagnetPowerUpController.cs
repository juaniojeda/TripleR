using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public sealed class MagnetPowerUpController : MonoBehaviour
{
    [Serializable]
    private struct MagnetTarget
    {
        [SerializeField] private string categoryId;
        [SerializeField] private Transform targetPoint;

        public string CategoryId => categoryId;
        public Transform TargetPoint => targetPoint;
    }

    private struct MagnetizedWaste
    {
        public PoolableObject Poolable;
        public Rigidbody Rigidbody;
        public Transform Transform;
        public Transform Target;
        public float Speed;
        public bool OriginalUseGravity;
    }

    public static MagnetPowerUpController Instance { get; private set; }

    [Header("Duration")]
    [SerializeField, Min(0.1f)] private float activeDuration = 8f;

    [Header("Targets")]
    [SerializeField] private MagnetTarget[] magnetTargets;

    [Header("Movement")]
    [SerializeField, Min(0.1f)] private float startSpeed = 2.25f;
    [SerializeField, Min(0.1f)] private float acceleration = 12f;
    [SerializeField, Min(0.01f)] private float arrivalDistance = 0.16f;
    [SerializeField, Range(0f, 1f)] private float throwVelocityRetention = 0.35f;

    [Header("Feedback")]
    [SerializeField] private GameObject activeVisualRoot;
    [SerializeField] private ParticleSystem activationParticles;
    [SerializeField] private AK.Wwise.Event activateEvent;
    [SerializeField] private AK.Wwise.Event deactivateEvent;
    [SerializeField] private AK.Wwise.Event magnetizeEvent;
    [SerializeField] private UnityEvent activated;
    [SerializeField] private UnityEvent deactivated;

    private readonly Dictionary<string, Transform> targetsByCategory =
        new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);
    private readonly List<MagnetizedWaste> activeWastes = new List<MagnetizedWaste>(8);
    private readonly List<MagnetizedWaste> pendingWastes = new List<MagnetizedWaste>(4);

    private bool isActive;
    private float deactivateAt;
    private float arrivalDistanceSqr;

    public bool IsActive => isActive;
    public float RemainingTime => isActive ? Mathf.Max(0f, deactivateAt - Time.time) : 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Hay mas de un MagnetPowerUpController activo. Se usara el ultimo habilitado.");
        }

        Instance = this;
        arrivalDistanceSqr = arrivalDistance * arrivalDistance;
        BuildTargetCache();
        SetActiveFeedback(false);
    }

    private void OnEnable()
    {
        Instance = this;
    }

    private void OnDisable()
    {
        if (Instance == this)
            Instance = null;

        ClearAllMagnetized();
        SetActiveFeedback(false);
        isActive = false;
    }

    private void Update()
    {
        if (!isActive)
            return;

        if (Time.time >= deactivateAt)
            Deactivate();
    }

    private void FixedUpdate()
    {
        MovePendingWastes();
        MoveActiveWastes(Time.fixedDeltaTime);
    }

    public void Activate()
    {
        isActive = true;
        deactivateAt = Time.time + activeDuration;

        SetActiveFeedback(true);
        PlayEvent(activateEvent, gameObject);
        activated?.Invoke();

        if (activationParticles != null)
            activationParticles.Play();
    }

    public void Deactivate()
    {
        if (!isActive)
            return;

        isActive = false;
        SetActiveFeedback(false);
        PlayEvent(deactivateEvent, gameObject);
        deactivated?.Invoke();
    }

    public bool TryRequestMagnet(PoolableObject poolableObject)
    {
        if (!isActive || poolableObject == null)
            return false;

        PoolItemData data = poolableObject.Data;

        if (data == null || string.IsNullOrWhiteSpace(data.CategoryId))
            return false;

        if (!targetsByCategory.TryGetValue(data.CategoryId, out Transform target) || target == null)
            return false;

        Rigidbody rb = poolableObject.Rigidbody;

        if (rb == null)
            return false;

        EnqueueMagnet(poolableObject, rb, target);
        return true;
    }

    public void CancelMagnet(PoolableObject poolableObject)
    {
        if (poolableObject == null)
            return;

        RemoveFromList(activeWastes, poolableObject, true);
        RemoveFromList(pendingWastes, poolableObject, false);
    }

    private void BuildTargetCache()
    {
        targetsByCategory.Clear();

        if (magnetTargets == null)
            return;

        for (int i = 0; i < magnetTargets.Length; i++)
        {
            string categoryId = magnetTargets[i].CategoryId;
            Transform target = magnetTargets[i].TargetPoint;

            if (string.IsNullOrWhiteSpace(categoryId) || target == null)
                continue;

            targetsByCategory[categoryId] = target;
        }
    }

    private void EnqueueMagnet(PoolableObject poolableObject, Rigidbody rb, Transform target)
    {
        CancelMagnet(poolableObject);

        MagnetizedWaste waste = new MagnetizedWaste
        {
            Poolable = poolableObject,
            Rigidbody = rb,
            Transform = poolableObject.transform,
            Target = target,
            Speed = startSpeed,
            OriginalUseGravity = rb.useGravity
        };

        // Unselect llega fuera de FixedUpdate; la física cambia en el siguiente paso fijo.
        pendingWastes.Add(waste);
        PlayEvent(magnetizeEvent, poolableObject.gameObject);
    }

    private void MovePendingWastes()
    {
        if (pendingWastes.Count == 0)
            return;

        for (int i = pendingWastes.Count - 1; i >= 0; i--)
        {
            MagnetizedWaste waste = pendingWastes[i];
            RemoveAtSwapBack(pendingWastes, i);

            if (!IsWasteValid(waste))
                continue;

            waste.Rigidbody.useGravity = false;
            waste.Rigidbody.linearVelocity *= throwVelocityRetention;
            activeWastes.Add(waste);
        }
    }

    private void MoveActiveWastes(float deltaTime)
    {
        for (int i = activeWastes.Count - 1; i >= 0; i--)
        {
            MagnetizedWaste waste = activeWastes[i];

            if (!IsWasteValid(waste))
            {
                RemoveAtSwapBack(activeWastes, i);
                continue;
            }

            Vector3 currentPosition = waste.Rigidbody.position;
            Vector3 targetPosition = waste.Target.position;
            Vector3 toTarget = targetPosition - currentPosition;

            if (toTarget.sqrMagnitude <= arrivalDistanceSqr)
            {
                FinishMagnetizedWaste(waste);
                RemoveAtSwapBack(activeWastes, i);
                continue;
            }

            waste.Speed += acceleration * deltaTime;
            Vector3 desiredVelocity = toTarget.normalized * waste.Speed;
            Vector3 blendedVelocity = Vector3.Lerp(waste.Rigidbody.linearVelocity, desiredVelocity, 0.55f);

            waste.Rigidbody.linearVelocity = blendedVelocity;
            activeWastes[i] = waste;
        }
    }

    private bool IsWasteValid(MagnetizedWaste waste)
    {
        return waste.Poolable != null
            && waste.Rigidbody != null
            && waste.Target != null
            && waste.Poolable.gameObject.activeInHierarchy;
    }

    private void FinishMagnetizedWaste(MagnetizedWaste waste)
    {
        // La caída final evita que el residuo quede flotando sobre el trigger del contenedor.
        waste.Rigidbody.useGravity = waste.OriginalUseGravity;
        waste.Rigidbody.linearVelocity = Vector3.down * Mathf.Max(0.5f, startSpeed * 0.4f);
    }

    private void ClearAllMagnetized()
    {
        for (int i = activeWastes.Count - 1; i >= 0; i--)
        {
            MagnetizedWaste waste = activeWastes[i];

            if (waste.Rigidbody != null)
                waste.Rigidbody.useGravity = waste.OriginalUseGravity;
        }

        activeWastes.Clear();
        pendingWastes.Clear();
    }

    private void RemoveFromList(List<MagnetizedWaste> list, PoolableObject poolableObject, bool restoreGravity)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            MagnetizedWaste waste = list[i];

            if (waste.Poolable != poolableObject)
                continue;

            if (restoreGravity && waste.Rigidbody != null)
                waste.Rigidbody.useGravity = waste.OriginalUseGravity;

            RemoveAtSwapBack(list, i);
        }
    }

    private static void RemoveAtSwapBack(List<MagnetizedWaste> list, int index)
    {
        // El orden no importa y así no se desplaza toda la lista en cada baja.
        int lastIndex = list.Count - 1;
        list[index] = list[lastIndex];
        list.RemoveAt(lastIndex);
    }

    private void SetActiveFeedback(bool active)
    {
        if (activeVisualRoot != null)
            activeVisualRoot.SetActive(active);
    }

    private static void PlayEvent(AK.Wwise.Event wwiseEvent, GameObject target)
    {
        if (wwiseEvent != null && wwiseEvent.IsValid())
            wwiseEvent.Post(target);
    }
}
