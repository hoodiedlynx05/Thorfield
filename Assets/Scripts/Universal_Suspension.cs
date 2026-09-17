using UnityEngine;
using Assets.Scripts;


/*
 * ========================================================
 * UNIVERSAL SUSPENSION
 * ========================================================
 */ 

public class UniversalSuspension : MonoBehaviour
{
    /*
     * ========================================================
     * SUSPENSION TYPE
     * ========================================================
     */



    /*
     * ========================================================
     * GENERAL SETTINGS
     * ========================================================
     */

    [Header("General Suspension")]

    [SerializeField]
    private SuspensionType suspensionType = SuspensionType.Null;

    [Tooltip("The roadwheel transform group")]
    [SerializeField]
    private Transform roadwheelTransformGroup;

    [Tooltip("Optional suspension arm / bogie transform group")]
    [SerializeField]
    private Transform suspensionArmTransformGroup;

    [Tooltip("Radius of the wheel in meters")]
    [SerializeField]
    private float wheelRadius = 0.25f;

    [Tooltip("Distance from suspension pivot to wheel center at rest")]
    [SerializeField]
    private float restLength = 0.50f;

    [Tooltip("Maximum compression from rest position")]
    [SerializeField]
    private float maximumCompression = 0.20f;

    [Tooltip("Maximum extension from rest position")]
    [SerializeField]
    private float maximumExtension = 0.15f;

    [Tooltip("Layers considered to be ground")]
    [SerializeField]
    private LayerMask groundMask = 6;

    [SerializeField]
    private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;


    /*
     * ========================================================
     * MECHANICAL GEOMETRY
     * ========================================================
     */

    [Header("Mechanical Geometry")]

    [Tooltip("How far the internal spring moves for one meter " + "of wheel movement."
    )]
    [SerializeField]
    private float springMotionRatio = 1.0f;

    [Tooltip("Distance from a rotational suspension pivot to " + "the point where the wheel force acts."
    )]
    [SerializeField]
    private float leverArm = 0.30f;

    [Tooltip("Use rotational movement for the suspension arm " + "instead of directly moving the wheel."
    )]
    [SerializeField]
    private bool useSuspensionArm = false;

    [Tooltip("Rotation axis of the suspension arm in local space."
    )]
    [SerializeField]
    private Vector3 armRotationAxis = Vector3.right;

    [Tooltip("Arm rotation in degrees per meter of wheel compression."
    )]
    [SerializeField]
    private float armRotationDegreesPerMeter = 60f;

    [SerializeField]
    private bool invertArmRotation = false;

    [Header("Suspension Mount")]

    [Tooltip("Point where the suspension starts its ground cast.")]
    [SerializeField]
    private Transform suspensionMount;


    /*
     * ========================================================
     * TORSION BAR SETTINGS
     * ========================================================
     */  

    [Header("Torsion Bar")]

    [SerializeField]
    private float torsionBarLength = 0.80f;

    [SerializeField]
    private float torsionBarDiameter = 0.05f;

    [Tooltip("Shear modulus in GPa.")]
    [SerializeField]
    private float modulusOfRigidityGPa = 50f;
    private float siUnitScale = 1_000f;
    private float modulusOfRigidityPa => Mathf.Pow(siUnitScale,3) * modulusOfRigidityGPa;

    [Tooltip("Rotational damping in N*m*s/rad.")]
    [SerializeField]
    private float torsionDamping = 1000f;

    [SerializeField]
    private float maximumTwistDegrees = 30f;


    /*
     * ========================================================
     * HVSS SETTINGS
     * ========================================================
     */  

    [Header("HVSS")]

    [SerializeField]
    private float hvssSpringRate = 50_000f;

    [SerializeField]
    private float hvssProgressiveRate = 20_000f;

    [SerializeField]
    private float hvssDamping = 5000f;

    [SerializeField]
    private float hvssPreload = 0f;

    [SerializeField]
    private float hvssMaximumSpringCompression = 0.30f;


    /*
     * ========================================================
     * VVSS SETTINGS
     * ========================================================
     */

    [Header("VVSS")]

    [SerializeField]
    private float vvssSpringRate = 50_000f;

    [SerializeField]
    private float vvssProgressiveRate = 20_000f;

    [SerializeField]
    private float vvssDamping = 5000f;

