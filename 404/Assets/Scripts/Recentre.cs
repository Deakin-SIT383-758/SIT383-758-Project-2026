using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class Recentre : MonoBehaviour
{
    public Transform targetTransform; // position to recentre HMD to
    public Transform hmd; // hmd transform
    public Transform xrOrigin; // XR origin transform

    public InputActionReference rightStickClick;
    public InputActionReference leftStickClick;

    private float cooldown = 0.0f; // limit recentring frequency

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        cooldown -= Time.deltaTime;
        if (rightStickClick.action.ReadValue<float>() > 0.0f && leftStickClick.action.ReadValue<float>() > 0.0f
            && cooldown <= 0.0f)
        {
            Debug.Log("STICKS PRESSED - RECENTRING");
            xrOrigin.position += targetTransform.position - hmd.position;
            cooldown = 1.0f;
        }
    }
}
