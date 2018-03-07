namespace GoogleARCore {

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.SceneManagement;       
    using UnityEngine.Rendering;

#if UNITY_EDITOR
    using Input = InstantPreviewInput;
#endif

    public class GameControllerAR_01 : MonoBehaviour {

        public enum GAME_STATE { IDLE, STARTING, SETTING, SPAWNING, PLAYING, ENDING };

        public GAME_STATE CurrentState {

            get { return currentstate; }

            set {
                currentstate = value;

                StopAllCoroutines();

                switch (currentstate) {

                    case GAME_STATE.IDLE:
                        StartCoroutine(GameIdle());
                        break;

                    case GAME_STATE.STARTING:
                        StartCoroutine(GameStart());
                        break;

                    case GAME_STATE.SETTING:
                        StartCoroutine(GameSetting());
                        break;

                    case GAME_STATE.SPAWNING:
                        StartCoroutine(GameSpawning());
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

        // UI Components     
        public GameObject m_StartButton, m_SearchingMessage;
        public GameObject m_GameStateUI;
        private Text m_gameStateText;
        private Fade m_Fade;

        //AR Components
        public Camera FirstPersonCamera;
        public GameObject TrackedPlanePrefab;
        public GameObject SearchingForPlaneUI;
        public GameObject m_GameRingPrefab;

        private List<TrackedPlane> m_NewPlanes = new List<TrackedPlane>();
        private List<TrackedPlane> m_AllPlanes = new List<TrackedPlane>();
        private bool m_IsQuitting = false;


        //Placement Components
        public GameObject m_StageInstance;
        public float surfaceOffset = 0.1f;

        //Spawning Components
        private Transform m_PlayerSpawn, m_EnemySpawn;
        public GameObject m_PlayerPrefab, m_EnemyPrefab;

        [SerializeField]
        GAME_STATE currentstate = GAME_STATE.SETTING;


        // Methods
        public void Start() {

            m_gameStateText = m_GameStateUI.GetComponent<Text>();
            m_Fade = GameObject.Find("FadeBall").GetComponent<Fade>();

            CurrentState = GAME_STATE.IDLE;

        }

        public void Update() {

            if (Input.GetKey(KeyCode.Escape)) {
                Application.Quit();
            }

            _QuitOnConnectionErrors();

            // Check that motion tracking is tracking.
            if (Session.Status != SessionStatus.Tracking) {
                const int lostTrackingSleepTimeout = 15;
                Screen.sleepTimeout = lostTrackingSleepTimeout;
                if (!m_IsQuitting && Session.Status.IsValid()) {
                    SearchingForPlaneUI.SetActive(true);
                }

                return;
            }

            // Iterate over planes found in this frame and instantiate corresponding GameObjects to visualize them.
            Session.GetTrackables<TrackedPlane>(m_NewPlanes, TrackableQueryFilter.New);
            for (int i = 0; i < m_NewPlanes.Count; i++) {
                // Instantiate a plane visualization prefab and set it to track the new plane. The transform is set to
                // the origin with an identity rotation since the mesh for our prefab is updated in Unity World
                // coordinates.
                GameObject planeObject = Instantiate(TrackedPlanePrefab, Vector3.zero, Quaternion.identity,
                    transform);
                planeObject.GetComponent<TrackedPlaneVisualizer>().Initialize(m_NewPlanes[i]);
            }

            // Disable the snackbar UI when no planes are valid.
            Session.GetTrackables<TrackedPlane>(m_AllPlanes);
            bool showSearchingUI = true;
            for (int i = 0; i < m_AllPlanes.Count; i++) {
                if (m_AllPlanes[i].TrackingState == TrackingState.Tracking) {
                    showSearchingUI = false;
                    break;
                }
            }

            SearchingForPlaneUI.SetActive(showSearchingUI);

        }

        ///////
        //ARCore Update requirements
        //////



        public void StartGame() {

            CurrentState = GAME_STATE.STARTING;

        }

        public void QuitGame() {

            CurrentState = GAME_STATE.ENDING;

        }


        /// <summary>
        ///   COROUTINES
        /// </summary>

        IEnumerator GameIdle() {
            //CurrentState = GAME_STATE.IDLE;
            Debug.Log(CurrentState);
            m_gameStateText.text = "Press to Start";

            while (currentstate == GAME_STATE.IDLE) {

                m_StartButton.SetActive(true);
                m_SearchingMessage.SetActive(false);

                yield return null;

            }

        }

        IEnumerator GameStart() {

            Debug.Log(CurrentState);
            m_gameStateText.text = "Starting";
             // Input.ResetInputAxes() cut because the ARCore input dosen't have a de
            

            while (currentstate == GAME_STATE.STARTING) {

                // remove button
                m_StartButton.SetActive(false);

                // call loading coroutine
                yield return LoadGameScene();

            }

            yield break;

        }

        IEnumerator GameSetting() {

            Debug.Log(CurrentState);
            m_gameStateText.text = "Setting";
            m_StartButton.SetActive(false);
            m_SearchingMessage.SetActive(true);
            // Input.ResetInputAxes();

            while (currentstate == GAME_STATE.SETTING) {

                //Testing ARController access
                yield return StartCoroutine(ARPlacement());
     
             }

            yield break;

        }

        IEnumerator GameSpawning() {
            Debug.Log(CurrentState);
            m_gameStateText.text = "Spawning";

            while (currentstate == GAME_STATE.SPAWNING) {
                Debug.Log("Starting SpawnMettles()");
                yield return new WaitForSeconds(2);
                // Spawn Mettle on the prefab     Asyncronous fire and forget because SpawnMettles() will start GamePlaying()
                StartCoroutine(SpawnMettles());
                yield break;
            }

            yield break;
        }

        IEnumerator GamePlaying() {

            Debug.Log(CurrentState);
            m_gameStateText.text = "Game Playing";

            while (currentstate == GAME_STATE.PLAYING) {

                if (Input.GetMouseButtonDown(1)) {

                    CurrentState = GAME_STATE.ENDING;
                    yield break;

                }

                yield return null;

            }

            yield break;
        }


        IEnumerator GameEnding() {

            Debug.Log(CurrentState);
            m_gameStateText.text = "Play Again?";

            while (currentstate == GAME_STATE.ENDING) {

                m_StartButton.SetActive(true);
                StartCoroutine(LoadStartScene());

                yield return null;

            }

            yield break;
        }

        //Coroutine fade
        IEnumerator Fade() {

            float fadeTime = m_Fade.BeginFade(1);
            yield return new WaitForSeconds(fadeTime);
            Debug.Log("Am I Fadeing?");
            yield break;

        }


        // Coroutine to load Game Scene
        IEnumerator LoadGameScene() {

            m_gameStateText.text = "Loading Scene";
            Debug.Log("Loading Scene");
            m_StartButton.SetActive(false);
            m_SearchingMessage.SetActive(false);
            yield return StartCoroutine(Fade());

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("ARstage");

            while (!asyncLoad.isDone) {

                yield return null;

            }

            Debug.Log("I'm switching to SETTING");
            CurrentState = GAME_STATE.SETTING;
            yield break;
        }

        // Coroutine to load Idle/Starting after game end
        IEnumerator LoadStartScene() {

            m_gameStateText.text = "Restart Game";
            Debug.Log("Loading Scene");
            m_StartButton.SetActive(false);
            m_SearchingMessage.SetActive(false);

            yield return StartCoroutine(Fade());

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("one");

            while (!asyncLoad.isDone) {

                yield return null;

            }

            Debug.Log("I'm switching to IDLE at game reset");
            CurrentState = GAME_STATE.IDLE;
            yield break;
        }



        // Coroutine to Set  Battle Area   
        IEnumerator SetRing() {

            Debug.Log("SetRing");
            m_SearchingMessage.SetActive(true);

            while (Input.GetMouseButtonDown(0)) {
                Debug.Log("Nothing happening in Set Ring");

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit)) {
                    Debug.Log("Raycast");

                    transform.position = hit.point + hit.normal * surfaceOffset;

                    if ((m_StageInstance != null) && (hit.transform.gameObject.tag == "Ground")) {
                        Debug.Log("Hit");

                        var m_stageInstance = Instantiate(m_StageInstance, hit.point, Quaternion.identity);
                        Debug.Log("I'm switching to SPAWNING");
                        CurrentState = GAME_STATE.SPAWNING;
                        m_SearchingMessage.SetActive(false);
                        yield break;

                    }
                }

                Debug.Log("I'm sitting at the base of the while statement returning null");
                yield return null;

            }

            yield break;

        }


        //ARPlacement attempts to replicate the HelloARController functionality. It's being contained in the Setting state.
        IEnumerator ARPlacement(){

            Debug.Log("This is ARPlacement");

            //Touch to place
            // If the player has not touched the screen, we are done with this update.
            Touch touch;
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began) {
                yield break;
            }

            // Raycast against the location the player touched to search for planes.
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                TrackableHitFlags.FeaturePointWithSurfaceNormal;

            if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit)) {
                var gameObject = Instantiate(m_GameRingPrefab, hit.Pose.position, hit.Pose.rotation);

                // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
                // world evolves.
                var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                // GameObject should look at the camera but still be flush with the plane.
                if ((hit.Flags & TrackableHitFlags.PlaneWithinPolygon) != TrackableHitFlags.None) {
                    // Get the camera position and match the y-component with the hit position.
                    Vector3 cameraPositionSameY = FirstPersonCamera.transform.position;
                    cameraPositionSameY.y = hit.Pose.position.y;

                    // Have GameObject look toward the camera respecting his "up" perspective, which may be from ceiling.
                    gameObject.transform.LookAt(cameraPositionSameY, gameObject.transform.up);
                }

                // Make GameObject model a child of the anchor.
                gameObject.transform.parent = anchor.transform;

                //Get out of this coroutine and into Spawning state
                m_SearchingMessage.SetActive(false);
                Debug.Log("I'm switching to SPAWNING");
                CurrentState = GAME_STATE.SPAWNING;
                yield break;
            }


            yield break;

        }

        IEnumerator SpawnMettles() {

            m_PlayerSpawn = GameObject.FindGameObjectWithTag("playerSpawn").transform;
            m_EnemySpawn = GameObject.FindGameObjectWithTag("enemySpawn").transform;

            while (m_PlayerPrefab && m_EnemyPrefab != null && m_EnemySpawn && m_PlayerSpawn != null) {

                var m_playerSpawn = Instantiate(m_PlayerPrefab, m_PlayerSpawn.transform.position, Quaternion.identity);
                var m_enemySpawn = Instantiate(m_EnemyPrefab, m_EnemySpawn.transform.position, Quaternion.identity);
                Debug.Log("I'm switching to PLAYING");
                CurrentState = GAME_STATE.PLAYING;
                yield break;
            }

            yield break;
        }

        /////////
        //ARCore ar controller 
        /////////
        /// <summary>
        /// Quit the application if there was a connection error for the ARCore session.
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
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                    AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                        message, 0);
                    toastObject.Call("show");
                }));
            }
        }

    }

}