    [SerializeField]
    private float vvssPreload = 0f;

    [SerializeField]
    private float vvssMaximumSpringCompression = 0.30f;


    /*
     * ========================================================
     * HYDRAULIC SETTINGS
     * ========================================================
     */

    [Header("Hydraulic")]

    [SerializeField]
    private float hydraulicCylinderBore = 0.10f;

    [Tooltip("Base hydraulic pressure in Pa.")]
    [SerializeField]
    private float hydraulicBasePressure = 10_000_000f;

    [Tooltip("Pressure increase per meter of compression.")]
    [SerializeField]
    private float hydraulicPressureIncrease = 20_000_000f;

    [Tooltip("Additional damping pressure per m/s.")]
    [SerializeField]
    private float hydraulicDampingPressure = 2_000_000f;

    [SerializeField]
    private float hydraulicMaximumPressure = 30_000_000f;

    [SerializeField]
    private float hydraulicEfficiency = 0.95f;


    /*
	 * ========================================================
	 * HYDRO-PNEUMATIC SETTINGS
	 * ========================================================
     */

    [Header("Hydro-Pneumatic")]

    [SerializeField]
    private float hydroCylinderBore = 0.10f;

    [Tooltip("Initial gas pressure, gauge pressure, Pa.")]
    [SerializeField]
    private float hydroInitialGasPressure = 5_000_000f;

    [Tooltip("Initial gas volume in cubic meters.")]
    [SerializeField]
    private float hydroInitialGasVolume = 0.005f;

    [Tooltip("Polytropic exponent. " + "Around 1.0 is isothermal; " + "around 1.4 is adiabatic."
    )]
    [SerializeField]
    private float hydroPolytropicExponent = 1.4f;

    [SerializeField]
    private float hydroAmbientPressure = 101_325f;

    [SerializeField]
    private float hydroDamping = 5000f;

    [SerializeField]
    private float hydroMaximumPressure = 50_000_000f;

    [SerializeField]
    private float hydroEfficiency = 0.95f;


    /*
	 * ========================================================
	 * RUNTIME DATA
	 * ========================================================
     */

    private ISuspensionModel suspensionModel;

    private SuspensionType activeSuspensionType;

    private float previousLength;

    private bool initialized;

    private Quaternion armRestRotation;


    // These are public read-only values that other systems
    // can use later.

    public SuspensionState CurrentState
    {
        get;
        private set;
    }

    public SuspensionContact CurrentContact
    {
        get;
        private set;
    }

    public float CurrentLength
    {
        get;
        private set;
    }

    public float SuspensionVelocity
    {
        get;
        private set;
    }


    /*
	 * ========================================================
	 * UNITY INITIALIZATION 
	 * ========================================================
	 */
    private void Awake()
    {
        Initialize();
    }


    private void OnEnable()
    {
        Initialize();
    }


    private void Initialize()
    {
        if (initialized &&
            activeSuspensionType ==  suspensionType)
        {
            return;
        }

        CreateSuspensionModel();

        previousLength = restLength;

        CurrentLength = restLength;

        foreach(Transform transform in suspensionArmTransformGroup)
        if (transform !=  null)
        {
            armRestRotation = transform.localRotation;
        }

        activeSuspensionType = suspensionType;

        initialized = true;
    }


    /*
	 * ========================================================
	 * PHYSICS UPDATE
	 * ========================================================
	 */

    private void FixedUpdate()
    {
        if (!initialized ||
            activeSuspensionType !=  suspensionType)
        {
            Initialize();
        }

        UpdateSuspension(Time.fixedDeltaTime);
    }


