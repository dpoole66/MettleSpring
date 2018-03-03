using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour {


public NavMeshAgent M_Agent { get; private set; }
public PlayerCharacter M_Char { get; private set; }
public Transform target;

    private void Start() {

        M_Agent = GetComponent<NavMeshAgent>();
        M_Char = GetComponent<PlayerCharacter>();

        M_Agent.enabled = true;
        M_Agent.updatePosition = true;
        M_Agent.updateRotation = false;

    }

    private void Update() {
        
        if(target != null){

                M_Agent.SetDestination(target.transform.position);

        }

        if (M_Agent.remainingDistance > M_Agent.stoppingDistance) {

            M_Char.Move(M_Agent.desiredVelocity, false, false);

        } else M_Char.Move(Vector3.zero, false, false);

    }

    public void SetTarget(Transform target){

        this.target = target;

    }

}
