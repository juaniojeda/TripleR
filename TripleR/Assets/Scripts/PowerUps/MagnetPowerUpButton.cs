using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public sealed class MagnetPowerUpButton : MonoBehaviour
{
    [SerializeField] private MagnetPowerUpController magnetPowerUp;
    [SerializeField] private LayerMask pressingLayers = ~0;
    [SerializeField, Min(0f)] private float pressCooldown = 0.25f;
    [SerializeField] private bool activateOnTriggerEnter = true;

    [Header("Feedback")]
    [SerializeField] private Animator buttonAnimator;
    [SerializeField] private string pressedTrigger = "Pressed";
    [SerializeField] private ParticleSystem pressParticles;
    [SerializeField] private AK.Wwise.Event pressEvent;
    [SerializeField] private UnityEvent pressed;

    private float nextPressTime;

    private void Awake()
    {
        if (magnetPowerUp == null)
            magnetPowerUp = MagnetPowerUpController.Instance;
    }

    public void Press()
    {
        if (Time.time < nextPressTime)
            return;

        nextPressTime = Time.time + pressCooldown;

        MagnetPowerUpController target = magnetPowerUp != null
            ? magnetPowerUp
            : MagnetPowerUpController.Instance;

        if (target != null)
            target.Activate();

        PlayFeedback();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!activateOnTriggerEnter)
            return;

        if (!IsLayerAccepted(other.gameObject.layer))
            return;

        Press();
    }

    private bool IsLayerAccepted(int layer)
    {
        return (pressingLayers.value & (1 << layer)) != 0;
    }

    private void PlayFeedback()
    {
        if (buttonAnimator != null && !string.IsNullOrEmpty(pressedTrigger))
            buttonAnimator.SetTrigger(pressedTrigger);

        if (pressParticles != null)
            pressParticles.Play();

        if (pressEvent != null && pressEvent.IsValid())
            pressEvent.Post(gameObject);

        pressed?.Invoke();
    }
}
