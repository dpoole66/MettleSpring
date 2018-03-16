//-----------------------------------------------------------------------
// <copyright file="HelloARController.cs" company="Google">
//
// Copyright 2017 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.HelloAR {
    using System.Collections.Generic;
    using System.Collections;
    using GoogleARCore;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.Rendering;

#if UNITY_EDITOR
    using Input = InstantPreviewInput;
#endif

    /// <summary>
    /// Controls the HelloAR example.
    /// </summary>
    public class HelloARController2 : MonoBehaviour {

        //ARCore stuff
        //
        //UI objects
        public GameObject SearchingForPlaneUI, StartingUI, PlayingUI, EndingUI, QuitUI, DebugUI;

        public Camera FirstPersonCamera;

        // Tracking components
        public GameObject TrackedPlanePrefab;      
        //private GameObject planePrefab;  
        //private Anchor anchor;


        /// A list to hold NEW planes ARCore began tracking in the current frame.       
        private List<TrackedPlane> m_NewPlanes = new List<TrackedPlane>();             
        /// A list to hold ALL planes ARCore is tracking in the current frame.          
        private List<TrackedPlane> m_AllPlanes = new List<TrackedPlane>();

        /// True if the app is in the process of quitting due to an ARCore connection error, otherwise false.             
        private bool m_IsQuitting = false;

        //
        //Mettle stuff
        //
        public Transform m_PlayerSpawn;
        //Stage
        public GameObject MettleStagePrefab;
        //private GameObject mettleStageInstance;
        //Mettles
        public GameObject MettlePlayerPrefab;
        //private GameObject mettlePlayerInstance;

        public GameObject m_GumBall;

        // Game States
        [SerializeField]
        GAME_STATE currentstate = GAME_STATE.STARTING;

        public enum GAME_STATE { STARTING, PLAYING, ENDING };

        public GAME_STATE CurrentState {

            get { return currentstate; }

            set {
                currentstate = value;

                StopAllCoroutines();

                switch (currentstate) {
      
                    case GAME_STATE.STARTING:
                        StartCoroutine(RoundStarting());
                        break;

                    case GAME_STATE.PLAYING:
                        StartCoroutine(RoundPlaying());
                        break;

                    case GAME_STATE.ENDING:
                        StartCoroutine(RoundEnding());
                        break;

                }
            }
        }

        //Body and Methods
        private void Start() {

            CurrentState = GAME_STATE.STARTING;

        }

        public void QuitButton() {

            //StartCoroutine(RoundEnding());
            CurrentState = GAME_STATE.ENDING;
            m_IsQuitting = true;

        }

        //UPDATE
        /// <summary>
        /// The Unity Update() method.
        /// </summary>
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

            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // Iterate over planes found in this frame and instantiate corresponding GameObjects to visualize them.
            Session.GetTrackables<TrackedPlane>(m_NewPlanes, TrackableQueryFilter.New);
            for (int i = 0; i < m_NewPlanes.Count; i++) {
                // Instantiate a plane visualization prefab and set it to track the new plane. The transform is set to
                // the origin with an identity rotation since the mesh for our prefab is updated in Unity World
                // coordinates.
                GameObject planePrefab = Instantiate(TrackedPlanePrefab, Vector3.zero, Quaternion.identity,
                    transform);
                planePrefab.GetComponent<TrackedPlaneVisualizer>().Initialize(m_NewPlanes[i]);
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


        //COROUTINES
        IEnumerator RoundStarting() {

            while (currentstate == GAME_STATE.STARTING) {

                StartingUI.SetActive(false);
                SearchingForPlaneUI.SetActive(true);

                Touch touch;             
          
                while (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began) {
                    yield return null;
                 }
                
                // Raycast against the location the player touched to search for planes.
                TrackableHit hit;
                TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                    TrackableHitFlags.FeaturePointWithSurfaceNormal;

                if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit)) {     

                    var mettleStageInstance = Instantiate(MettleStagePrefab, hit.Pose.position, hit.Pose.rotation);

                    var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                    if ((hit.Flags & TrackableHitFlags.PlaneWithinPolygon) != TrackableHitFlags.None) {

                        Vector3 cameraPositionSameY = FirstPersonCamera.transform.position;
                        cameraPositionSameY.y = hit.Pose.position.y;

                        mettleStageInstance.transform.LookAt(cameraPositionSameY, mettleStageInstance.transform.up);

                    }

                    mettleStageInstance.transform.parent = anchor.transform;
                    yield return new WaitForSeconds(3);
                    yield return SpawnMettles();
                    CurrentState = GAME_STATE.PLAYING;
                    break;                            
                }                 
             }
        }

        IEnumerator RoundPlaying() {

            while (currentstate == GAME_STATE.PLAYING) {

                SearchingForPlaneUI.SetActive(false);
                StartingUI.SetActive(false);
                PlayingUI.SetActive(true);
                QuitUI.SetActive(true);   
                m_GumBall.SetActive(false);

                //CurrentState = GAME_STATE.ENDING;
                
                yield return null;
             }
        }                                                                                                                                                          

    

        IEnumerator RoundEnding() {

            while (currentstate == GAME_STATE.ENDING) {

                m_GumBall.SetActive(false);

                PlayingUI.SetActive(false);
                EndingUI.SetActive(true);
                QuitUI.SetActive(false);

                yield return new WaitForSeconds(3);

                EndingUI.SetActive(false);
                SceneManager.LoadScene("Start");

                CurrentState = GAME_STATE.STARTING;

             }

        }

        // Supporting Coroutines
        IEnumerator SpawnMettles(){

            m_PlayerSpawn = GameObject.FindGameObjectWithTag("Spawn_Player").transform;
            var mettlePlayerInstance = Instantiate(MettlePlayerPrefab, m_PlayerSpawn.transform.position, Quaternion.identity);

            yield break;

        }

  
       /* IEnumerator TouchPlace() {

            while (true) {

                Touch touch;

                while (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began) {
                    yield return null;
                 }

                // Raycast against the location the player touched to search for planes.
                TrackableHit hit;
                TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                    TrackableHitFlags.FeaturePointWithSurfaceNormal;

                while (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit)) {
                    mettleStageInstance = Instantiate(MettleStagePrefab, hit.Pose.position, hit.Pose.rotation);

                    anchor = hit.Trackable.CreateAnchor(hit.Pose);
  
                    if ((hit.Flags & TrackableHitFlags.PlaneWithinPolygon) != TrackableHitFlags.None) {

                        Vector3 cameraPositionSameY = FirstPersonCamera.transform.position;
                        cameraPositionSameY.y = hit.Pose.position.y;

                        mettleStageInstance.transform.LookAt(cameraPositionSameY, mettleStageInstance.transform.up);

                        //yield break;

                    }

                    mettleStageInstance.transform.parent = anchor.transform;

                    yield break;
                 }

                //yield return null;

             }
        }  */

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
