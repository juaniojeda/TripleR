using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class ScoreTextAnimator
{
    private readonly MonoBehaviour coroutineOwner;
    private readonly Text scoreText;
    private readonly Vector3 originalScale;
    private readonly Vector3 originalPosition;

    private Coroutine activeAnimation;

    public ScoreTextAnimator(MonoBehaviour coroutineOwner, Text scoreText)
    {
        this.coroutineOwner = coroutineOwner;
        this.scoreText = scoreText;

        if (scoreText != null)
        {
            originalScale = scoreText.transform.localScale;
            originalPosition = scoreText.transform.localPosition;
        }
    }

    public void Play(int amount, float bumpScale, float bumpDuration, float shakeMagnitude, float shakeDuration)
    {
        if (scoreText == null || coroutineOwner == null)
            return;

        StopActiveAnimation();

        if (amount > 0)
            activeAnimation = coroutineOwner.StartCoroutine(BumpAnimation(bumpScale, bumpDuration));
        else if (amount < 0)
            activeAnimation = coroutineOwner.StartCoroutine(ShakeAnimation(shakeMagnitude, shakeDuration));
    }

    private void StopActiveAnimation()
    {
        if (activeAnimation == null)
            return;

        coroutineOwner.StopCoroutine(activeAnimation);
        scoreText.transform.localScale = originalScale;
        scoreText.transform.localPosition = originalPosition;
        activeAnimation = null;
    }

    private IEnumerator BumpAnimation(float bumpScale, float bumpDuration)
    {
        float half = bumpDuration * 0.5f;
        float t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            float progress = t / half;
            float scale = Mathf.Lerp(1f, bumpScale, Mathf.SmoothStep(0f, 1f, progress));
            scoreText.transform.localScale = originalScale * scale;
            yield return null;
        }

        t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            float progress = t / half;
            float scale = Mathf.Lerp(bumpScale, 1f, Mathf.SmoothStep(0f, 1f, progress));
            scoreText.transform.localScale = originalScale * scale;
            yield return null;
        }

        scoreText.transform.localScale = originalScale;
        activeAnimation = null;
    }

    private IEnumerator ShakeAnimation(float shakeMagnitude, float shakeDuration)
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float damping = 1f - elapsed / shakeDuration;
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude * damping;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude * damping;

            scoreText.transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);
            yield return null;
        }

        scoreText.transform.localPosition = originalPosition;
        activeAnimation = null;
    }
}
