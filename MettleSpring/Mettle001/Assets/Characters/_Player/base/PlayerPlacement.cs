using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlacement : MonoBehaviour {

    public float surfaceOffset = 0.1f;
   // [HideInInspector]public GameObject m_PlayerInstance;
    public GameObject m_StageInstance;
    public GameManager m_GameManager;
    private GameObject m_PlayBall, m_GumBall;


    private void Start() {
        m_GameManager = GameObject.Find("GameController"). GetComponent<GameManager>();
        m_PlayBall = this.gameObject;
        m_PlayBall.SetActive(true);

        m_GumBall = GameObject.FindGameObjectWithTag("IAmGumBall");
        m_GumBall.SetActive(false);

    }

    public void Update() {

        if (!Input.GetMouseButtonDown(0)) {

            return;

        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit)) {
            
            return;
        }

        transform.position = hit.point + hit.normal * surfaceOffset;

        if ((m_StageInstance != null) && (hit.transform.gameObject.tag == "Ground")) {
            
            var m_stageInstance = Instantiate(m_StageInstance, hit.point, Quaternion.identity);
            
            m_GumBall.SetActive(true);
            this.gameObject.SetActive(false);

            m_GameManager.SetGameRing(true);

        }
    }
}
