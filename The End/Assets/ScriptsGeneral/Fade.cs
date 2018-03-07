using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fade : MonoBehaviour {

    public Texture2D fadeOutTex;
    public float fadeSpeed = 2.0f;

    private int drawDepth = -1000; // draw last to render on top
    private float alpha = 1.0f;
    private int fadeDirection = -1;       // fade in = -1, fade out = 1

    private void OnGUI() {

        alpha += fadeDirection * fadeSpeed * Time.deltaTime;
        // clamp alpha value
        alpha = Mathf.Clamp01(alpha);
        // set alpha
        GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, alpha);
        GUI.depth = drawDepth; //This sets the tex to draw last
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeOutTex);

    }

    public float BeginFade (int direction){

        fadeDirection = direction;
        return (fadeSpeed);

    }

    void OnLevelIsLoaded (){

        BeginFade(-1);

    }


}
