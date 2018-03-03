using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

   public enum GAME_STATE {IDLE, STARTING, SETTING, SPAWNING, PLAYING, ENDING };

   public GAME_STATE CurrentState{

        get{ return currentstate; }

        set{
                currentstate = value;

                StopAllCoroutines();

                switch(currentstate){

                    case GAME_STATE.IDLE:
                        StartCoroutine(GameIdle());
                        break;

                    case GAME_STATE.STARTING:
                        StartCoroutine(GameStart());
                        break;

                    case GAME_STATE.SETTING:
                        StartCoroutine(GameSetting());
                        break;

                    case GAME_STATE.SPAWNING:
                        StartCoroutine(GameSpawning());
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

   // UI Components     
    public GameObject m_StartButton;
    public GameObject m_GameStateUI;
    private Text m_gameStateText;
    private Fade m_Fade;


    //Placement Components
    public GameObject m_StageInstance;
    public float surfaceOffset = 0.1f;

    //Spawning Components
    private Transform m_PlayerSpawn, m_EnemySpawn;
    public GameObject m_PlayerPrefab, m_EnemyPrefab;

    [SerializeField]         
    GAME_STATE currentstate = GAME_STATE.STARTING;


    // Methods
    public void Start() {

        m_gameStateText = m_GameStateUI.GetComponent<Text>();
        m_Fade = GameObject.Find("FadeBall").GetComponent<Fade>();
        
        CurrentState = GAME_STATE.IDLE;         

    }

    public void StartGame(){
       
        CurrentState = GAME_STATE.STARTING;  
     
    }


/// <summary>
///   COROUTINES
/// </summary>

    IEnumerator GameIdle() {
        //CurrentState = GAME_STATE.IDLE;
        Debug.Log(CurrentState);
        m_gameStateText.text = "Press to Start";

        while (currentstate == GAME_STATE.IDLE) {

            m_StartButton.SetActive(true);
                                                                                                                                
            yield return null;

         }

    }

    IEnumerator GameStart(){
        
        Debug.Log(CurrentState);
        m_gameStateText.text = "Starting";
        Input.ResetInputAxes();

        while (currentstate == GAME_STATE.STARTING){

           // remove button
            m_StartButton.SetActive(false);

           // call loading coroutine
            yield return LoadGameScene();

         }

         yield break;

    }

    IEnumerator GameSetting() {
       
        Debug.Log(CurrentState);
        m_gameStateText.text = "Setting";
       // Input.ResetInputAxes();

        while (currentstate == GAME_STATE.SETTING) {

            // SetRing sets the play space 
            yield return StartCoroutine(SetRing());

         }

        yield break;

    }

    IEnumerator GameSpawning(){
        Debug.Log(CurrentState);
        m_gameStateText.text = "Spawning";

        while (currentstate == GAME_STATE.SPAWNING){
            Debug.Log("Starting SpawnMettles()");
            yield return new WaitForSeconds(2);
            // Spawn Mettle on the prefab     Asyncronous fire and forget because SpawnMettles() will start GamePlaying()
            StartCoroutine(SpawnMettles());
            yield break;
         } 

        yield break;    
    }

    IEnumerator GamePlaying(){

        Debug.Log(CurrentState);
        m_gameStateText.text = "Game Playing";

        while (currentstate == GAME_STATE.PLAYING) {

            if (Input.GetMouseButtonDown(1)) {
  
                CurrentState = GAME_STATE.ENDING;
                yield break;

            }

            yield return null;

         }

        yield break;         
    }


    IEnumerator GameEnding(){

        Debug.Log(CurrentState);
        m_gameStateText.text = "Play Again?";

        while (currentstate == GAME_STATE.ENDING) {

            m_StartButton.SetActive(true);
 
            yield return null;

         }

        yield break;           
    }

    //Coroutine fade
    IEnumerator Fade(){

        float fadeTime = m_Fade.BeginFade(1);
        yield return new WaitForSeconds(fadeTime);
        Debug.Log("Am I Fadeing?");
        yield break;

    }


    // Coroutine to load Game Scene
    IEnumerator LoadGameScene(){

        m_gameStateText.text = "Loading Scene";
        Debug.Log("Loading Scene");

        yield return StartCoroutine(Fade());

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("two");

        while (!asyncLoad.isDone) {

            yield return null;

         }

        Debug.Log("I'm switching to SETTING");
        CurrentState = GAME_STATE.SETTING;
        yield break;  
    }



    // Coroutine to Set  Battle Area   
    IEnumerator SetRing() {

        Debug.Log("SetRing");

            while (Input.GetMouseButtonDown(0)) {
                Debug.Log("Nothing happening in Set Ring");     

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit)) {
                    Debug.Log("Raycast");
          
                    transform.position = hit.point + hit.normal * surfaceOffset;

                        if ((m_StageInstance != null) && (hit.transform.gameObject.tag == "Ground")) {
                            Debug.Log("Hit");

                            var m_stageInstance = Instantiate(m_StageInstance, hit.point, Quaternion.identity);
                            Debug.Log("I'm switching to SPAWNING");
                            CurrentState = GAME_STATE.SPAWNING;
                            yield break;

                        }         
                }

                    Debug.Log("I'm sitting at the base of the while statement returning null");
                    yield return null;

             }

        yield break;

    }    
    
    IEnumerator SpawnMettles(){

        m_PlayerSpawn = GameObject.FindGameObjectWithTag("playerSpawn").transform;
        m_EnemySpawn = GameObject.FindGameObjectWithTag("enemySpawn").transform;

        while (m_PlayerPrefab && m_EnemyPrefab != null && m_EnemySpawn && m_PlayerSpawn != null)   {

            var m_playerSpawn = Instantiate(m_PlayerPrefab, m_PlayerSpawn.transform.position, Quaternion.identity);
            var m_enemySpawn = Instantiate(m_EnemyPrefab, m_EnemySpawn.transform.position, Quaternion.identity);
            Debug.Log("I'm switching to PLAYING");
            CurrentState = GAME_STATE.PLAYING;
            yield break;
         } 

        yield break;                     
    } 
}


