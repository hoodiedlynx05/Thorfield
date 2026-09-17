using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;
public class Turret : MonoBehaviour
{
    // Optional GameObject representing the turret visual/rotating part.
    // If null, the GameObject that holds this Turret component will be used.
    public GameObject TurretPrefab;

    // Optional explicit pivots. If not assigned, defaults are chosen in Awake:
    // YawPivot: the transform that should rotate around the up axis (yaw)
    // PitchPivot: the transform that should rotate around the right axis (pitch)
    public Transform YawPivot;
    public Transform PitchPivot;

    // cached transform state
    public Vector3 position;
    public Quaternion rotation;

    // angular velocity applied by controllers
    public Vector3 AngularVel;
    // runtime angles (degrees)
    public float YawAngle;
    public float PitchAngle;

    void Awake()
    {
        // Default TurretPrefab to this GameObject if not provided
        if (TurretPrefab == null)
            TurretPrefab = this.gameObject;

        // If no explicit yaw pivot, use the TurretPrefab's transform
        if (YawPivot == null)
            YawPivot = TurretPrefab.transform;

        // If no explicit pitch pivot, try to use the first child of the yaw pivot,
        // otherwise fall back to the yaw pivot itself
        if (PitchPivot == null)
        {
            if (YawPivot.childCount > 0)
                PitchPivot = YawPivot.GetChild(0);
            else
                PitchPivot = YawPivot;
        }

        position = TurretPrefab.transform.position;
        rotation = TurretPrefab.transform.rotation;
        AngularVel = Vector3.zero;
        // Initialize stored angles from transforms
        Vector3 yawLocal = YawPivot.localEulerAngles;
        Vector3 pitchLocal = PitchPivot.localEulerAngles;
        // store as signed angles
        YawAngle = (yawLocal.y + 180f) % 360f - 180f;
        switch (PitchAxis)
        {
            case Axis.X:
                PitchAngle = -((pitchLocal.x + 180f) % 360f - 180f);
                break;
            case Axis.Y:
                PitchAngle = -((pitchLocal.y + 180f) % 360f - 180f);
                break;
            case Axis.Z:
                PitchAngle = -((pitchLocal.z + 180f) % 360f - 180f);
                break;
        }
    }

    // Helpers to get the transforms used for yaw/pitch operations
    public Transform GetYawTransform() => YawPivot != null ? YawPivot : this.transform;
    public Transform GetPitchTransform() => PitchPivot != null ? PitchPivot : this.transform;

    // Which local axis on the pitch transform represents pitching motion for this model.
    public enum Axis { X, Y, Z }
    public Axis PitchAxis = Axis.X;
}
