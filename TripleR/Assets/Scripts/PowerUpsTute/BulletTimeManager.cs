using System.Collections;
using UnityEngine;

public class BulletTimeManager : MonoBehaviour
{
    public static BulletTimeManager Instance { get; private set; }

    [Header("Bullet Time Settings")]
    [Range(0.05f, 1f)]
    public float slowTimeScale = 0.2f;

    public float enterDuration = 0.25f;
    public float exitDuration = 0.35f;

    [Header("Optional Audio")]
    public bool affectAudioPitch = true;

    private float originalFixedDeltaTime;
    private Coroutine bulletTimeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    public void ActivateBulletTime(float duration)
    {
        if (bulletTimeRoutine != null)
        {
            StopCoroutine(bulletTimeRoutine);
        }

        bulletTimeRoutine = StartCoroutine(BulletTimeCoroutine(duration));
    }

    private IEnumerator BulletTimeCoroutine(float duration)
    {
        yield return LerpTimeScale(Time.timeScale, slowTimeScale, enterDuration);

        // Espera en tiempo real, no afectado por Time.timeScale
        yield return new WaitForSecondsRealtime(duration);

        yield return LerpTimeScale(Time.timeScale, 1f, exitDuration);

        bulletTimeRoutine = null;
    }

    private IEnumerator LerpTimeScale(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            SetTimeScale(Mathf.Lerp(from, to, t));

            yield return null;
        }

        SetTimeScale(to);
    }

    private void SetTimeScale(float value)
    {
        Time.timeScale = value;

        // Ajusta la física para que acompañe la cámara lenta
        Time.fixedDeltaTime = originalFixedDeltaTime * value;

        if (affectAudioPitch)
        {
            AudioSource[] audioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

            foreach (AudioSource source in audioSources)
            {
                if (source != null)
                {
                    source.pitch = value;
                }
            }
        }
    }

    public void ForceResetTime()
    {
        if (bulletTimeRoutine != null)
        {
            StopCoroutine(bulletTimeRoutine);
            bulletTimeRoutine = null;
        }

        SetTimeScale(1f);
    }

    private void OnDisable()
    {
        ForceResetTime();
    }

    private void OnApplicationQuit()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;
    }
}