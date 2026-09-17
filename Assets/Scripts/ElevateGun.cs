using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class ElevateGun : MonoBehaviour
{
    public float maxVerticalAngle = 20f;
    public float minVerticalAngle = -10f;
    Camera m_Camera;

    float vfov;
    float aspect;
    float hfov;
    public Rigidbody target;
    Turret turret;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_Camera = Camera.main;
        if (m_Camera == null)
        {
            Debug.LogError("ElevateGun: No Camera.main found. Disabling script.");
            enabled = false;
            return;
        }

        vfov = m_Camera.fieldOfView;
        aspect = m_Camera.aspect;
        hfov = Camera.VerticalToHorizontalFieldOfView(vfov, aspect);

        // auto-find target/turret if not assigned in Inspector
        if (target == null)
        {
            target = GetComponent<Rigidbody>();
            if (target == null) target = GetComponentInParent<Rigidbody>();
            if (target == null) target = GetComponentInChildren<Rigidbody>();
        }

        if (target == null)
        {
            turret = FindAnyObjectByType<Turret>();
            if (turret != null) target = turret.GetComponent<Rigidbody>();
        }

        if (turret == null && target != null)
        {
            turret = target.GetComponent<Turret>();
        }

        if (turret == null)
        {
            turret = FindAnyObjectByType<Turret>();
        }
    }

    public float mouseSensitivity = 0.1f;
    // smoothing time in seconds for pitch smoothing (smaller = snappier)
    public float smoothTime = 0.05f;
    // faster smoothing while input is active
    public float activeSmoothTime = 0.01f;
    // slower smoothing while idle
    public float idleSmoothTime = 0.05f;
    // minimum mouse delta magnitude (pixels) to consider as active input
    public float inputDeadzone = 0.5f;
    private float pitchSmoothVelocity;

    // Update is called once per frame
    void Update()
    {

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Cursor.visible = false;
            Mouse.current.WarpCursorPosition(new Vector2(Screen.width / 2, Screen.height / 2));
        }


        if (!Cursor.visible && Mouse.current != null)
        {
            // Use raw mouse delta for reliable input when cursor is locked
            Vector2 delta = Mouse.current.delta.ReadValue();

            // recover turret reference if needed
            if (turret == null && target != null) turret = target.GetComponent<Turret>();
            if (turret == null) turret = FindAnyObjectByType<Turret>();
            if (turret == null) return;

            // compute pitch delta from mouse Y (positive when moving mouse up)
            float deltaDeg = delta.y * mouseSensitivity;
            turret.PitchAngle += deltaDeg;
            turret.PitchAngle = Mathf.Clamp(turret.PitchAngle, minVerticalAngle, maxVerticalAngle);

            // apply pitch to the pitch pivot using SmoothDampAngle to avoid drift/jitter
            Transform pitchTransform = turret.GetPitchTransform();
            Vector3 currentLocal = pitchTransform.localEulerAngles;
            float currentPitch;
            // choose smoothing time depending on whether input is active
            float useSmooth = delta.magnitude > inputDeadzone ? activeSmoothTime : idleSmoothTime;

            switch (turret.PitchAxis)
            {
                case Turret.Axis.X:
                    currentPitch = (currentLocal.x + 180f) % 360f - 180f;
                    // target transform angle is the negative of logical PitchAngle (so positive logical = pitch up)
                    float targetX = -turret.PitchAngle;
                    currentPitch = Mathf.SmoothDampAngle(currentPitch, targetX, ref pitchSmoothVelocity, useSmooth);
                    if (Mathf.Abs(Mathf.DeltaAngle(currentPitch, targetX)) < 0.01f) { currentPitch = targetX; pitchSmoothVelocity = 0f; }
                    currentLocal.x = currentPitch;
                    break;
                case Turret.Axis.Y:
                    currentPitch = (currentLocal.y + 180f) % 360f - 180f;
                    float targetY = -turret.PitchAngle;
                    currentPitch = Mathf.SmoothDampAngle(currentPitch, targetY, ref pitchSmoothVelocity, useSmooth);
                    if (Mathf.Abs(Mathf.DeltaAngle(currentPitch, targetY)) < 0.01f) { currentPitch = targetY; pitchSmoothVelocity = 0f; }
                    currentLocal.y = currentPitch;
                    break;
                case Turret.Axis.Z:
                    currentPitch = (currentLocal.z + 180f) % 360f - 180f;
                    float targetZ = -turret.PitchAngle;
                    currentPitch = Mathf.SmoothDampAngle(currentPitch, targetZ, ref pitchSmoothVelocity, useSmooth);
                    if (Mathf.Abs(Mathf.DeltaAngle(currentPitch, targetZ)) < 0.01f) { currentPitch = targetZ; pitchSmoothVelocity = 0f; }
                    currentLocal.z = currentPitch;
                    break;
            }
            pitchTransform.localEulerAngles = currentLocal;
        }
        target.angularVelocity = Vector3.zero; // prevent physics from interfering with pitch control
    }
}
