﻿using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[Serializable]
public class KnightManager {

    public Color m_PlayerColor;                             // This is the color this tank will be tinted.
    public Transform m_SpawnPoint;                          // The position and direction the tank will have when it spawns.
    [HideInInspector] public int m_MettleNumber;            // This specifies which player this the manager for.
    [HideInInspector] public string m_ColoredPlayerText;    // A string that represents the player with their number colored to match their tank.
    [HideInInspector] public GameObject m_Instance;         // A reference to the instance of the tank when it is created.
    [HideInInspector] public int m_Wins;                    // The number of wins this player has so far.
    [HideInInspector] public List<Transform> m_WayPointList;

    //Controllers
    public GameObject m_GumBall;
    public GameObject m_DropBall;

    private MettleMovement m_Movement;                        // Reference to tank's movement script, used to disable and enable control.
    //private TankShooting m_Shooting;                        // Reference to tank's shooting script, used to disable and enable control.
    private GameObject m_CanvasGameObject;                  // Used to disable the world space UI during the Starting and Ending phases of each round.
    private StateController m_StateController;	              //Enemy Mettle AI;


    public void SetupAI(List<Transform> wayPointList) {
        m_StateController = m_Instance.GetComponent<StateController>();
        m_StateController.SetupAI(true, wayPointList);

        //m_Shooting = m_Instance.GetComponent<TankShooting>();
        //m_Shooting.m_MettleNumber = m_MettleNumber;

        m_CanvasGameObject = m_Instance.GetComponentInChildren<Canvas>().gameObject;
        m_ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + ">PLAYER " + m_MettleNumber + "</color>";

        // Get all of the renderers of the tank.
        MeshRenderer[] renderers = m_Instance.GetComponentsInChildren<MeshRenderer>();

        // Go through all the renderers...
        for (int i = 0; i < renderers.Length; i++) {
            // ... set their material color to the color specific to this tank.
            renderers[i].material.color = m_PlayerColor;
        }
    }

    public void SetupPlayerMettle() {
        // Get references to the components.

        m_Movement = m_Instance.GetComponent<MettleMovement>();
        //m_Shooting = m_Instance.GetComponent<TankShooting>();
        m_CanvasGameObject = m_Instance.GetComponentInChildren<Canvas>().gameObject;

        // Set the player numbers to be consistent across the scripts.
        m_Movement.m_MettleNumber = m_MettleNumber;
       // m_Shooting.m_MettleNumber = m_MettleNumber;

        // Create a string using the correct color that says 'PLAYER 1' etc based on the tank's color and the player's number.
        m_ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + ">PLAYER " + m_MettleNumber + "</color>";

        // Get all of the renderers of the tank.
        MeshRenderer[] renderers = m_Instance.GetComponentsInChildren<MeshRenderer>();

        // Go through all the renderers...
        for (int i = 0; i < renderers.Length; i++) {
            // ... set their material color to the color specific to this tank.
            renderers[i].material.color = m_PlayerColor;
        }
    }

    // Used during the phases of the game where the player shouldn't be able to control their tank.
    public void DisableControl() {
        if (m_Movement != null)
            m_Movement.enabled = false;

        if (m_StateController != null)
            m_StateController.enabled = false;

        //m_Shooting.enabled = false;

        m_CanvasGameObject.SetActive(false);
    }

    // Used during the phases of the game where the player should be able to control their tank.
    public void EnableControl() {
        if (m_Movement != null)
            m_Movement.enabled = true;

        if (m_StateController != null)
            m_StateController.enabled = true;

        //m_Shooting.enabled = true;

        m_CanvasGameObject.SetActive(true);
    }


    // Used at the start of each round to put the tank into it's default state.
    public void Reset() {
        m_Instance.transform.position = m_SpawnPoint.position;
        m_Instance.transform.rotation = m_SpawnPoint.rotation;

        m_Instance.SetActive(false);
        m_Instance.SetActive(true);
    }
}

