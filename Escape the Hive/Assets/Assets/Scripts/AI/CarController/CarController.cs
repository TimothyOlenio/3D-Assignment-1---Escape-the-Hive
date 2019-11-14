using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public float maxTurnAngle = 10;
    public float maxTorque = 10;
    public WheelCollider WheelFront_Left;
    public WheelCollider WheelFront_Right;
    public WheelCollider WheelBack_Left;
    public WheelCollider WheelBack_Right;

    public Transform WheelTransformFL;
    public Transform WheelTransformFR;
    public Transform WheelTransformBL;
    public Transform WheelTransformBR;

    public Vector3 centerOfMassAdjustment = new Vector3(0f, -0.9f, 0f);
    private Rigidbody body;

    public float spoilerRatio = 0.1f;

    public float decelerationTorque = 30;

    public float topSpeed = 150;
    public float maxReverseSpeed = -50;
    private float currentSpeed;

    public float maxBrakeTorque = 100;
    private bool applyHandBrake = false;

    public float handBrakeForwardSlip = 0.04f;
    public float handBrakeSidewaysSlip = 0.08f;

    public Transform WaypointContainer;

    private Transform[] waypoints;
    private int currentWaypoint = 0;
    private float inputSteer;
    private float inputTorque;

    public float brakeingDistance = 6f;
    public float forwardOffset;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        body.centerOfMass += centerOfMassAdjustment;

        //Get the waypoints from the track.
        GetWaypoints();
    }

    private void Update()
    {
        float rotationThisFrame = 360 * Time.deltaTime;
        WheelTransformFL.Rotate(0, WheelFront_Left.rpm / rotationThisFrame, 0);
        WheelTransformFR.Rotate(0, WheelFront_Right.rpm / rotationThisFrame, 0);
        WheelTransformBL.Rotate(0, WheelBack_Left.rpm / rotationThisFrame, 0);
        WheelTransformBR.Rotate(0, WheelBack_Right.rpm / rotationThisFrame, 0);

        UpdateWheelPositions();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Front wheel steering
        Vector3 RelativeWaypointPosition = transform.InverseTransformPoint(new Vector3(waypoints[currentWaypoint].position.x, transform.position.y, waypoints[currentWaypoint].position.z));
        inputSteer = RelativeWaypointPosition.x / RelativeWaypointPosition.magnitude;

        Vector3 LocalVelocity = transform.InverseTransformDirection(body.velocity);
        body.AddForce(-transform.up * (LocalVelocity.z * spoilerRatio), ForceMode.Impulse);


        if (Mathf.Abs(inputSteer) < 0.5f)
        {
            inputTorque = (RelativeWaypointPosition.z / RelativeWaypointPosition.magnitude);
            applyHandBrake = false;
        }
        else
        {
            if (body.velocity.magnitude > 10)
            {
                applyHandBrake = true;
            }
            else if (LocalVelocity.z < 0)
            {
                applyHandBrake = false;
                inputTorque = -1;
                inputSteer *= -1;
            }
            else
            {
                applyHandBrake = false;
                inputTorque = 0;
            }
        }
        //Hand brake
        if (applyHandBrake)
        {
            SetSlipValues(handBrakeForwardSlip, handBrakeSidewaysSlip);
        }
        else
        {
            SetSlipValues(1f, 1f);
        }

        if (RelativeWaypointPosition.magnitude < 25)
        {
            currentWaypoint++;
            if (currentWaypoint >= waypoints.Length)
            {
                currentWaypoint = 0;
                //RaceManager.Instance.LapFinishedByAI(this);
            }
        }

        WheelFront_Left.steerAngle = inputSteer * maxTurnAngle;
        WheelFront_Right.steerAngle = inputSteer * maxTurnAngle;


        //  KM/H
        currentSpeed = WheelBack_Left.radius * WheelBack_Left.rpm * Mathf.PI * 0.12f;
        if (currentSpeed < topSpeed && currentSpeed > maxReverseSpeed)
        {
            float adjustment = ForwardRayCast();

            //Rear wheel drive
            WheelBack_Left.motorTorque = inputTorque * adjustment * maxTorque;
            WheelBack_Right.motorTorque = inputTorque * adjustment * maxTorque;
        }
        else
        {
            WheelBack_Left.motorTorque = 0;
            WheelBack_Right.motorTorque = 0;
        }

    }

    void SetSlipValues(float forward, float sideways)
    {
        WheelFrictionCurve tempStruct = WheelBack_Right.forwardFriction;
        tempStruct.stiffness = forward;
        WheelBack_Right.forwardFriction = tempStruct;

        tempStruct = WheelBack_Right.sidewaysFriction;
        tempStruct.stiffness = sideways;
        WheelBack_Right.sidewaysFriction = tempStruct;

        tempStruct = WheelBack_Left.forwardFriction;
        tempStruct.stiffness = forward;
        WheelBack_Left.forwardFriction = tempStruct;

        tempStruct = WheelBack_Left.sidewaysFriction;
        tempStruct.stiffness = sideways;
        WheelBack_Left.sidewaysFriction = tempStruct;
    }

    void UpdateWheelPositions()
    {
        WheelHit Contact = new WheelHit();

        if (WheelFront_Left.GetGroundHit(out Contact))
        {
            Vector3 temp = WheelFront_Left.transform.position;
            temp.y = (Contact.point + (WheelFront_Left.transform.up * WheelFront_Left.radius)).y;
            WheelTransformFL.position = temp;
        }
        if (WheelFront_Right.GetGroundHit(out Contact))
        {
            Vector3 temp = WheelFront_Right.transform.position;
            temp.y = (Contact.point + (WheelFront_Right.transform.up * WheelFront_Right.radius)).y;
            WheelTransformFR.position = temp;

        }
        if (WheelBack_Right.GetGroundHit(out Contact))
        {
            Vector3 temp = WheelBack_Right.transform.position;
            temp.y = (Contact.point + (WheelBack_Right.transform.up * WheelBack_Right.radius)).y;
            WheelTransformBR.position = temp;

        }
        if (WheelBack_Left.GetGroundHit(out Contact))
        {
            Vector3 temp = WheelBack_Left.transform.position;
            temp.y = (Contact.point + (WheelBack_Left.transform.up * WheelBack_Left.radius)).y;
            WheelTransformBL.position = temp;

        }
    }

    float ForwardRayCast()
    {
        RaycastHit hit;
        Vector3 CarFront = transform.position + (transform.forward * forwardOffset);
        Debug.DrawRay(CarFront, transform.forward * brakeingDistance);

        if (Physics.Raycast(CarFront, transform.forward, out hit, brakeingDistance))
        {
            return (((CarFront = hit.point).magnitude / brakeingDistance) * 2) - 1;
        }

        return 1f;
    }

    void GetWaypoints()
    {
        Transform[] potentialWaypoints = WaypointContainer.GetComponentsInChildren<Transform>();
        waypoints = new Transform[(potentialWaypoints.Length - 1)];

        for (int i = 1; i < potentialWaypoints.Length; i++)
        {
            waypoints[i - 1] = potentialWaypoints[i];
        }
    }
    public Transform GetCurrentWaypoint()
    {
        return waypoints[currentWaypoint];
    }
    public Transform GetLastWaypoint()
    {
        if (currentWaypoint - 1 < 0)
        {
            return waypoints[waypoints.Length - 1];
        }

        return waypoints[currentWaypoint - 1];

    }

}