    /*
	 * ========================================================
	 * MAIN SUSPENSION UPDATE
	 * ========================================================
	 */
    private void UpdateSuspension(float deltaTime)
    {
        if (deltaTime <=  0f)
            return;


        // ----------------------------------------------------
        // 1. Detect ground
        // ----------------------------------------------------

        SuspensionContact contact = DetectGround();

        CurrentContact = contact;


        // ----------------------------------------------------
        // 2. Determine current suspension length
        // ----------------------------------------------------

        float currentLength;

        if (contact.grounded)
        {
            currentLength = Mathf.Clamp(        
                    contact.distance,MinimumLength,MaximumLength);
        }
        else
        {
            // No ground = suspension fully extends.

            currentLength = MaximumLength;
        }

        CurrentLength = currentLength;


        // ----------------------------------------------------
        // 3. Calculate suspension velocity
        // ----------------------------------------------------
        //
        // POSITIVE:
        //     suspension is compressing
        //
        // NEGATIVE:
        //     suspension is extending
        //
        // Therefore:
        //
        // velocity = previousLength - currentLength
        //
        // ----------------------------------------------------

        SuspensionVelocity = (previousLength - currentLength)
            / deltaTime;


        // ----------------------------------------------------
        // 4. Build input
        // ----------------------------------------------------

        SuspensionInput input = new SuspensionInput
            {
                grounded = contact.grounded, restLength = restLength, currentLength = currentLength, travel = maximumCompression + maximumExtension, contactPoint = contact.point, contactNormal = contact.normal, SuspensionVelocity = SuspensionVelocity, wheelVelocity = SuspensionVelocity, angularVelocity = CalculateAngularVelocity(SuspensionVelocity), leverArm = leverArm, springMotionRatio = springMotionRatio
            };


        // ----------------------------------------------------
        // 5. Run suspension model
        // ----------------------------------------------------

        SuspensionState state = suspensionModel.Simulate(input,deltaTime);


        CurrentState = state;


        // ----------------------------------------------------
        // 6. Apply geometry
        // ----------------------------------------------------

        ApplySuspensionGeometry(currentLength,state);


        // ----------------------------------------------------
        // 7. Save previous position
        // ----------------------------------------------------

        previousLength = currentLength;
    }


    /*
	 * ========================================================
	 * GROUND DETECTION
	 * ========================================================
	 */

    private SuspensionContact DetectGround()
    {
        Vector3 origin = suspensionMount !=  null ? suspensionMount.position : transform.position;

        Vector3 direction = suspensionMount !=  null ? -suspensionMount.up : -transform.up;

        float castDistance = restLength + maximumExtension + 0.5f;

        Debug.DrawLine(origin,origin + direction * castDistance,Color.red,Time.fixedDeltaTime);

        if (Physics.SphereCast(origin,wheelRadius,direction,out RaycastHit hit,castDistance,groundMask,QueryTriggerInteraction.Ignore))
        {
            Vector3 wheelCenter = hit.point + hit.normal * wheelRadius;

            float currentLength = Vector3.Dot(wheelCenter - origin,direction);

            currentLength = Mathf.Clamp(currentLength,MinimumLength,MaximumLength);

            Debug.DrawLine(origin,wheelCenter,Color.green,Time.fixedDeltaTime);

            Debug.DrawRay(hit.point,hit.normal * 0.5f,Color.blue,Time.fixedDeltaTime);

            return new SuspensionContact
            {
                grounded = true,point = hit.point,normal = hit.normal,distance = currentLength
            };
        }

        return new SuspensionContact
        {
            grounded = false, point = origin + direction * castDistance, normal = transform.up, distance = castDistance
        };
    }


    /*
	 * ========================================================
	 * APPLY SUSPENSION GEOMETRY
	 * ========================================================
	 */

    private void ApplySuspensionGeometry(float currentLength,SuspensionState state)
    {
        Vector3 axis = -transform.up;


        // ----------------------------------------------------
        // DIRECT WHEEL MOVEMENT
        // ----------------------------------------------------

        if (!useSuspensionArm)
        {
            foreach (Transform roadwheel in roadwheelTransformGroup)
            {
                if (roadwheel !=  null)
                {
                    roadwheel.position = transform.position + axis * currentLength;
                }

                return;
            }
        }


        // ----------------------------------------------------
        // SUSPENSION ARM MOVEMENT
        // ----------------------------------------------------

        foreach (Transform transform in suspensionArmTransformGroup)
        {
            if (transform ==  null)
            {
                return;
            }
        }

        float compression = restLength - currentLength;


        float angle = compression * armRotationDegreesPerMeter;


        if (invertArmRotation)
        {
            angle = -angle;
        }


        Vector3 rotationAxis = armRotationAxis.normalized;

        foreach (Transform suspensionArm in suspensionArmTransformGroup)
        {
            suspensionArm.localRotation = armRestRotation * Quaternion.AngleAxis(angle,rotationAxis);
        }
    }


    /*
	 * ========================================================
	 * ANGULAR VELOCITY
	 * ========================================================
	 */

