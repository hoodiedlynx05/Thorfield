using UnityEngine;
using Assets.Scripts;
using static Assets.Scripts.CustomValues;   

namespace Assets.Scripts.Suspension
{
    public class SuspensionContactSensor : MonoBehaviour
    {
        [SerializeField] private float travel = 0.5f;
        [SerializeField] private LayerMask groundMask;

        public SuspensionContact GetContact()
        {
            Vector3 origin = transform.position;
            Vector3 direction = -transform.up;

            if (Physics.Raycast(
                    origin,
                    direction,
                    out RaycastHit hit,
                    travel,
                    groundMask))
            {
                return new SuspensionContact
                {
                    grounded = true,
                    point = hit.point,
                    normal = hit.normal,
                    distance = hit.distance
                };
            }

            return new SuspensionContact
            {
                grounded = false,
                point = origin + direction * travel,
                normal = transform.up,
                distance = travel
            };
        }
    }
}