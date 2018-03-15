using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

public class PlayerPlacement : MonoBehaviour {

    public float surfaceOffset = 0.1f;
   // [HideInInspector]public GameObject m_PlayerInstance;
    public GameObject m_StageInstance;
    public GameManagerAR m_GameManagerAR;
    private GameObject m_PlayBall;               
    public Camera ARCamera;


    private void Start() {

        m_GameManagerAR = GameObject.Find("GameControllerAR"). GetComponent<GameManagerAR>();
        m_PlayBall = this.gameObject;
        m_PlayBall.SetActive(true);

        ARCamera = GetComponent<Camera>();

    }

    public void Update() {

        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began) {
           return;
        }

        // Raycast against the location the player touched to search for planes.
        TrackableHit hit;
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
            TrackableHitFlags.FeaturePointWithSurfaceNormal;

        // if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit)) {
        if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit)) {
            var StageInstance = Instantiate(m_StageInstance, hit.Pose.position, hit.Pose.rotation);

            // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
            // world evolves.
            var anchor = hit.Trackable.CreateAnchor(hit.Pose);

            // Stage should look at the camera but still be flush with the plane.
            if ((hit.Flags & TrackableHitFlags.PlaneWithinPolygon) != TrackableHitFlags.None) {
                // Get the camera position and match the y-component with the hit position.
                Vector3 cameraPositionSameY = ARCamera.transform.position;
                cameraPositionSameY.y = hit.Pose.position.y;

                // Have Stage look toward the camera respecting his "up" perspective, which may be from ceiling.
                StageInstance.transform.LookAt(cameraPositionSameY, StageInstance.transform.up);
            }

            // Make Stage model a child of the anchor.
            StageInstance.transform.parent = anchor.transform;
            m_GameManagerAR.StagePlaced = true;

            // Turn this object off to allow movement controls to take over.

            if (m_GameManagerAR.StagePlaced == true) {
        
                Debug.Log(StageInstance);    
                Destroy(this.gameObject);

            }
        }

       
    }
}
