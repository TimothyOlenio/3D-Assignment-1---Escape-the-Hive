﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    //Create Variables
    //The speed at which the wheels turn the car
    public float maxTurnAngle;
    //The speed that the wheels push the car forward
    public float maxTorque;
    //Create collider variables to attach to WheelColliders
    public WheelCollider WheelFront_Left;
    public WheelCollider WheelFront_Right;
    public WheelCollider WheelBack_Left;
    public WheelCollider WheelBack_Right;

    //Create transform colliders to attach the Rendered Wheels 
    public Transform WheelTransformFL;
    public Transform WheelTransformFR;
    public Transform WheelTransformBL;
    public Transform WheelTransformBR;

    //A vector that pushes the car down to prevent it from flying away
    public Vector3 centerOfMassAdjustment = new Vector3(0f, -0.9f, 0f);
    private Rigidbody body;

    //A 
    public float spoilerRatio = 0.1f;
    //while the gas is off, the car slows down by this rate
    public float decelerationTorque;

    //the car's maximum speed
    public float topSpeed = 150;
    //A variable to get the player's current movement speed
    private float currentSpeed;

    //when the brakes are hit, slow down the car by this rate
    public float maxBrakeTorque = 100;
    private bool applyHandBrake = false;

    public float handBrakeForwardSlip = 0.04f;
    public float handBrakeSidewaysSlip = 0.08f;


    private void Start()
    {
        body = GetComponent<Rigidbody>();
        body.centerOfMass += centerOfMassAdjustment;
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
        WheelFront_Left.steerAngle = Input.GetAxis("Horizontal") * maxTurnAngle;
        WheelFront_Right.steerAngle = Input.GetAxis("Horizontal") * maxTurnAngle;


        Vector3 LocalVelocity = transform.InverseTransformDirection(body.velocity);
        body.AddForce(-transform.up * (LocalVelocity.z * spoilerRatio), ForceMode.Impulse);



        //  KM/H
        currentSpeed = WheelBack_Left.radius * WheelBack_Right.rpm * Mathf.PI * 0.12f;



        print(currentSpeed);
        print("Radius " + WheelBack_Left.radius);
        print("RPM " + WheelBack_Right.rpm);
        print("PIE " + Mathf.PI);
        print("" + 0.12f);



        if (currentSpeed < topSpeed)
        {
            //Rear wheel drive
            WheelBack_Left.motorTorque = Input.GetAxis("Vertical") * maxTorque;
            WheelBack_Right.motorTorque = Input.GetAxis("Vertical") * maxTorque;
            print(currentSpeed);
        }
        else
        {
            WheelBack_Left.motorTorque = 0;
            WheelBack_Right.motorTorque = 0;
        }
        //Hand brake

        if(Input.GetButton("Jump"))
        {
            applyHandBrake = true;
            WheelFront_Left.brakeTorque = maxBrakeTorque;
            WheelFront_Right.brakeTorque = maxBrakeTorque;

            //PowerSlide
            if(GetComponent<Rigidbody>().velocity.magnitude > 1)
            {
                SetSlipValues(handBrakeForwardSlip, handBrakeSidewaysSlip);
            }
            else
            {
                SetSlipValues(1f, 1f);
            }

        }
        else
        {
            applyHandBrake = false;
            WheelFront_Left.brakeTorque = 0;
            WheelFront_Right.brakeTorque = 0;
            SetSlipValues(1f, 1f);
        }

        if (!applyHandBrake && ((Input.GetAxis("Vertical") <= -0.5f && LocalVelocity.z > 0) ||
            (Input.GetAxis("Vertical") <= -0.5f && LocalVelocity.z > 0)))
        {
            WheelBack_Left.brakeTorque = decelerationTorque + maxTorque;
            WheelBack_Right.brakeTorque = decelerationTorque + maxTorque;
        }
        else if (!applyHandBrake && Input.GetAxis("Vertical") == 0)
        {
            WheelBack_Left.brakeTorque = decelerationTorque;
            WheelBack_Right.brakeTorque = decelerationTorque;
        }
        else
        {
            WheelBack_Left.brakeTorque = 0;
            WheelBack_Right.brakeTorque = 0;
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

        if(WheelFront_Left.GetGroundHit(out Contact))
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

}
