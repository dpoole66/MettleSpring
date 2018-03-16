using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour {

    public int delay = 3;
    public Text banner, broadcast;

    private void Awake() {

        Text banner = GetComponent<Text>();
        Text broadcast = GetComponent<Text>();

    }

    void Start () {

        StartCoroutine(StartPhase());

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator StartPhase()   {

        Debug.Log("StartPhase is now");
        banner.text = "Start Phase";
        broadcast.text = "Starting in 3 seconds";
        yield return new WaitForSeconds(3);
        StartCoroutine(IntroPhase());

    }

    IEnumerator IntroPhase() {

        while (true)  {

            banner.text = "IntroPhase";
            broadcast.text = "Tap to Run Game";

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended){

                Debug.Log("Touched and starting RunningPhase");
                banner.text = "Starting RunningPhase";
                broadcast.text = null;
                StartCoroutine(RunningPhase());
                break;
            }

            yield return null;
         }

    }

    IEnumerator RunningPhase() {

        while (true) {

            banner.text = "RunningPhase";
            broadcast.text = "Tap to End Game";

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {

                Debug.Log("EndingPhase");
                banner.text = "Switching to EndingPhase";
                broadcast.text = "Ending...";
                yield return new WaitForSeconds(3);
                StartCoroutine(EndingPhase());
                break;

            }  

            yield return null;

         }

    }

    IEnumerator EndingPhase() {

        while (true) {

            Debug.Log("EndingPhase");
            banner.text = "Ending in 3 Seconds";
            broadcast.text = "Restarting Game";
            yield return new WaitForSeconds(3);
            StartCoroutine(StartPhase());
            break;
         }

        yield return null;

    }



}
