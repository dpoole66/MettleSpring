using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameControllerBasic : MonoBehaviour {

   public enum GAME_STATE { STARTING, SETTING, PLAYING, ENDING };

   public GAME_STATE CurrentState{

        get{ return currentstate; }

        set{
                currentstate = value;

                StopAllCoroutines();

                switch(currentstate){

                    case GAME_STATE.STARTING:
                        StartCoroutine(GameStart());
                        break;

                    case GAME_STATE.SETTING:
                        StartCoroutine(GameSetting());
                        break;

                    case GAME_STATE.PLAYING:
                        StartCoroutine(GamePlaying());
                        break;

                    case GAME_STATE.ENDING:
                        StartCoroutine(GameEnding());
                        break;

                }
        }


   }

   // Components     
    public GameObject m_StartButton;

    [SerializeField]         
    GAME_STATE currentstate = GAME_STATE.STARTING;


  // Methods
    public void Start() {

        CurrentState = GAME_STATE.STARTING;         

    }

    public void SetGame(){

        CurrentState = GAME_STATE.SETTING;

    }


    IEnumerator GameStart(){

        Debug.Log(CurrentState);

        while (currentstate == GAME_STATE.STARTING){

            m_StartButton.SetActive(true);

            yield return null;

        }

    }

    IEnumerator GameSetting(){

        Debug.Log(CurrentState);

        m_StartButton.SetActive(false);

        while (currentstate == GAME_STATE.SETTING) {

 
            if (Input.GetMouseButtonDown(0)) {

                Input.ResetInputAxes();
                CurrentState = GAME_STATE.PLAYING;
                     
                yield break;

            }

            yield return null;

        }

        yield break;

    }

    IEnumerator GamePlaying(){
;
        Debug.Log(CurrentState);

        while (currentstate == GAME_STATE.PLAYING) {

            if (Input.GetMouseButtonDown(0)) {

                CurrentState = GAME_STATE.ENDING;
                yield break;

            }

            yield return null;

        }

        yield break;

    }


    IEnumerator GameEnding(){

        Debug.Log(CurrentState);

        while (currentstate == GAME_STATE.ENDING) {

            m_StartButton.SetActive(true);

  
            yield return null;

        }

        yield break;

    }
}
