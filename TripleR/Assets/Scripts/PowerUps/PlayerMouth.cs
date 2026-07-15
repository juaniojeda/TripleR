using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class PlayerMouth : MonoBehaviour
{
    [Header("Efectos Opcionales")]
    [SerializeField] private AudioSource mouthAudio;
    [SerializeField] private AudioClip eatSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out BulletTimePowerUp bulletTime))
        {
            bulletTime.GrabPowerUp();
            PlayFeedback();
            return;
        }

        if (other.TryGetComponent(out MagnetPowerUpButton magnetPowerUp))
        {
            magnetPowerUp.Press();
            PlayFeedback();
            return;
        }
    }

    private void PlayFeedback()
    {
        if (mouthAudio != null && eatSound != null)
        {
            mouthAudio.PlayOneShot(eatSound);
        }
    }
}
