using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using GoogleARCore;

public class SceneController : MonoBehaviour {

    public GameObject SearchingForPlaneUI;
    public GameObject m_GameStateUI;
    private Text m_gameStateUI;
    private bool m_IsQuitting = false;

    public GameObject TrackedPlanePrefab;
    private List<TrackedPlane> m_NewPlanes = new List<TrackedPlane>();
    private List<TrackedPlane> m_AllPlanes = new List<TrackedPlane>();

    public Camera FirstPersonCamera;

    public GameObject m_StageInstance;
    public float surfaceOffset = 0.1f;

    GAME_STATE currentstate;

    // STATES
    public enum GAME_STATE { START, PLAYING, ENDING };

    public GAME_STATE CurrentState {

        get { return currentstate; }

        set {
            currentstate = value;

            StopAllCoroutines();

            switch (currentstate) {

                case GAME_STATE.START:
                    StartCoroutine(GameStart());
                    break;
  
                case GAME_STATE.PLAYING:
                    StartCoroutine(GamePlaying());
                    break;

                case GAME_STATE.ENDING:
                    StartCoroutine(GameEnding());
                    break;

            }
        }  
    }

    // STATE_COs
    IEnumerator GameStart() {
        while (currentstate == GAME_STATE.START) {

            Debug.Log(CurrentState);
            m_gameStateUI.text = "Starting";

         }

        yield break;
    }

    IEnumerator GamePlaying() {
        while (currentstate == GAME_STATE.PLAYING) {

            Debug.Log(CurrentState);
            m_gameStateUI.text = "Playing";

         }

        yield break; 
    }

    IEnumerator GameEnding() {
        while (currentstate == GAME_STATE.ENDING) {

            Debug.Log(CurrentState);
            m_gameStateUI.text = "Ending";

         }

        yield break;
    }

    public void Start() {

        m_gameStateUI = m_GameStateUI.GetComponent<Text>();
        
    }


    // Update is called once per frame
    public void Update () {

        _QuitOnConnectionErrors();

        if (Session.Status != SessionStatus.Tracking) {
            const int lostTrackingSleepTimeout = 15;
            Screen.sleepTimeout = lostTrackingSleepTimeout;
            if (!m_IsQuitting && Session.Status.IsValid()) {
                SearchingForPlaneUI.SetActive(true);
            }

            return;
        }

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        ProcessNewPlanes();
        ClearSearchingUI();

    }

    // END UPDATE

    private void ProcessNewPlanes(){

        // Find planes and illuminate them
        Session.GetTrackables<TrackedPlane>(m_NewPlanes, TrackableQueryFilter.New);
        for (int i = 0; i < m_NewPlanes.Count; i++) {
            // Instantiate a plane visualization prefab and set it to track the new plane. The transform is set to
            // the origin with an identity rotation since the mesh for our prefab is updated in Unity World
            // coordinates.
            GameObject planeObject = Instantiate(TrackedPlanePrefab, Vector3.zero, Quaternion.identity,
                transform);
            planeObject.GetComponent<TrackedPlaneVisualizer>().Initialize(m_NewPlanes[i]);
        }

    }

    // Remove UI when planes are found
    private void ClearSearchingUI(){

        Session.GetTrackables<TrackedPlane>(m_AllPlanes);
        bool showSearchingUI = true;
        for (int i = 0; i < m_AllPlanes.Count; i++) {
            if (m_AllPlanes[i].TrackingState == TrackingState.Tracking) {
                showSearchingUI = false;
                break;
            }
        }

        SearchingForPlaneUI.SetActive(showSearchingUI);
        CurrentState = GAME_STATE.PLAYING;

    }


    /// <summary>
    /// ARCore required 
    /// </summary>

    private void _QuitOnConnectionErrors() {
        if (m_IsQuitting) {
            return;
        }

        // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted) {
            _ShowAndroidToastMessage("Camera permission is needed to run this application.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        } else if (Session.Status.IsError()) {
            _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
    }

    /// <summary>
    /// Actually quit the application.
    /// </summary>
    private void _DoQuit() {
        Application.Quit();
    }

    /// <summary>
    /// Show an Android toast message.
    /// </summary>
    /// <param name="message">Message string to show in the toast.</param>
    private void _ShowAndroidToastMessage(string message) {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null) {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                    message, 0);
                toastObject.Call("show");
            }));
        }
    }

}

