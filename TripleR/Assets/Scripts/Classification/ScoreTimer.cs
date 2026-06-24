using UnityEngine;

public sealed class ScoreTimer
{
    private int lastDisplayedSecond = -1;

    public ScoreTimer(float initialTime, bool startImmediately = false)
    {
        CurrentTime = initialTime;
        IsRunning = startImmediately;
    }

    public float CurrentTime { get; private set; }
    public bool IsRunning { get; private set; }

    public void TurnOn() => IsRunning = true;
    public void TurnOff() => IsRunning = false;
    public void Toggle() => IsRunning = !IsRunning;

    public bool Tick(float deltaTime, out bool finished)
    {
        finished = false;

        if (!IsRunning)
            return false;

        CurrentTime -= deltaTime;

        if (CurrentTime <= 0f)
        {
            CurrentTime = 0f;
            IsRunning = false;
            finished = true;
            return true;
        }

        int currentSecond = Mathf.CeilToInt(CurrentTime);

        if (currentSecond == lastDisplayedSecond)
            return false;

        lastDisplayedSecond = currentSecond;
        return true;
    }

    public void AddSeconds(float seconds)
    {
        if (!IsRunning)
            return;

        CurrentTime += seconds;

        if (CurrentTime < 0f)
            CurrentTime = 0f;

        lastDisplayedSecond = Mathf.CeilToInt(CurrentTime);
    }
}