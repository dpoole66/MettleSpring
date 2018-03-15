using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


public class PlayerStateController : MonoBehaviour {

    //-------------Fields and Methods

    NavMeshAgent m_Agent = null;
    Animator m_Anim;
    GameObject m_Enemy, m_Player;
    Transform m_EnemyPosition;  
    MettleAttack m_PlayerAttack;
    float enemyRange;
    Vector3 enemyHeading;
    // Movement
    public float surfaceOffset = 0.01f;
    //Init Player with MettleManager
    private bool aiActive;


    void Awake() {

        m_Agent = GetComponent<NavMeshAgent>();
        m_Anim = GetComponent<Animator>();

    }


    void Start() {

        CurrentState = PLAYER_STATE.IDLE;

        m_Enemy = GameObject.FindWithTag("Enemy");
        m_Player = GameObject.FindWithTag("Player");

        m_EnemyPosition = m_Enemy.transform; 
        m_PlayerAttack = GetComponent<MettleAttack>();

        if (m_Enemy != null){

            Debug.Log(m_EnemyPosition);
      
        }

    }

    

    public void SetUpPlayerMettle(bool aiActivationFromMettleManager) {        //**

        aiActive = aiActivationFromMettleManager;

        if (aiActive) {

            m_Agent.enabled = true;
            CurrentState = PlayerStateController.PLAYER_STATE.IDLE;

        } else {

            m_Agent.enabled = false;

        }

    }

    public void ResetPlayerMettle() {         //**

        m_Agent.enabled = true;
        CurrentState = PlayerStateController.PLAYER_STATE.IDLE;

    }

    //-------------Player finite state machine

    public enum PLAYER_STATE { IDLE, MOVE, ATTACK, DEFEND, INJURED, DEAD };

    [SerializeField]
    private PLAYER_STATE currentState = PLAYER_STATE.IDLE;

    // get private currentState from public encapsulation and return corresponding state
    public PLAYER_STATE CurrentState
    {

        get{ return currentState; }
        set
        {
            currentState = value;

            StopAllCoroutines();

            switch(currentState)
            {
                case PLAYER_STATE.IDLE:
                    StartCoroutine(Player_Idle());
                    break;

                case PLAYER_STATE.MOVE:
                    StartCoroutine(Player_Move());
                    break;             

                case PLAYER_STATE.ATTACK:
                    StartCoroutine(Player_Attack());
                    break;

                case PLAYER_STATE.DEFEND:
                    StartCoroutine(Player_Defend());
                    break;

                case PLAYER_STATE.INJURED:
                    StartCoroutine(Player_Injured());
                    break;

                case PLAYER_STATE.DEAD:
                    StartCoroutine(Player_Dead());
                    break;

            }

        }

    }


    public IEnumerator   Player_Idle(){

        while(currentState == PLAYER_STATE.IDLE)
        {
     
            m_Agent.isStopped = true;
            ClearAnimParams();
            m_Anim.SetTrigger("Idle");           
          

            if (m_Agent.remainingDistance <= m_Agent.stoppingDistance){

                m_Agent.transform.LookAt(m_EnemyPosition);

            }
                    
                
            if (Input.GetMouseButtonDown(0)) 
            {
                    if(Input.GetMouseButtonDown(0)){

                    RaycastHit hit;

                        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100)){

                            m_Agent.destination = hit.point;

                            m_Agent.isStopped = false;
                            CurrentState = PLAYER_STATE.MOVE;

                            yield break;
                        }

                    }

            }

            // attack  and move input button
            if (Input.GetMouseButtonDown(1)) {

                m_Agent.isStopped = true;
                m_Anim.ResetTrigger("Idle");
                CurrentState = PLAYER_STATE.ATTACK;

                yield break;

            }

            // attack  and move input button
            if (Input.GetMouseButton(2)) {

                m_Agent.isStopped = true;
                m_Anim.ResetTrigger("Idle");
                CurrentState = PLAYER_STATE.DEFEND;

                yield break;

            }

            yield return null;
        }

    }


    //---MOVE
    public IEnumerator Player_Move() {

        while(currentState == PLAYER_STATE.MOVE)
        {      
         
            //Wait until path is computed
            while (m_Agent.pathPending)
                yield return null;

            m_Agent.isStopped = false;
            //ClearAnimParams();
            m_Anim.SetTrigger("Walking");   

                // attack  and move input button
                if (Input.GetMouseButtonDown(1)) {

                m_Agent.isStopped = true;
                CurrentState = PLAYER_STATE.ATTACK;

                    yield break;

                }

                // stop  and defend input button
                if (Input.GetMouseButton(2)) {

                m_Agent.isStopped = true;
                CurrentState = PLAYER_STATE.DEFEND;

                    yield break;

                }

                // Once Player is at destination
                if (m_Agent.remainingDistance <= m_Agent.stoppingDistance) 
                {

                m_Agent.isStopped = true;
                CurrentState = PLAYER_STATE.IDLE;

                    yield break;
                }

            yield return null;
            }

    }

    public IEnumerator Player_Attack() {

        while (currentState == PLAYER_STATE.ATTACK) {

            //ClearAnimParams();
            if(Input.GetMouseButtonDown(1)){

                m_Agent.transform.LookAt(m_EnemyPosition.transform.position);
                m_Anim.SetTrigger("Attacking");        
                yield return null;

            } else {

                m_Anim.SetTrigger("Idle");
                CurrentState = PLAYER_STATE.IDLE;

                yield break;
            }

        }

        yield return null;

    }




    public IEnumerator Player_Injured() {

        while (currentState == PLAYER_STATE.INJURED) {
            yield return null;
        }

        yield break;

    }

    public IEnumerator Player_Defend() {

        while (currentState == PLAYER_STATE.DEFEND) {

            if (Input.GetMouseButton(2)) {

                m_Agent.transform.LookAt(m_EnemyPosition);
                m_Anim.SetTrigger("Defending");
       
                yield return null;

            } else  {

                CurrentState = PLAYER_STATE.IDLE;

                yield break;
            }

        }

        yield return null;

    }

    public IEnumerator Player_Dead() {

        while (currentState == PLAYER_STATE.DEAD) {
            yield return null;
        }

        yield break;

    }

    //-------------End Player state machine

    public void ClearAnimParams() {

        foreach (AnimatorControllerParameter paramater in m_Anim.parameters) {
               
            m_Anim.ResetTrigger(paramater.name);

        }

    }

}

