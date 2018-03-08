using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerAR : MonoBehaviour {

    public int m_NumRoundsToWin = 5;            // The number of rounds a single player has to win to win the game.
    public float m_StartDelay = 3f;             // The delay between the start of RoundStarting and RoundPlaying phases.
    public float m_EndDelay = 5f;               // The delay between the end of RoundPlaying and RoundEnding phases.
    public GameObject m_MessageTextUI, m_Icon;                  // Reference to the overlay Text to display winning text, etc.
    public Text m_messageText;
    public GameObject[] m_MettlePrefabs;
    public MettleManager[] m_Mettles;               // A collection of managers for enabling and disabling different aspects of the tanks.
    public List<Transform> wayPointsForAI;

    // UI, Placement and Movement components
    public GameObject m_PlaceRing, m_GumBall, m_StartButton, m_StageInstance;
    public float surfaceOffset = 0.1f;


    private int m_RoundNumber;               
    private WaitForSeconds m_StartWait;         
    private WaitForSeconds m_EndWait;        
    private MettleManager m_RoundWinner;      
    private MettleManager m_GameWinner;
    public bool Starting = false;
    public bool StagePlaced = false;

    [SerializeField]
    GAME_STATE currentstate = GAME_STATE.IDLE;

    public enum GAME_STATE { IDLE, SETUP, STARTING, PLAYING, ENDING };

    public GAME_STATE CurrentState {

        get { return currentstate; }

        set {
            currentstate = value;

            StopAllCoroutines();

            switch (currentstate) {

                case GAME_STATE.IDLE:
                    StartCoroutine(GameIdle());
                    break;

                case GAME_STATE.SETUP:
                    StartCoroutine(GameSetup());
                    break;

                case GAME_STATE.STARTING:
                    StartCoroutine(RoundStarting());
                    break;

                case GAME_STATE.PLAYING:
                    StartCoroutine(RoundPlaying());
                    break;

                case GAME_STATE.ENDING:
                    StartCoroutine(RoundEnding());
                    break;

            }
        }
    }


    //Start methods

    public void Start() {
        // Create the delays so they only have to be made once.
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);

        m_messageText = m_MessageTextUI.GetComponent<Text>();
        Button m_startButton = m_StartButton.GetComponent<Button>();
        //m_startButton.onClick.AddListener(StartInit);

        m_StartButton.SetActive(true);
        m_Icon.SetActive(true);
 
        m_messageText.text = "Press Start to begin";

        CurrentState = GAME_STATE.IDLE;

    }



    private IEnumerator PlaceTiger() {

        yield return m_StartWait;
        Debug.Log("PlaceTiger() running");
        m_messageText.text = "Tap to place Tiger";

        while (true) {
            if (!Input.GetMouseButtonDown(0)) {

                yield return null;

            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (!Physics.Raycast(ray, out hit)) {

                yield return null;
            }

            transform.position = hit.point + hit.normal * surfaceOffset;

            if ((m_StageInstance == null) && (hit.transform.gameObject.tag == "Ground")) {

                var m_stageInstance = Instantiate(m_StageInstance, hit.point, Quaternion.identity);

                yield return null;

                if (m_stageInstance != null) {

                    yield break;

                }

            }           
            Debug.Log("At the Bottom of PlaceTiger()");
            StartCoroutine(RoundStarting());
         }
    }

    // Start Button in IDLE
    public void StartButton() {

        // This is for the UI start button. It will start the SETUP state.
        CurrentState = GAME_STATE.SETUP ;
        //m_messageText.text = "Hold On";
    }

    public void KillStartButton() {

        m_StartButton.gameObject.SetActive(false);

    }

    /// 
    ///GAME LOOP
    ///
    /// RESTING STATE AT START
    private IEnumerator GameIdle(){

        while (currentstate == GAME_STATE.IDLE) {


            Debug.Log("Game Idle");
            m_PlaceRing.SetActive(false);
            m_GumBall.SetActive(false);
            m_StartButton.gameObject.SetActive(true);
            m_Icon.SetActive(true);

            yield return null;

         }

        yield break;

    }

  // INITIAL GAME LOADING
    private IEnumerator GameSetup() {

        while (currentstate == GAME_STATE.SETUP) {

            m_messageText.text = "Tap to Place Tiger";
            Debug.Log("Game Setting Up");
            m_StartButton.gameObject.SetActive(false);
            m_Icon.SetActive(false);
            m_PlaceRing.SetActive(true);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Stage002");

            while (!asyncLoad.isDone) {

                yield return null;

             }

            if (StagePlaced  == true) {

                m_PlaceRing.SetActive(false);
                m_messageText.text = "All Mettles Spawning";
                yield return new WaitForSeconds(3);   
                Debug.Log("Stage is Placed and now switching to STARTING");
                CurrentState = GAME_STATE.STARTING;
                yield break;

            }

            yield return null;

         }
    }

    // ROUIND STARTING
    private IEnumerator RoundStarting() {

        while (currentstate == GAME_STATE.STARTING) {
            
            m_messageText.text = "Round Starting";
            Debug.Log("top of RoundStarting");

            // As soon as the round starts reset the Mettles and make sure they can't move.
            if (m_StageInstance != null) {

                Debug.Log("I found the stage and now I'm spawning");
                SpawnAllMettles();
                ResetAllMettles();
                DisableMettleControl();
                yield return new WaitForSeconds(3);      
                
            }

            // Wait for the specified length of time until yielding control back to the STATE MACHINE.
            yield return m_EndWait;

            Debug.Log("Activating GumBall, Setting Round");
            m_GumBall.SetActive(true);

            // Increment the round number and display text showing the players what round it is.
            m_RoundNumber++;
            m_messageText.text = "ROUND " + m_RoundNumber;
    
            CurrentState = GAME_STATE.PLAYING;

         }

    }


    private IEnumerator RoundPlaying() {

        while (currentstate == GAME_STATE.PLAYING) {
            // As soon as the round begins playing let the players control the tanks.
            EnableMettleControl();

            // Clear the text from the screen.
            m_messageText.text = string.Empty;
    
            // While there is not one tank left...
            while (!OneMettleLeft()) {
                // ... return on the next frame.
                yield return null;
            }

        }
    }


    private IEnumerator RoundEnding() {
        // Stop tanks from moving.
        DisableMettleControl();

        // Clear the winner from the previous round.
        m_RoundWinner = null;

        // See if there is a winner now the round is over.
        m_RoundWinner = GetRoundWinner();

        // If there is a winner, increment their score.
        if (m_RoundWinner != null)
            m_RoundWinner.m_Wins++;

        // Now the winner's score has been incremented, see if someone has one the game.
        m_GameWinner = GetGameWinner();

        // Get a message based on the scores and whether or not there is a game winner and display it.
        string message = EndMessage();
        m_messageText.text = message;

        // Wait for the specified length of time until yielding control back to the game loop.
        yield return m_EndWait;
    }

    //Methods  within loop

    void StartInit() {

        Starting = true;

    }

    public void SetGameRing(bool stagePlaced) {

        this.StagePlaced = stagePlaced;

    }


    //Spawn Mettles           
    private void SpawnAllMettles() {
        //Manually setup the player at index zero in the tanks array
        m_Mettles[0].m_MettleInstance =
            Instantiate(m_MettlePrefabs[0], m_Mettles[0].m_SpawnPoint.transform.position,
            m_Mettles[0].m_SpawnPoint.transform.rotation) as GameObject;
        m_Mettles[0].m_PlayerNumber = 1;
        m_Mettles[0].SetupPlayerMettle();

        // Setup the AI Mettle
        for (int i = 1; i < m_Mettles.Length; i++) {
            // ... create them, set their player number and references needed for control.
            m_Mettles[i].m_MettleInstance =
                Instantiate(m_MettlePrefabs[i], m_Mettles[i].m_SpawnPoint.transform.position,
                m_Mettles[i].m_SpawnPoint.transform.rotation) as GameObject;
            m_Mettles[i].m_PlayerNumber = i + 1;
            m_Mettles[i].SetupAI(wayPointsForAI);
        }
    }


    // This is used to check if there is one or fewer tanks remaining and thus the round should end.
    private bool OneMettleLeft() {
        // Start the count of tanks left at zero.
        int numMettleLeft = 0;

        // Go through all the tanks...
        for (int i = 0; i < m_Mettles.Length; i++) {
            // ... and if they are active, increment the counter.
            if (m_Mettles[i].m_MettleInstance.activeSelf)
                numMettleLeft++;
        }

        // If there are one or fewer tanks remaining return true, otherwise return false.
        return numMettleLeft <= 1;
    }


    // This function is to find out if there is a winner of the round.
    // This function is called with the assumption that 1 or fewer tanks are currently active.
    private MettleManager GetRoundWinner() {
        // Go through all the Mettles...
        for (int i = 0; i < m_Mettles.Length; i++) {
            // ... and if one of them is active, it is the winner so return it.
            if (m_Mettles[i].m_MettleInstance.activeSelf)
                return m_Mettles[i];
        }

        // If none of the tanks are active it is a draw so return null.
        return null;
    }


    // This function is to find out if there is a winner of the game.
    private MettleManager GetGameWinner() {
        // Go through all the Mettles...
        for (int i = 0; i < m_Mettles.Length; i++) {
            // ... and if one of them has enough rounds to win the game, return it.
            if (m_Mettles[i].m_Wins == m_NumRoundsToWin)
                return m_Mettles[i];
        }

        // If no tanks have enough rounds to win, return null.
        return null;
    }


    // Returns a string message to display at the end of each round.
    private string EndMessage() {
        // By default when a round ends there are no winners so the default end message is a draw.
        string message = "DRAW!";

        // If there is a winner then change the message to reflect that.
        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

        // Add some line breaks after the initial message.
        message += "\n\n\n\n";

        // Go through all the tanks and add each of their scores to the message.
        for (int i = 0; i < m_Mettles.Length; i++) {
            message += m_Mettles[i].m_ColoredPlayerText + ": " + m_Mettles[i].m_Wins + " WINS\n";
        }

        // If there is a game winner, change the entire message to reflect that.
        if (m_GameWinner != null)
            message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

        return message;
    }


    // This function is used to turn all the tanks back on and reset their positions and properties.
    private void ResetAllMettles() {
        for (int i = 0; i < m_Mettles.Length; i++) {
            m_Mettles[i].Reset();
        }
    }


    private void EnableMettleControl() {
        for (int i = 0; i < m_Mettles.Length; i++) {
            m_Mettles[i].EnableControl();
        }
    }


    private void DisableMettleControl() {
        for (int i = 0; i < m_Mettles.Length; i++) {
            m_Mettles[i].DisableControl();
        }
    }

    void ClickTask() {

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Stage001");

        while (!asyncLoad.isDone) {
            new WaitForSeconds(5);
        }
    }
}
