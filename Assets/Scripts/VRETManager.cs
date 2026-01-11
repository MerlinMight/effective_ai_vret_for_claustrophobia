using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class VRETManager : MonoBehaviour
{
    private float sessionStartTime;
    public VideoPlayer videoPlayer; // The VideoPlayer component
    public Material skyboxMaterial; // The Skybox material
    public VideoClip calmScenario; // Calm scenario video
    public Levels[] levels; // Array to store levels and their videos

    private RenderTexture renderTexture; // Render Texture for the Skybox
    private int currentLevel = 0;
    private int currentVideoIndex = 0;

    [System.Serializable]
    public class Levels
    {
        public VideoClip[] videos; // Videos for each level
    }

    public Transform headTransform; // Reference to the player's head or camera
    public float sensitivity = 7.0f; // Mouse sensitivity for rotation

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;
    private float movementThreshold = 3.0f;
    private int movementCount = 0;
    private bool isInCalmScenario = false;
    private Coroutine returnToScenarioCoroutine;

    void Start()
    {
        sessionStartTime = Time.time;
        ReportGenerator.Instance.patientName = "John Doe";
        SetupRenderTexture();
        StartCoroutine(PlayVideoWithDelay());
    }

    void Update()
    {
        HandleHeadRotation();
        if (headTransform != null)
        {
            HandleHeadMovement();
        }
    }

    void SetupRenderTexture()
    {
        renderTexture = new RenderTexture(1920, 1080, 24);
        renderTexture.Create();
        videoPlayer.targetTexture = renderTexture;
        skyboxMaterial.SetTexture("_MainTex", renderTexture);
        RenderSettings.skybox = skyboxMaterial;
        DynamicGI.UpdateEnvironment();
    }

    private IEnumerator PlayVideoWithDelay()
    {
        yield return new WaitUntil(() => videoPlayer.isActiveAndEnabled);
        PlayCurrentVideo();
    }

    public void PlayNextLevel()
    {
        if (currentLevel < levels.Length - 1)
        {
            ReportGenerator.Instance.RecordLevelTime("Level " + (currentLevel + 1), Time.time - sessionStartTime);
            sessionStartTime = Time.time;
            currentLevel++;
            currentVideoIndex = 0;
            PlayCurrentVideo();
        }
        else
        {
            Debug.Log("You are already at the highest level!");
        }
    }

    public void CycleVideos()
    {
        currentVideoIndex = (currentVideoIndex + 1) % levels[currentLevel].videos.Length;
        PlayCurrentVideo();
    }

    public void PlayCalmScenario()
    {
        ReportGenerator.Instance.RecordPanicButtonPress("Level " + (currentLevel + 1));
        if (calmScenario != null)
        {
            videoPlayer.clip = calmScenario;
            videoPlayer.Play();
            isInCalmScenario = true;
            UpdateSkyboxMaterial();
        }
        else
        {
            Debug.LogWarning("Calm scenario video is not assigned!");
        }
    }

    public void EndVRETSession()
    {
        if (ReportGenerator.Instance != null)
        {
            ReportGenerator.Instance.GeneratePDFReport();
            Debug.Log("VRET Session Ended, Report Generated.");
        }
        else
        {
            Debug.LogError("ReportGenerator instance is missing! Make sure the ReportGenerator script is attached to a GameObject in the scene.");
        }
    }

    private void PlayCurrentVideo()
    {
        if (!videoPlayer.isActiveAndEnabled)
        {
            Debug.LogError("VideoPlayer is not active or enabled!");
            return;
        }
        if (levels.Length > 0 && levels[currentLevel].videos.Length > 0)
        {
            currentVideoIndex = Mathf.Clamp(currentVideoIndex, 0, levels[currentLevel].videos.Length - 1);
            videoPlayer.clip = levels[currentLevel].videos[currentVideoIndex];
            videoPlayer.Play();
            isInCalmScenario = false;
            UpdateSkyboxMaterial();
        }
        else
        {
            Debug.LogWarning("No videos assigned for this level!");
        }
    }

    private void UpdateSkyboxMaterial()
    {
        if (skyboxMaterial != null && renderTexture != null)
        {
            skyboxMaterial.SetTexture("_MainTex", renderTexture);
            RenderSettings.skybox = skyboxMaterial;
            DynamicGI.UpdateEnvironment();
        }
    }

    private void HandleHeadRotation()
    {
        if (headTransform != null)
        {
            RenderSettings.skybox.SetFloat("_Rotation", headTransform.eulerAngles.y);
        }
    }

    private void HandleHeadMovement()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        float verticalMovement = Input.GetAxis("Vertical");
        float movementMagnitude = Mathf.Abs(mouseX) + Mathf.Abs(mouseY) + Mathf.Abs(verticalMovement);

        if (movementMagnitude > movementThreshold)
        {
            movementCount++;

            if (movementCount >= 10 && !isInCalmScenario)
            {
                if (videoPlayer != null && videoPlayer.isActiveAndEnabled)
                {
                    PlayCalmScenario();
                }
                else
                {
                    Debug.LogWarning("VideoPlayer is not active/enabled yet – skipping PlayCalmScenario.");
                }
            }
        }
        else
        {
            movementCount = Mathf.Max(0, movementCount - 1);
        }

        rotationX += mouseX * sensitivity;
        rotationY -= mouseY * sensitivity;
        rotationY = Mathf.Clamp(rotationY, -80f, 80f);
        headTransform.localRotation = Quaternion.Euler(rotationY, rotationX, 0f);
    }

    public void OnEndSessionButtonClick()
    {
        EndTherapy();
    }

    private void EndTherapy()
    {
        ReportGenerator.Instance.GeneratePDFReport();
        Debug.Log("VRET Session Ended, Report Generated.");
    }

    // New method for HeartRateMonitor to resume scenario
    public void PlayCurrentVideoExternally()
    {
        Debug.Log("Resuming previous scenario...");
        PlayCurrentVideo();
    }
}