using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerAR : MonoBehaviour {

    public int m_NumRoundsToWin = 3;            // The number of rounds a single player has to win to win the game.
    public float m_StartDelay = 5f;             // The delay between the start of RoundStarting and RoundPlaying phases.
    public float m_EndDelay = 3f;               // The delay between the end of RoundPlaying and RoundEnding phases.
    public Text m_messageText;
    //public Image m_Icon;


    public GameObject[] m_MettlePrefabs;
    public MettleManager[] m_Mettles;               // A collection of managers for enabling and disabling different aspects of the tanks.
    public List<Transform> wayPointsForAI;

    // UI, Placement and Movement components
    public GameObject m_PlaceRing, m_GumBall, m_StageInstance;
    public float surfaceOffset = 0.1f;

    //AR Camera
    public Camera ARCamera;                          

    private int m_RoundNumber;               
    private WaitForSeconds m_StartWait;         
    private WaitForSeconds m_EndWait;        
    private MettleManager m_RoundWinner;      
    private MettleManager m_GameWinner;

    public bool Starting = false;
    public bool StagePlaced = false;  

    [SerializeField]
    GAME_STATE currentstate = GAME_STATE.PLACE;

    public enum GAME_STATE {PLACE, STARTING, PLAYING, ENDING };

    public GAME_STATE CurrentState {

        get { return currentstate; }

        set {
            currentstate = value;

            StopAllCoroutines();

            switch (currentstate) {

                case GAME_STATE.PLACE:
                    StartCoroutine(GamePlace());
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

        Camera ARCamera = GetComponent<Camera>();
        Text m_messageText = GetComponent<Text>();
        GameObject m_PlaceRing = GetComponent<GameObject>();
        GameObject m_GumBall = GetComponent<GameObject>();

        Debug.Log("at Start");

        new WaitForSeconds(5);
        CurrentState = GAME_STATE.PLACE;

    }


    /// 
    ///GAME LOOP
    ///
   
    // GAME PLACE
    private IEnumerator GamePlace(){

        Debug.Log("GamePlacing");
        m_messageText.text = "Tap to Place Tiger";
        m_PlaceRing.SetActive(true);

        if (StagePlaced == true) {

            m_PlaceRing.SetActive(false);
            Debug.Log("Spawning");
            SpawnAllMettles();
            CurrentState = GAME_STATE.STARTING;
            yield break;

        }

    }

    // ROUIND STARTING
    private IEnumerator RoundStarting() {

        while (currentstate == GAME_STATE.STARTING) {
            
            m_messageText.text = "Round Starting";
            Debug.Log("top of RoundStarting");
   
            ResetAllMettles();
            DisableMettleControl();

            // Wait for the specified length of time until yielding control back to the STATE MACHINE.
            yield return m_EndWait;

            // Increment the round number and display text showing the players what round it is.
            m_RoundNumber++;
            m_messageText.text = "ROUND " + m_RoundNumber;

            Debug.Log("Activating GumBall, Setting Round");
            m_GumBall.SetActive(true);

            CurrentState = GAME_STATE.PLAYING;

         }

    }

     // ROUND PLAYING
    private IEnumerator RoundPlaying() {

        while (currentstate == GAME_STATE.PLAYING) {
            // As soon as the round begins playing let the players control the tanks.
            EnableMettleControl();

            // Clear the text from the screen.
            m_messageText.text = "ROUND " + m_RoundNumber;

            // While there is not one tank left...
            while (!OneMettleLeft()) {
                // ... return on the next frame.
                yield return null;
             }

            CurrentState = GAME_STATE.ENDING;

        }
    }

     // ROUND ENDING
    private IEnumerator RoundEnding() {

        while (currentstate == GAME_STATE.ENDING) {
            // Stop Mettles from moving.
            DisableMettleControl();

            // Clear the winner from the previous round.
            m_RoundWinner = null;

            // See if there is a winner now the round is over.
            m_RoundWinner = GetRoundWinner();

            // If there is a winner, increment their score.
            if (m_RoundWinner != null)
                m_RoundWinner.m_Wins++;

            // Get a message based on the scores and whether or not there is a game winner and display it.
            string message = EndMessage();
            m_messageText.text = message;

            // Now the winner's score has been incremented, see if someone has wne the game.
            m_GameWinner = GetGameWinner();

            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_EndWait;

            if (m_GameWinner != null) {

                CurrentState = GAME_STATE.PLACE;

            } else  CurrentState = GAME_STATE.STARTING;

        }
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
        m_Mettles[0].m_MettleNumber = 1;
        m_Mettles[0].SetupPlayerMettle();

        // Setup the AI Mettle
        for (int i = 1; i < m_Mettles.Length; i++) {
            // ... create them, set their player number and references needed for control.
            m_Mettles[i].m_MettleInstance =
                Instantiate(m_MettlePrefabs[i], m_Mettles[i].m_SpawnPoint.transform.position,
                m_Mettles[i].m_SpawnPoint.transform.rotation) as GameObject;
            m_Mettles[i].m_MettleNumber = i + 1;
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

        // If no mettles have enough rounds to win, return null.
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

        // Go through all the Mettles and add each of their scores to the message.
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
