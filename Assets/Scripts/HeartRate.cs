using UnityEngine;
using System.Collections;

public class HeartRate : MonoBehaviour
{
    public float heartRateThreshold = 90f;
    public float returnThreshold = 85f;
    private VRETManager vretManager;
    private float aboveThresholdStartTime = -1f;
    private bool inCalmScenario = false;
    private float currentHeartRate = 0f;

    // Flags for control
    private bool canTriggerCalm = true;
    private bool calmMinimumTimeCompleted = false;

    void Start()
    {
        vretManager = FindObjectOfType<VRETManager>();
        UDPReceiver.OnHeartRateReceived += OnHeartRateReceived;
    }

    void OnDestroy()
    {
        UDPReceiver.OnHeartRateReceived -= OnHeartRateReceived;
    }

    void OnHeartRateReceived(int bpm)
    {
        currentHeartRate = bpm;
        Debug.Log("HeartRate.cs received BPM: " + bpm);

        if (inCalmScenario || !canTriggerCalm) return;

        if (bpm > heartRateThreshold)
        {
            if (aboveThresholdStartTime < 0)
                aboveThresholdStartTime = Time.time;

            if (Time.time - aboveThresholdStartTime >= 3f)
            {
                TriggerCalmScene();
            }
        }
        else
        {
            aboveThresholdStartTime = -1f;
        }
    }

    void TriggerCalmScene()
    {
        if (inCalmScenario || vretManager == null || !canTriggerCalm) return;

        Debug.Log("Triggering calm scene from HeartRate.cs");
        vretManager.PlayCalmScenario();
        inCalmScenario = true;
        canTriggerCalm = false; // Prevent re-triggering until calm scenario ends

        StartCoroutine(WaitThenCheckForReturn());
    }

    IEnumerator WaitThenCheckForReturn()
    {
        // Wait for 10 seconds minimum
        yield return new WaitForSeconds(10f);

        // Now wait until heart rate drops below returnThreshold
        while (currentHeartRate > returnThreshold)
        {
            yield return null; // check every frame
        }

        if (vretManager != null)
        {
            vretManager.PlayCurrentVideoExternally();
        }
        inCalmScenario = false;
        canTriggerCalm = true;  // Allow re-triggering after the calm scene
        calmMinimumTimeCompleted = false;
        Debug.Log("Returning to therapy scene after calm scenario.");
    }
}