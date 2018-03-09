using System;
using UnityEngine;
using UnityEngine.AI;


public class AIMettleControl : MonoBehaviour {

    public int m_MettleNumber = 1;
    public NavMeshAgent M_Agent { get; private set; }
    public PlayerStateController M_Character { get; private set; }
    private Transform target;
    [HideInInspector] public GameObject m_Enemy;
    [HideInInspector] public Animator m_Anim;
    //[HideInInspector] public MettleStatus m_PlayerStats;

    // positions and smoothing for movement
    Vector2 smoothDeltaPosition = Vector2.zero;
    Vector2 velocity = Vector2.zero;

    private void Start() {
        // get the components on the object we need ( should not be null due to require component so no need to check )
        M_Agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
        m_Anim = GetComponent<Animator>();
        M_Character = GetComponent<PlayerStateController>();
        m_Enemy = GameObject.FindWithTag("Enemy");

        // for Player M_Agent will follow animation so allow updates
        M_Agent.updateRotation = true;
        M_Agent.updatePosition = true;

    }

    private void Update() {

        if (m_Enemy != null) {

            Debug.Log("Enemy Detected");

        }

        // Get player world position
        Vector3 worldDeltaPosition = M_Agent.nextPosition - m_Anim.rootPosition;
        // Map npc to local space
        float dirX = Vector3.Dot(transform.right, worldDeltaPosition);
        float dirY = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dirX, dirY);     // deltaPosition is the local space vector

        //  filter the movement by returning the smallest value of 2 inputs, 1 and time/.# seconds
        float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
        smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);         // local pos after smoothing

        //Update velocity with time advance
        if (Time.deltaTime > 1e-5f) {

            velocity = smoothDeltaPosition / Time.deltaTime;

        }

        bool isMoving = velocity.magnitude > 0.5f && M_Agent.remainingDistance > M_Agent.radius;

        // Send local space coords to the animator
        m_Anim.SetBool("Moveing", isMoving);
        m_Anim.SetFloat("Turn", dirX);
        m_Anim.SetFloat("Forward", dirY);

        // Quaternion rotation to enemy
        Vector3 relativePos = m_Enemy.transform.position - M_Agent.transform.position;
        Quaternion lookRotation = Quaternion.Slerp(M_Agent.transform.rotation, Quaternion.LookRotation(relativePos), Time.deltaTime * 4.0f);

        // Talk to PlaceTargetWithMouse
        if (target != null) {
            M_Agent.SetDestination(target.position);
        }

        //Idle
        if (M_Agent.remainingDistance <= M_Agent.stoppingDistance) {

            M_Agent.isStopped = true;
            M_Character.CurrentState = PlayerStateController.PLAYER_STATE.IDLE;
            m_Anim.SetTrigger("Idle");
            M_Agent.transform.rotation = lookRotation;


        } else {           //Walk
            M_Agent.isStopped = false;
            M_Character.CurrentState = PlayerStateController.PLAYER_STATE.MOVE;
            m_Anim.SetTrigger("Walking");
        }

    }


    public void SetTarget(Transform target) {
        this.target = target;
    }

}

