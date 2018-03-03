using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetectHit : MonoBehaviour {
    //[HideInInspector] public AudioSource m_NPCAudio;
    [HideInInspector] public Slider m_P_healthbar;
    private GameObject m_UI_player;
    Animator m_Anim;
    public string m_Opponent;

    void Start() {

        m_Anim = GetComponent<Animator>();
        m_UI_player = GameObject.Find("UI_player");
        m_P_healthbar = m_UI_player.GetComponentInChildren<Slider>();
        m_P_healthbar.value = 100;

    }


    // Hit Trigger count
    void OnTriggerEnter(Collider other) {

        if (other.gameObject.tag != m_Opponent) {

            m_Anim.SetBool("isHit", true);
            Debug.Log("Hit");
            return;

        } else  {

            m_P_healthbar.value -= 5;

        }

        if(m_P_healthbar.value <= 0){

            m_Anim.SetBool("isDead", true);

        }
      
    }

    private void OnTriggerExit(Collider other) {
        m_Anim.SetBool("isHit", false);
    }


}
