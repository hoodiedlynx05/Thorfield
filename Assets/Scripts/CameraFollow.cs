using UnityEngine;  
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.VisualScripting;

public class CameraFollow: MonoBehaviour
{
    Camera m_Camera;

    float vfov;
    float aspect;
    float hfov;
    Rigidbody target;
    Turret turret;
    void Start()
    {
        m_Camera = Camera.main;
        if (m_Camera == null)
        {
            Debug.LogError("CameraFollow: No Camera.main found. Disabling script.");
            enabled = false;
            return;
        }

        vfov = m_Camera.fieldOfView;
        aspect = m_Camera.aspect;
        hfov = Camera.VerticalToHorizontalFieldOfView(vfov, aspect);

        // Auto-find Rigidbody target if not assigned in Inspector
        if (target == null)
        {
            target = GetComponent<Rigidbody>();
            if (target == null) target = GetComponentInParent<Rigidbody>();
            if (target == null) target = GetComponentInChildren<Rigidbody>();
        }

        if (target == null)
        {
            Turret sceneTurret = FindAnyObjectByType<Turret>();
            if (sceneTurret != null)
            {
                turret = sceneTurret;
                target = turret.GetComponent<Rigidbody>();
            }
        }

        if (turret == null && target != null)
        {
            turret = target.GetComponent<Turret>();
        }

        if (turret == null)
        {
            turret = FindAnyObjectByType<Turret>();
            if (turret == null) Debug.LogWarning("CameraFollow: No Turret found automatically. Assign target or Turret in the Inspector if automatic lookup is unsuitable.");
        }
    }


    public float mouseSensitivity = 0.1f;
    // smoothing time in seconds for SmoothDampAngle (smaller = snappier)
    public float smoothTime = 0.05f;
    // faster smoothing while input is active
    public float activeSmoothTime = 0.01f;
    // slower smoothing while idle
    public float idleSmoothTime = 0.05f;
    // minimum mouse delta magnitude (pixels) to consider as active input
    public float inputDeadzone = 0.5f;
    private float yawSmoothVelocity;

    void Update()
    {
        // ensure turret and target are available at runtime; try to recover if needed
        if (target == null)
        {
            target = GetComponent<Rigidbody>();
            if (target == null) target = GetComponentInParent<Rigidbody>();
            if (target == null) target = GetComponentInChildren<Rigidbody>();
        }

        if (turret == null && target != null)
        {
            turret = target.GetComponent<Turret>();
        }

        if (turret == null)
        {
            turret = FindAnyObjectByType<Turret>();
        }

        if (turret == null)
        {
            // nothing to follow yet
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }


        if (!Cursor.visible && Mouse.current != null)
        {
            // Use raw mouse delta for reliable input when cursor is locked
            Vector2 delta = Mouse.current.delta.ReadValue();
            // apply sensitivity
            float yawDelta = delta.x * mouseSensitivity;

            // update turret yaw angle (target)
            turret.YawAngle += yawDelta;

            Transform yawTransform = turret.GetYawTransform();
            // get current applied yaw as signed angle
            float currentYaw = (yawTransform.localEulerAngles.y + 180f) % 360f - 180f;
            // choose smoothing time depending on whether input is active
            float useSmooth = delta.magnitude > inputDeadzone ? activeSmoothTime : idleSmoothTime;
            // smooth toward target angle
            float smoothedYaw = Mathf.SmoothDampAngle(currentYaw, turret.YawAngle, ref yawSmoothVelocity, useSmooth);
            // if close enough, snap to target to avoid perpetual tiny corrections
            if (Mathf.Abs(Mathf.DeltaAngle(smoothedYaw, turret.YawAngle)) < 0.01f)
            {
                smoothedYaw = turret.YawAngle;
                yawSmoothVelocity = 0f;
            }
            yawTransform.localRotation = Quaternion.Euler(0f, smoothedYaw, 0f);
        }

        target.angularVelocity = Vector3.zero; // prevent physics from interfering with manual control
    }
}   