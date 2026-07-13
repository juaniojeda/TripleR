using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class PlayerMouth : MonoBehaviour
{
    [Header("Efectos Opcionales")]
    [SerializeField] private AudioSource mouthAudio;
    [SerializeField] private AudioClip eatSound;

    private void OnTriggerEnter(Collider other)
    {
        // 1. Verificamos si el objeto en la boca es el Bullet Time
        if (other.TryGetComponent(out BulletTimePowerUp bulletTime))
        {
            bulletTime.GrabPowerUp();
            PlayFeedback();
            return; // Salimos para no seguir evaluando
        }

        // 2. Verificamos si el objeto en la boca es el Imán
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
