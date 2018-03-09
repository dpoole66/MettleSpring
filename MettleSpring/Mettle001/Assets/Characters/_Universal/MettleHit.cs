using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MettleHit : MonoBehaviour {

    MettleHealth m_MettleHealth;            
    public string m_Opponent;


    void Start() {

        m_MettleHealth = GetComponent<MettleHealth>();

    }


    // Hit Trigger count

    //Player
    void OnTriggerEnter(Collider other) {

        if (other.gameObject.tag != (m_Opponent)) {

            return;

        } else {

            m_MettleHealth.TakeDamage(100.0f);


        }

        if (m_MettleHealth.m_HealthSlider.value <= 0) {

            m_MettleHealth.OnDeath();

        }

    }

    private void OnTriggerExit(Collider other) {

 
    }
}
