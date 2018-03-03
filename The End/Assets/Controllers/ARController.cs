using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARController : MonoBehaviour {


   public IEnumerator EscapeGame() {
        while (Input.GetMouseButtonDown(1)) {

            Application.Quit();
            yield break;

         }

        StartCoroutine(QuitOnConnectionErrors());

        yield break;

    }

    private IEnumerator QuitOnConnectionErrors() {

            yield break;
    }

    public IEnumerator FeedBack(){

        Debug.Log("I'm providing Feedback");

        yield return null;

    }



    // Update is called once per frame
    void Update () {

        if (Input.GetKey(KeyCode.Escape)) {
            Application.Quit();
        }

    }
}
