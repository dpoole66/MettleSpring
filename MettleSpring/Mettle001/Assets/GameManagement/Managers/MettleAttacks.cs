using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MettleAttacks : MonoBehaviour {

    public int m_PlayerNumber = 1;              // Used to identify the different players.
    //public Rigidbody m_Shell;                   // Prefab of the shell.
    //public Transform m_FireTransform;           // A child of the tank where the shells are spawned.
    //public Slider m_AimSlider;                  // A child of MettlePrefabs that displays the current launch force.
    public AudioSource m_AttackingAudio;         // Reference to the audio source used to play the Attacking. NB: different to the movement audio source.
    public AudioClip m_ChargingClip;            // Audio that plays when each Attack is charging up.
    public AudioClip m_AttackClip;                // Audio that plays when each attack is launched.
    public float m_MinAttackForce = 15f;        // The force given to the shell if the fire button is not held.
    public float m_MaxAttackForce = 30f;        // The force given to the shell if the fire button is held for the max charge time.
    public float m_MaxChargeTime = 0.75f;       // How long the shell can charge for before it is fired at max force.


    private string m_FireButton;                // The input axis that is used for launching shells.
    private float m_CurrentAttackForce;         // The force that will be given to the shell when the fire button is released.
    private float m_ChargeSpeed;                // How fast the launch force increases, based on the max charge time.
    private bool m_Fired;                       // Whether or not the shell has been launched with this button press.
    private float nextFireTime;

    private void OnEnable() {
        // When the tank is turned on, reset the launch force and the UI
        m_CurrentAttackForce = m_MinAttackForce;
        //m_AimSlider.value = m_MinAttackForce;
    }


    private void Start() {
        // The fire axis is based on the player number.
        m_FireButton = "Fire" + m_PlayerNumber;

        // The rate that the launch force charges up is the range of possible forces by the max charge time.
        m_ChargeSpeed = (m_MaxAttackForce - m_MinAttackForce) / m_MaxChargeTime;
    }


    private void Update() {
        // The slider should have a default value of the minimum launch force.
       // m_AimSlider.value = m_MinAttackForce;

        // If the max force has been exceeded and the shell hasn't yet been launched...
        if (m_CurrentAttackForce >= m_MaxAttackForce && !m_Fired) {
            // ... use the max force and launch the shell.
            m_CurrentAttackForce = m_MaxAttackForce;
            Fire(m_CurrentAttackForce, 1);
        }
        // Otherwise, if the fire button has just started being pressed...
        else if (Input.GetButtonDown(m_FireButton)) {
            // ... reset the fired flag and reset the launch force.
            m_Fired = false;
            m_CurrentAttackForce = m_MinAttackForce;

            // Change the clip to the charging clip and start it playing.
            m_AttackingAudio.clip = m_ChargingClip;
            m_AttackingAudio.Play();
        }
        // Otherwise, if the fire button is being held and the shell hasn't been launched yet...
        else if (Input.GetButton(m_FireButton) && !m_Fired) {
            // Increment the launch force and update the slider.
            m_CurrentAttackForce += m_ChargeSpeed * Time.deltaTime;

            //m_AimSlider.value = m_CurrentAttackForce;
        }
        // Otherwise, if the fire button is released and the shell hasn't been launched yet...
        else if (Input.GetButtonUp(m_FireButton) && !m_Fired) {
            // ... launch the shell.
            Fire(m_CurrentAttackForce, 1);

        }
    }


    public void Fire(float attackForce, float attackRate) {
        if (Time.time > nextFireTime) {
            nextFireTime = Time.time + attackRate;
            // Set the fired flag so only Fire is only called once.
            m_Fired = true;

            // Change the clip to the firing clip and play it.
            m_AttackingAudio.clip = m_AttackClip;
            m_AttackingAudio.Play();

            // Reset the launch force.  This is a precaution in case of missing button events.
            m_CurrentAttackForce = m_MinAttackForce;
        }

    }
}
