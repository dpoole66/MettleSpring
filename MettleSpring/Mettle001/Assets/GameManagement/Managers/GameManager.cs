using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public int m_NumRoundsToWin = 5;            // The number of rounds a single player has to win to win the game.
    public float m_StartDelay = 3f;             // The delay between the start of RoundStarting and RoundPlaying phases.
    public float m_EndDelay = 3f;               // The delay between the end of RoundPlaying and RoundEnding phases.
    //public CameraControl m_CameraControl;       // Reference to the CameraControl script for control during different phases.
    public GameObject m_MessageText;                  // Reference to the overlay Text to display winning text, etc.
    public Text m_messageText;
    public GameObject[] m_MettlePrefabs;
    public MettleManager[] m_Mettles;               // A collection of managers for enabling and disabling different aspects of the tanks.
    public List<Transform> wayPointsForAI;

    // UI, Placement and Movement components
    public GameObject m_GumBall, m_StartButton, m_StageInstance;
    //[HideInInspector] public GameObject m_StageInstance;
    public float surfaceOffset = 0.1f;


    private int m_RoundNumber;                  // Which round the game is currently on.
    private WaitForSeconds m_StartWait;         // Used to have a delay whilst the round starts.
    private WaitForSeconds m_EndWait;           // Used to have a delay whilst the round or game ends.
    private MettleManager m_RoundWinner;          // Reference to the winner of the current round.  Used to make an announcement of who won.
    private MettleManager m_GameWinner;
    private bool Starting = false;    
    public bool RoundPlaced ;



    //Start methods

    public void Start() {
        // Create the delays so they only have to be made once.
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);

        m_messageText = m_MessageText.GetComponent<Text>();
        Button m_startButton = m_StartButton.GetComponent<Button>();
        m_startButton.onClick.AddListener(StartInit);

        m_StartButton.SetActive(true);

        m_messageText.text = "Press Start to begin";

        RoundPlaced = false;

        //StartCoroutine(GameLoop());

    }

    void StartInit(){

       Starting = true;

    }

    public void SetGameRing(bool roundPlaced){

        this.RoundPlaced = roundPlaced;
        StartCoroutine(GameLoop());

    }


    public void StartButton(){

        // This is for the UI start button. It will start the first coroutine RoundSetup which will start the GameLoop.
        StartCoroutine(GameStart());
        m_messageText.text = "Hold On";

    }

    public void KillStartButton(){

        m_StartButton.gameObject.SetActive(false);

    }


    //Spawn Mettles

    private void SpawnAllMettles() {
        //Manually setup the player at index zero in the tanks array
        m_Mettles[0].m_MettleInstance =
            Instantiate(m_MettlePrefabs[0], m_Mettles[0].m_SpawnPoint.transform.position, 
            m_Mettles[0].m_SpawnPoint.transform.rotation) as GameObject;
        m_Mettles[0].m_PlayerNumber = 1;
        m_Mettles[0].SetupPlayerMettle();

        // Setup the AI tanks
        for (int i = 1; i < m_Mettles.Length; i++) {
            // ... create them, set their player number and references needed for control.
            m_Mettles[i].m_MettleInstance =
                Instantiate(m_MettlePrefabs[i], m_Mettles[i].m_SpawnPoint.transform.position, 
                m_Mettles[i].m_SpawnPoint.transform.rotation) as GameObject;
            m_Mettles[i].m_PlayerNumber = i + 1;
            m_Mettles[i].SetupAI(wayPointsForAI);
        }
    }

    //Game Loop

    // This is called from start and will run each phase of the game one after another.
    private IEnumerator GameLoop() {                                                  

        // After Setup continue by running the 'RoundStarting' coroutine but don't return until it's finished.
        yield return StartCoroutine(RoundStarting());  

        // Once the 'RoundStarting' coroutine is finished, run the 'RoundPlaying' coroutine but don't return until it's finished.
        yield return StartCoroutine(RoundPlaying());

        // Once execution has returned here, run the 'RoundEnding' coroutine, again don't return until it's finished.
        yield return StartCoroutine(RoundEnding());

        // This code is not run until 'RoundEnding' has finished.  At which point, check if a game winner has been found.
        if (m_GameWinner != null) {
            // If there is a game winner, restart the level.
            SceneManager.LoadScene(0);
        } else {
            // If there isn't a winner yet, restart this coroutine so the loop continues.
            // Note that this coroutine doesn't yield.  This means that the current version of the GameLoop will end.
            StartCoroutine(GameLoop());
        }
    }


    private IEnumerator GameStart(){

        m_messageText.text = "Game Starting";
        Debug.Log("Game Starting");
     
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Stage002");
      
          while  (!asyncLoad.isDone){

                yield return null;

           }

        m_StartButton.SetActive(true);

          if (Starting){

                m_StartButton.SetActive(false);
                Debug.Log("Button Press");
   
          }

        StartCoroutine(RoundSetup());

    }

    private IEnumerator RoundSetup() {
  
        yield return m_StartWait;
        Debug.Log("Round Setup");
        m_messageText.text = "Tap to place Tiger";

        m_StageInstance = GameObject.Find("BattleArea(Clone)");
  
        //yield return null;

    }


        private IEnumerator RoundStarting() {

        // As soon as the round starts reset the Mettles and make sure they can't move.
        SpawnAllMettles();
        ResetAllMettles();
        DisableMettleControl();
   
        // Increment the round number and display text showing the players what round it is.
        m_RoundNumber++;
        m_messageText.text = "ROUND " + m_RoundNumber;

        // Wait for the specified length of time until yielding control back to the game loop.
        yield return m_StartWait;

    }


    private IEnumerator RoundPlaying() {
        // As soon as the round begins playing let the players control the tanks.
        EnableMettleControl();

        // Give control GumBall and deactivate PlayBall

        //m_PlayBall.SetActive(false);
        m_GumBall.SetActive(true);

        // Clear the text from the screen.
        m_messageText.text = string.Empty;

        // While there is not one tank left...
        while (!OneMettleLeft()) {
            // ... return on the next frame.
            yield return null;
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




