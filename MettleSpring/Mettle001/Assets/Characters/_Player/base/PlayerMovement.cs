using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public float surfaceOffset = 0.1f;
    public GameObject m_PlayerPrefab;

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
        if ((m_PlayerPrefab != null) && (hit.transform.gameObject.tag == "Ground")) {

            m_PlayerPrefab.SendMessage("SetTarget", transform);

        }
    }
}
