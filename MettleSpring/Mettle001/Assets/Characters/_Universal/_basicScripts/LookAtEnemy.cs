using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class LookAtEnemy : MonoBehaviour {

    public Transform m_MettleHead;
    public Transform m_EnemyMettle;
    public Vector3 lookAtTargetPosition;
    public float lookAtCoolTime = 0.2f;
    public float lookAtHeatTime = 0.2f;
    private bool isLooking;

    private Vector3 lookAtPosition;
    private Animator m_Animator;
    private float lookAtWeight = 0.0f;

    void Start() {
        if (!m_MettleHead) {
            Debug.LogError("No head transform - LookAt disabled");
            enabled = false;
            return;
        }
        m_Animator = GetComponent<Animator>();
        lookAtTargetPosition = m_MettleHead.position + transform.forward;
        lookAtPosition = lookAtTargetPosition;

        isLooking = false;
        m_Animator.SetBool("isLooking", false);

        m_EnemyMettle = GetComponent<Transform>();
    }

    void Update() {
        if ((Vector3.Distance(m_EnemyMettle.position, m_MettleHead.position) < 7.0f)) {

            if (Input.GetButton("LookAt")) {
                isLooking = true;
                m_Animator.SetBool("isLooking", true);
            } else {
                m_Animator.SetBool("isLooking", false);
                isLooking = false;
            }
        }  
    }


    void OnAnimatorIK() {
        // shitbird

        if (Input.GetButton("LookAt")) {
            lookAtTargetPosition.y = m_MettleHead.position.y;
            float lookAtTargetWeight = isLooking ? 1.0f : 0.0f;

            Vector3 curDir = lookAtPosition - m_MettleHead.position;
            Vector3 futDir = lookAtTargetPosition - m_MettleHead.position;

            curDir = Vector3.RotateTowards(curDir, futDir, 26.28f * Time.deltaTime, float.PositiveInfinity);
            lookAtPosition = m_MettleHead.position + curDir;

            float blendTime = lookAtTargetWeight > lookAtWeight ? lookAtHeatTime : lookAtCoolTime;
            lookAtWeight = Mathf.MoveTowards(lookAtWeight, lookAtTargetWeight, Time.deltaTime / blendTime);
            m_Animator.SetLookAtWeight(lookAtWeight, 0.2f, 0.5f, 0.7f, 0.5f);
            m_Animator.SetLookAtPosition(lookAtPosition);

            Debug.Log(lookAtPosition);
        }
    }

}