    private float CalculateAngularVelocity(float linearVelocity)
    {
        if (leverArm <=  0.0001f)
            return 0f;


        return
            (linearVelocity * springMotionRatio) / leverArm;
    }


    /*
	 * ========================================================
	 * LENGTH LIMITS
	 * ========================================================
	 */

    private float MinimumLength
    {
        get
        {
            return Mathf.Max(0.001f,restLength - maximumCompression);
        }
    }


    private float MaximumLength
    {
        get
        {
            return
                restLength + maximumExtension;
        }
    }


    /*
	 * ========================================================
	 * CREATE SUSPENSION MODEL
     * ========================================================
	 */

    private void CreateSuspensionModel()
    {
        switch (suspensionType)
        {
            case SuspensionType.Null:
                new NullSuspension();
                break;

            case SuspensionType.VVSS:

                VVSSuspension vvss = new VVSSuspension
                    {
                        SpringRate = vvssSpringRate, ProgressiveRate = vvssProgressiveRate, DampingConstant = vvssDamping, PreloadForce = vvssPreload, MaximumSpringCompression = vvssMaximumSpringCompression
                    };

                suspensionModel = vvss;

                break;

            case SuspensionType.HVSS:

                HVSSuspension hvss = new HVSSuspension
                    {
                        SpringRate = hvssSpringRate, ProgressiveRate = hvssProgressiveRate, DampingConstant = hvssDamping, PreloadForce = hvssPreload, MaximumSpringCompression = hvssMaximumSpringCompression
                    };

                suspensionModel = hvss;

                break;

            case SuspensionType.Torsion:

                TorsionBarSuspension torsion = new TorsionBarSuspension
                    {
                        TorsionSpringLength = torsionBarLength, TorsionDiameter = torsionBarDiameter, ModulusOfRigidity = modulusOfRigidityGPa, TorsionDampingConstant = torsionDamping, MaximumTwistDegrees = maximumTwistDegrees, DefaultLeverArm = leverArm
                    };

                suspensionModel = torsion;

                break;

            case SuspensionType.Hydraulic:

                HydraulicSuspension hydraulic = new HydraulicSuspension
                    {
                        CylinderBoreDiameter = hydraulicCylinderBore, BasePressurePa = hydraulicBasePressure, PressureIncreasePerMeter = hydraulicPressureIncrease, DampingPressurePerSpeed = hydraulicDampingPressure, MaximumPressurePa = hydraulicMaximumPressure, Efficiency = hydraulicEfficiency
                    };

                suspensionModel = hydraulic;

                break;

            case SuspensionType.HydroPneumatic:

                HydroPneumaticSuspension hydro = new HydroPneumaticSuspension
                    {
                        CylinderBoreDiameter = hydroCylinderBore, InitialGasPressurePa = hydroInitialGasPressure, InitialGasVolume = hydroInitialGasVolume, PolytropicExponent = hydroPolytropicExponent, AmbientPressurePa = hydroAmbientPressure, DampingConstant = hydroDamping, MaximumPressurePa = hydroMaximumPressure, Efficiency = hydroEfficiency
                    };

                suspensionModel = hydro;

                break;
        }
    }


    /*
	 * ========================================================
	 * DEBUG GIZMOS
	 * ========================================================
	 */

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = suspensionMount != null ? suspensionMount.position : transform.position;

        Vector3 direction = -transform.up;


        // Full suspension range.

        Gizmos.DrawLine(origin + direction * MinimumLength, origin + direction * MaximumLength);


        // Rest position.

        Gizmos.DrawWireSphere(origin + direction * restLength, wheelRadius);


        // Current wheel position.

        if (Application.isPlaying)
        {
            Gizmos.DrawWireSphere(origin + direction * CurrentLength, wheelRadius);


            // Contact point.

            if (CurrentContact.grounded)
            {
                Gizmos.DrawLine(CurrentContact.point, CurrentContact.point + CurrentContact.normal * 0.25f);
            }
        }
    }
}


/*
 * ========================================================
 *  CONTACT DATA
 * ========================================================
 */

public struct SuspensionContact
{
    public bool grounded;

    public Vector3 point;

    public Vector3 normal;

    public float distance;
}


/* 
 * INPUT DATA
*/ 

public struct SuspensionInput
{
    public bool grounded;

    public float restLength;

    public float currentLength;

