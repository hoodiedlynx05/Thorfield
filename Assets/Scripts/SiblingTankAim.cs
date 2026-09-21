// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity
using Unity.Mathematics;
using Unity.VisualScripting;

// Unity Engine
using UnityEngine;
using UnityEngine.InputSystem;

public class SiblingTankAim : MonoBehaviour
{
    [Header("Chassis Sibling")]
    [SerializeField] private Transform tankHull;
    [SerializeField] private Vector3 mountOffset;

    [Header("Cannon Data")]
    [SerializeField] private Transform gunBarrel;

    [Header("Input")]
    [SerializeField] private InputActionReference lookAction;

    [Header("Speeds")]
    [SerializeField] private float horizontalSpeed = 5f;
    [SerializeField] private float verticalSpeed = 5f;

    [Header("Limits")]
    [SerializeField] private float minElevation = -10f;
    [SerializeField] private float maxElevation = 45f;

    private float turretYRotation;
    private float barrelXRotation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        lookAction.action.Enable();

        turretYRotation = transform.localEulerAngles.y;
        if(gunBarrel != null)
        {
            barrelXRotation = gunBarrel.localEulerAngles.x;
        }
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        lookAction.action.Disable();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (tankHull == null) {
            Debug.LogWarning("Tank hull reference is missing. Please assign a tank hull Transform in the inspector.");
            return;
        }

        transform.position = tankHull.TransformPoint(mountOffset);

        Vector2 lookInput = lookAction != null ? lookAction.action.ReadValue<Vector2>() : Vector2.zero;

        turretYRotation += lookInput.x * horizontalSpeed * Time.deltaTime;
        transform.rotation = tankHull.rotation * Quaternion.Euler(0f, turretYRotation, 0f);

        if (gunBarrel != null)
        {
            barrelXRotation += lookInput.y * verticalSpeed * Time.deltaTime;

            if(barrelXRotation > 180f) barrelXRotation -= 360f;
            if(barrelXRotation < -180f) barrelXRotation += 360f;
            barrelXRotation = Mathf.Clamp(barrelXRotation, minElevation, maxElevation);

            var currentBarrelRotation = Quaternion.Euler(barrelXRotation, 90f, 0f);
            gunBarrel.rotation = transform.rotation * currentBarrelRotation;
        }
    }
}
