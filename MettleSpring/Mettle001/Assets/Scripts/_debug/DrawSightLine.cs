using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawSightLine : MonoBehaviour {
    
    public Transform MettleEyeForward;

	void OnDrawGizmos () {
        Gizmos.color = Color.blue;
        //Gizmos.DrawLine(transform.position, MettleEyeForward.position);	
	}
}