    public float travel;

    public Vector3 contactPoint;

    public Vector3 contactNormal;

    // Positive = compression
    // Negative = rebound
    public float SuspensionVelocity;

    public float wheelVelocity;

    // radians / second
    public float angularVelocity;

    // meters
    public float leverArm;

    // spring movement / wheel movement
    public float springMotionRatio;
}


/*
 * ========================================================
 * OUTPUT DATA
 * ========================================================
 */ 

public struct SuspensionState
{
    public bool grounded;

    // meters
    public float compression;

    // 0 -> 1
    public float normalizedCompression;

    // Newtons
    public float force;

    // Newton meters
    public float torque;

    // radians
    public float angularDisplacement;

    // meters
    public float linearDisplacement;

    // meters
    public float springCompression;

    // Pascals
    public float pressure;
}


/*
 * ========================================================
 * SUSPENSION MODEL INTERFACE
 * ========================================================
 */ 

public interface ISuspensionModel
{
    SuspensionState Simulate(SuspensionInput input,float deltaTime);
}


/*
 * ========================================================
 * COMMON MATH
 * ========================================================
 */ 

public static class SuspensionMath
{
    public static float GetCompression(SuspensionInput input)
    {
        return Mathf.Clamp(input.restLength - input.currentLength,0f,input.travel);
    }


    public static float GetNormalizedCompression(float compression,float travel)
    {
        if (travel <=  0f)
            return 0f;

        return Mathf.Clamp01(compression / travel);
    }


    public static float GetMotionRatio(SuspensionInput input)
    {
        if (Mathf.Abs(input.springMotionRatio)
            < 0.0001f)
        {
            return 1f;
        }

        return input.springMotionRatio;
    }


    public static float GetLeverArm(SuspensionInput input,float fallback)
    {
        if (input.leverArm > 0.0001f)
            return input.leverArm;

        return fallback;
    }
}


/*
 * ========================================================
 * Null
 * ========================================================
 */ 

public class NullSuspension : ISuspensionModel
{
    public SuspensionState Simulate(SuspensionInput input,float deltaTime)
    {
        return new SuspensionState()
        {
            grounded = input.grounded
        };
    }
}


/*
 * ========================================================
 * VVSS
 * ========================================================
 */ 

public class VVSSuspension : ISuspensionModel
{
    public float SpringRate = 50_000f;

    public float ProgressiveRate = 20_000f;

    public float DampingConstant = 5000f;

    public float PreloadForce = 0f;

    public float MaximumSpringCompression = 0.30f;


    public SuspensionState Simulate(SuspensionInput input,float deltaTime)
    {
        SuspensionState state = new SuspensionState();

        state.grounded = input.grounded;

        if (!input.grounded)
            return state;


        float wheelCompression = SuspensionMath.GetCompression(input);

        state.compression = wheelCompression;

        state.normalizedCompression = SuspensionMath.GetNormalizedCompression(wheelCompression,input.travel);


        float motionRatio = SuspensionMath.GetMotionRatio(input);


        float springCompression = wheelCompression * motionRatio;

        springCompression = Mathf.Clamp(springCompression,0f,MaximumSpringCompression);

        state.springCompression = springCompression;


        // Progressive volute spring.

        float springForce = PreloadForce + SpringRate * springCompression + ProgressiveRate * springCompression * springCompression;


        float springVelocity = input.SuspensionVelocity * motionRatio;


        float dampingForce = DampingConstant * springVelocity;


        float totalSpringForce = springForce + dampingForce;


        float wheelForce = totalSpringForce * motionRatio;


        state.force = Mathf.Max(0f,wheelForce);


        return state;
    }
}


/*
 * ========================================================
 * HVSS
 * ========================================================
 */ 

public class HVSSuspension : ISuspensionModel
{
    public float SpringRate = 50_000f;

    public float ProgressiveRate = 20_000f;

    public float DampingConstant = 5000f;

    public float PreloadForce = 0f;

    public float MaximumSpringCompression = 0.30f;


