using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToMove : MonoBehaviour {

    public float surfaceOffset = -0.1f;
    public GameObject MoveTarget;

    private void Update() {
        if (!Input.GetMouseButtonDown(0)) {
            return;
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit)) {
            return;
        }
        transform.position = hit.point + hit.normal * surfaceOffset;
        if ((MoveTarget != null) && (hit.transform.gameObject.tag == "Ground")) {
            MoveTarget.SendMessage("SetTarget", transform);
            //Debug.Log("GumBall's active again");
        }
    }
}
