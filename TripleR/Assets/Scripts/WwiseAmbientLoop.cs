using UnityEngine;

public sealed class WwiseAmbientLoop : MonoBehaviour
{
    [SerializeField] private AK.Wwise.Event playEvent;
    [SerializeField] private AK.Wwise.Event stopEvent;

    private bool isPlaying;

    private void OnEnable()
    {
        Play();
    }

    private void OnDisable()
    {
        Stop();
    }

    public void Play()
    {
        if (isPlaying)
            return;

        if (playEvent != null && playEvent.IsValid())
        {
            playEvent.Post(gameObject);
            isPlaying = true;
        }
    }

    public void Stop()
    {
        if (!isPlaying)
            return;

        if (stopEvent != null && stopEvent.IsValid())
            stopEvent.Post(gameObject);

        isPlaying = false;
    }
}