    public SuspensionState Simulate(SuspensionInput input,float deltaTime)
    {
        SuspensionState state = new SuspensionState();

        state.grounded = input.grounded;

        if (!input.grounded)
            return state;


        float wheelCompression = SuspensionMath.GetCompression(input);

        state.compression = wheelCompression;

        state.normalizedCompression = SuspensionMath.GetNormalizedCompression(wheelCompression,input.travel);


        float motionRatio = SuspensionMath.GetMotionRatio(input);


        // Wheel → spring movement

        float springCompression = wheelCompression * motionRatio;

        springCompression = Mathf.Clamp(springCompression,0f,MaximumSpringCompression);

        state.springCompression = springCompression;


        // Progressive volute spring:
        //
        // F = Fpreload + kx + px²

        float springForce = PreloadForce + SpringRate * springCompression + ProgressiveRate * springCompression * springCompression;


        // Damping

        float springVelocity = input.SuspensionVelocity * motionRatio;

        float dampingForce = DampingConstant * springVelocity;


        float totalSpringForce = springForce + dampingForce;


        float wheelForce = totalSpringForce * motionRatio;


        state.force = Mathf.Max(0f,wheelForce);


        return state;
    }
}


/*
 * ========================================================
 * TORSION BAR
 * ========================================================
 */ 

public class TorsionBarSuspension : ISuspensionModel
{
    public float TorsionSpringLength = 0.80f;

    public float TorsionDiameter = 0.05f;

    public float ModulusOfRigidity = 50_000_000_000f;

    public float TorsionDampingConstant = 1000f;

    public float MaximumTwistDegrees = 30f;

    public float DefaultLeverArm = 0.30f;


    public SuspensionState Simulate(SuspensionInput input,float deltaTime)
    {
        SuspensionState state = new SuspensionState();

        state.grounded = input.grounded;

        if (!input.grounded)
            return state;


        // ----------------------------------------------------
        // Compression
        // ----------------------------------------------------

        float compression = SuspensionMath.GetCompression(input);

        state.compression = compression;

        state.normalizedCompression = SuspensionMath.GetNormalizedCompression(compression,input.travel);


        // ----------------------------------------------------
        // Geometry
        // ----------------------------------------------------

        float leverArm = SuspensionMath.GetLeverArm(input,DefaultLeverArm);

        float motionRatio = SuspensionMath.GetMotionRatio(input);


        // ----------------------------------------------------
        // Wheel displacement → torsion angle
        // ----------------------------------------------------

        float angularDisplacement = (compression * motionRatio) / leverArm;


        // Limit maximum twist.

        float maximumTwist = MaximumTwistDegrees * Mathf.Deg2Rad;

        angularDisplacement = Mathf.Clamp(angularDisplacement,-maximumTwist,maximumTwist);


        state.angularDisplacement = angularDisplacement;


        // ----------------------------------------------------
        // Polar second moment
        //
        // J = πd⁴ / 32
        // ----------------------------------------------------

        float J = Mathf.PI * Mathf.Pow(TorsionDiameter,4f) / 32f;


        // ----------------------------------------------------
        // Torsional spring constant
        //
        // k = GJ / L
        //
        // N*m/rad
        // ----------------------------------------------------

        float torsionalSpringConstant = (ModulusOfRigidity * J) / TorsionSpringLength;


        // ----------------------------------------------------
        // Spring torque
        //
        // T = kθ
        // ----------------------------------------------------

        float springTorque = torsionalSpringConstant * angularDisplacement;


        // ----------------------------------------------------
        // Angular velocity
        // ----------------------------------------------------

        float angularVelocity = input.angularVelocity;


        // ----------------------------------------------------
        // Damping torque
        //
        // T = cω
        // ----------------------------------------------------

        float dampingTorque = TorsionDampingConstant * angularVelocity;


        // ----------------------------------------------------
        // Total torque
        // ----------------------------------------------------

        float totalTorque = springTorque + dampingTorque;


        state.torque = totalTorque;


        // ----------------------------------------------------
        // Torque → wheel force
        //
        // F = T / r
        // ----------------------------------------------------

        float wheelForce = (totalTorque * motionRatio) / leverArm;


        state.force = Mathf.Max(0f,wheelForce);


        return state;
    }
}


/*
 * ========================================================
 * HYDRAULIC
 * ========================================================
 */ 

public class HydraulicSuspension : ISuspensionModel
{
    public float CylinderBoreDiameter = 0.10f;

    public float BasePressurePa = 10_000_000f;

    public float PressureIncreasePerMeter = 20_000_000f;

    public float DampingPressurePerSpeed = 2_000_000f;

