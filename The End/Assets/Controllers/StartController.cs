using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartController : MonoBehaviour {

    // UI Components     
    public GameObject m_StartButton;
    public GameObject m_GameStateUI;
    private Text m_gameStateText;
    private Fade m_Fade;

    // Use this for initialization
    void Start () {

        m_StartButton.SetActive(true);
        m_gameStateText = m_GameStateUI.GetComponent<Text>();
        m_Fade = GetComponent<Fade>();

    }
	
	// Update is called once per frame
	void Update () {

        m_gameStateText.text = "Press Start to begin";

    }
}
