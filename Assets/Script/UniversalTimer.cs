using UnityEngine;

public class UniversalTimer
{
    private float startTime;
    private float stoppedElapsedTime;
    private bool isRunning;

    public void StartTimer()
    {
        if (isRunning)
            return;

        startTime = Time.time - stoppedElapsedTime;
        isRunning = true;
    }

    public void StopTimer()
    {
        if (!isRunning)
            return;

        stoppedElapsedTime = Time.time - startTime;
        isRunning = false;
    }

    public float CheckTimer()
    {
        if (isRunning)
            return Time.time - startTime;

        return stoppedElapsedTime;
    }

    public void ResetTimer()
    {
        startTime = 0f;
        stoppedElapsedTime = 0f;
        isRunning = false;
    }
}