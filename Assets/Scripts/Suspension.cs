using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Assets.Scripts;
using Assets.Scripts.Suspension;
using static Assets.Scripts.CustomValues;

public class Suspension : MonoBehaviour
{
    private enum SuspensionType {
        Torsion,
        HVSS,
        VVSS,
        Hydraulic,
        Null
    }
    [Header("Suspension Data")]
    [SerializeField] private SuspensionType suspensionType = SuspensionType.Null;
    private Transform Hull;
    private Transform Tracks;
    private Transform[] RoadWheels;

    [Header("General Suspension Parameters/")]

    [Header("General Suspension Parameters/Torsion")]


    [Header("General Suspension Parameters/HVSS")]
    [SerializeField] private float HVSSSpringConstant = 8000f; // Example value, adjust as needed
    [SerializeField] private float HVSSDampingConstant = 80f; // Example value, adjust as needed

    [Header("General Suspension Parameters/VVSS")]
    [SerializeField] private float VVSSSpringConstant = 6000f; // Example value, adjust as needed
    [SerializeField] private float VVSSDampingConstant = 60f; // Example value, adjust as needed

    [Header("General Suspension Parameters/Hydraulic")]
    [SerializeField] private float HydraulicSpringConstant = 12000f; // Example value, adjust as needed
    [SerializeField] private float HydraulicDampingConstant = 120f; // Example value, adjust as needed

    private float correctionForce;
    private Vector3 prevTrackPos;
    private Vector3 currTrackPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (suspensionType == SuspensionType.Null) return;
        if (suspensionType == SuspensionType.Torsion)
        {
        }


        Hull = transform.parent;
        Tracks = Hull.Find("Suspension").Find("Tracks");


        if (Tracks != null)
        {
            int TrackChildCount = 0;
            for (int i = 0; i < Tracks.childCount; i++)
            {
                TrackChildCount += Tracks.GetChild(i).childCount;
            }

            RoadWheels = new Transform[TrackChildCount];

            for (int i = 0; i < Tracks.childCount; i++)
            {
                for (int j = 0; j < Tracks.Find($"{(Side)i}").childCount; j++)
                {
                    RoadWheels[i] = Tracks.Find($"{(Side)i}").GetChild(j);
                }
            }

            if (RoadWheels == null)
            {
                Debug.LogWarning("Suspension: No RoadWheels found under Sides. Please ensure the hierarchy is correct.");
            }   
        }
        else
        {
            Debug.LogWarning("Suspension: No Tracks found under parent. Please ensure the hierarchy is correct.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        prevTrackPos = currTrackPos;
        currTrackPos = Tracks.position;
        SuspensionContactSensor suspensionContactSensor = GetComponent<SuspensionContactSensor>();
        Vector3 trackVelocity = (currTrackPos - prevTrackPos) / Time.deltaTime;
        if (suspensionType == SuspensionType.Torsion)
        {       

            Hull.GetComponent<Rigidbody>().AddRelativeForce(Vector3.up * correctionForce, ForceMode.Force);
        }
        else if (suspensionType == SuspensionType.HVSS)
        {
            // Implement HVSS suspension logic here
        }
        else if (suspensionType == SuspensionType.VVSS)
        {
            // Implement VVSS suspension logic here
        }
        else if (suspensionType == SuspensionType.Hydraulic)
        {
            // Implement Hydraulic suspension logic here
        }

    }
}