    public float MaximumPressurePa = 30_000_000f;

    public float Efficiency = 0.95f;


    public SuspensionState Simulate(SuspensionInput input,float deltaTime)
    {
        SuspensionState state = new SuspensionState();

        state.grounded = input.grounded;

        if (!input.grounded)
            return state;


        float compression = SuspensionMath.GetCompression(input);

        state.compression = compression;

        state.normalizedCompression = SuspensionMath.GetNormalizedCompression(compression,input.travel);


        // ----------------------------------------------------
        // Piston area
        //
        // A = πr²
        // ----------------------------------------------------

        float radius = CylinderBoreDiameter / 2f;

        float pistonArea = Mathf.PI * radius * radius;


        // ----------------------------------------------------
        // Pressure
        // ----------------------------------------------------

        float pressure = BasePressurePa + compression * PressureIncreasePerMeter;


        // ----------------------------------------------------
        // Hydraulic damping
        // ----------------------------------------------------

        pressure += input.SuspensionVelocity * DampingPressurePerSpeed;


        pressure = Mathf.Clamp(pressure,0f,MaximumPressurePa);


        state.pressure = pressure;


        // ----------------------------------------------------
        // F = P × A
        // ----------------------------------------------------

        float hydraulicForce = pressure * pistonArea;


        hydraulicForce *= Efficiency;


        state.force = Mathf.Max(0f,hydraulicForce);


        return state;
    }
}


/*
 * ========================================================
 * HYDRO-PNEUMATIC
 * ========================================================
 */ 

public class HydroPneumaticSuspension : ISuspensionModel
{
    public float CylinderBoreDiameter = 0.10f;

    public float InitialGasPressurePa = 5_000_000f;

    public float InitialGasVolume = 0.005f;

    public float PolytropicExponent = 1.4f;

    public float AmbientPressurePa = 101_325f;

    public float DampingConstant = 5000f;

    public float MaximumPressurePa = 50_000_000f;

    public float Efficiency = 0.95f;


    public SuspensionState Simulate(SuspensionInput input,float deltaTime)
    {
        SuspensionState state = new SuspensionState();

        state.grounded = input.grounded;

        if (!input.grounded)
            return state;


        float compression = SuspensionMath.GetCompression(input);

        state.compression = compression;

        state.normalizedCompression = SuspensionMath.GetNormalizedCompression(compression,input.travel);


        // ----------------------------------------------------
        // Piston area
        // ----------------------------------------------------

        float radius = CylinderBoreDiameter / 2f;

        float pistonArea = Mathf.PI * radius * radius;


        // ----------------------------------------------------
        // Hydraulic displacement
        //
        // ΔV = A × x
        // ----------------------------------------------------

        float displacedVolume = pistonArea * compression;


        float currentGasVolume = InitialGasVolume - displacedVolume;


        // Keep the gas volume above zero.

        currentGasVolume = Mathf.Max(currentGasVolume,InitialGasVolume * 0.05f);


        // ----------------------------------------------------
        // Convert gauge pressure → absolute
        // ----------------------------------------------------

        float initialAbsolutePressure = InitialGasPressurePa + AmbientPressurePa;


        // ----------------------------------------------------
        // Polytropic gas law
        //
        // P₁V₁ⁿ = P₂V₂ⁿ
        //
        // P₂ = P₁(V₁ / V₂)ⁿ
        // ----------------------------------------------------

        float currentAbsolutePressure = initialAbsolutePressure * Mathf.Pow(InitialGasVolume / currentGasVolume, PolytropicExponent);


        // Back to gauge pressure.

        float pressure = currentAbsolutePressure - AmbientPressurePa;


        pressure = Mathf.Clamp(pressure,0f,MaximumPressurePa);


        state.pressure = pressure;


        // ----------------------------------------------------
        // Gas spring force
        //
        // F = P × A
        // ----------------------------------------------------

        float gasForce = pressure * pistonArea * Efficiency;


        // ----------------------------------------------------
        // Damping
        // ----------------------------------------------------

        float dampingForce = input.SuspensionVelocity * DampingConstant;


        // ----------------------------------------------------
        // Total force
        // ----------------------------------------------------

        float totalForce = gasForce + dampingForce;


        state.force = Mathf.Max(0f,totalForce);


        state.springCompression = displacedVolume / pistonArea;

            
        return state;
    }
}   