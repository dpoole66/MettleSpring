using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class StartButton : MonoBehaviour {

    public Text t_GameStateUI;

public void StartLoadAR(){

        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("ARstage");

        while (!asyncLoad.isDone) {

            t_GameStateUI.text = "Loading Stage";

            return;

         }

    }    
}
