using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour {


public NavMeshAgent M_Agent { get; private set; }
public PlayerStateController M_Char { get; private set; }
public Transform target;
//private MettleMovement m_Movement;

    private void Start() {

        M_Agent = GetComponentInChildren<NavMeshAgent>();
        M_Char = GetComponent<PlayerStateController>();
        //m_Movement = GetComponent<MettleMovement>();

        Transform target = GameObject.Find("GumBall").GetComponent<Transform>();            
        Debug.Log(target);

        M_Agent.enabled = true;
        M_Agent.updatePosition = true;
        M_Agent.updateRotation = false;

    }

    private void Update() {

        if (target != null) {

            M_Agent.SetDestination(target.transform.position);

        }

    }

        /*if (M_Agent.remainingDistance > M_Agent.stoppingDistance) {

            m_Movement.Move();

        } else M_Agent.isStopped = true;       

    }                     */

    public void SetTarget(Transform target){

        this.target = target;

    }

}
