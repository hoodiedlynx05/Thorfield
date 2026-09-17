using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    Hull target;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (target == null)
        {
            target = GetComponent<Hull>();
            if (target == null) target = GetComponentInParent<Hull>();
            if (target == null) target = GetComponentInChildren<Hull>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Keyboard.current.wKey.isPressed)
        {
            // Use the Hull's forward so movement aligns with the hull orientation
            target.AddForce(target.transform.forward * 100f, ForceMode.Acceleration);
        }
        if (Keyboard.current.sKey.isPressed)
        {
            target.AddForce(-target.transform.forward * 100f, ForceMode.Acceleration);
        }
        if (Keyboard.current.aKey.isPressed)
        {
            target.AddForce(-target.transform.right * 100f, ForceMode.Acceleration);
        }
        if (Keyboard.current.dKey.isPressed)
        {
            target.AddForce(target.transform.right * 100f, ForceMode.Acceleration);
        }
    }
}
