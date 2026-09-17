using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    [RequireComponent(typeof(Transform))]
    public class CustomValues
    {
        public enum Axis
        {
            X,
            Y,
            Z
        }

        public enum Side
        {
            Left,
            Right
        }
    }
    public enum SuspensionType
    {
        Null,
        VVSS,
        HVSS,
        Torsion,
        Hydraulic,
        HydroPneumatic
    }
    public struct SuspensionContact
    {
        public bool grounded;
        public Vector3 point;
        public Vector3 normal;
        public float distance;
    }
}
