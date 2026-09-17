using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody))]
public class Hull : MonoBehaviour
{

    public GameObject HullPrefab;

    // cached transform state
    public Vector3 position;
    public Quaternion rotation;

    // angular velocity applied by controllers
    public Vector3 HullVel;
    // runtime angles (degrees)
    public float Z_Vel;
    public float X_Vel;
    public float Y_Vel;
    public enum Axis { X, Y, Z }
    public Axis Hull_X_Axis = Axis.X;

    private Transform newTransform = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        newTransform = new GameObject("HullTransform").transform;
        newTransform.position = Vector3.zero;
        newTransform.rotation = Quaternion.identity;
        var Hull = HullPrefab.transform;
        // Default TurretPrefab to this GameObject if not provided
        if (HullPrefab == null) {
            HullPrefab = this.gameObject;
        }

        position = Hull.position;
        rotation = Hull.rotation;
        // Initialize stored angles from transforms
        Vector3 velLocal = Hull.position;
        // store as signed angles
        HullVel.z = velLocal.z;
        switch (Hull_X_Axis)
        {
            case Axis.X:
                HullVel.x = velLocal.x;
                break;
            case Axis.Y:
                HullVel.y = velLocal.y;
                break;
            case Axis.Z:
                HullVel.z = velLocal.z;
                break;
        }

        if(Hull_X_Axis == Axis.X)
        {
            newTransform.position = new Vector3(velLocal.x, velLocal.y, velLocal.z);
            newTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else if (Hull_X_Axis == Axis.Y)
        {
            newTransform.position = new Vector3(velLocal.z, velLocal.x, velLocal.y);
            newTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else if (Hull_X_Axis == Axis.Z)
        {
            newTransform.position = new Vector3(velLocal.y, velLocal.z, velLocal.x);
            newTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }
    public void AddForce(Vector3 force, ForceMode mode)
    {
        Vector3 transformedForce = newTransform.InverseTransformVector(force);
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(transformedForce, mode);
        }
    }
}
