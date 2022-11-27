using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class SteeringWheel : XRBaseInteractable
{
    [SerializeField] private Transform steeringTransform;


    //Destination Angle
    private float toAngle = 0.0f;

    //Angle For SmoothMovement
    private float normalizedAngle = 0.0f;
    [SerializeField] private float minAngle, maxAngle;

    [Min(1)]
    [SerializeField] private float rotationSpeed = 1;

    private GameObject rHand;
    private GameObject lHand;

    // [SerializeField] private float turnDamping = 200f;
    private Vector3 positionAfterMovement;

    private Vector3 rHandLastPosition;
    private Vector3 lHandLastPosition;

    private bool rHandOnSteering;
    private bool lHandOnSteering;


    private float lastAngle = 0f;


    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        XRBaseInteractor _interactor = args.interactor;
        //Debug.Log(_interactor.gameObject.name);

        if (_interactor.gameObject.name == "RightHand")
        {
            rHand = _interactor.gameObject;
            rHandLastPosition = transform.InverseTransformPoint(rHand.transform.position);
            rHandLastPosition = new Vector3(rHandLastPosition.x, rHandLastPosition.y, 0);
            rHandOnSteering = true;
        }
        else
        {
            lHand = _interactor.gameObject;
            lHandLastPosition = transform.InverseTransformPoint(lHand.transform.position);
            lHandLastPosition = new Vector3(lHandLastPosition.x, lHandLastPosition.y, 0);
            lHandOnSteering = true;
        }

    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        XRBaseInteractor _interactor = args.interactor;

        // Debug.Log(_interactor.gameObject.name);

        if (_interactor.gameObject.name == "RightHand")
        {
            rHand = null;
            rHandOnSteering = false;

        }
        else
        {
            lHand = null;
            lHandOnSteering = false;
        }

        lastAngle = toAngle;
        Debug.Log("Last Angle : " + lastAngle);
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            if (isSelected)
                RotateSteering();
        }
    }

    private void FixedUpdate()
    {
        if (!rHandOnSteering && !lHandOnSteering && lastAngle != 0f)
        {
            Debug.Log(" Return To Normal Angle " + lastAngle);
            lastAngle = Mathf.Lerp(lastAngle, 0f, Time.deltaTime * rotationSpeed / 2);
            steeringTransform.localEulerAngles = new Vector3(0, 0, lastAngle);
            toAngle = normalizedAngle = lastAngle;

            if (lastAngle < 1f && lastAngle > -1f)
                lastAngle = 0f;
        }
    }

    public float GetSteeringAngle() => toAngle = Mathf.Clamp(toAngle, minAngle, maxAngle);
    private void RotateSteering()
    {
        // Convert that direction to an angle, then rotation
        float changedRotationAngle = FindSteeringAngleDifference();
        ApplyRotationOnSteering(changedRotationAngle);
        //TurnVehicle();


    }

    /* private void TurnVehicle()
     {
         var turn = -toAngle;

         turn /= 10f;

         vehicleRigidbody.MoveRotation(Quaternion.RotateTowards(vehicle.rotation, Quaternion.Euler(0, turn, 0), Time.deltaTime * turnDamping));
     }*/

    private void ApplyRotationOnSteering(float changedRotationAngle)
    {
        //'-' sign because '+' sign will move inverse
        toAngle -= changedRotationAngle;
        toAngle = Mathf.Clamp(toAngle, minAngle, maxAngle);

        normalizedAngle = Mathf.Lerp(normalizedAngle, toAngle, Time.deltaTime * rotationSpeed);
        steeringTransform.localEulerAngles = new Vector3(0, 0, normalizedAngle);
        //Debug.Log("Rotation : " + toAngle);
    }

    private float FindSteeringAngleDifference()
    {
        float angleChange = 0f;

        if (rHandOnSteering && rHand && !lHandOnSteering)
        {
            positionAfterMovement = FindLocalPoint(rHand.transform.position);
            angleChange = GetAngleBLines(positionAfterMovement, rHandLastPosition);
            rHandLastPosition = positionAfterMovement;
        }

        if (lHandOnSteering && lHand && !rHandOnSteering)
        {
            positionAfterMovement = FindLocalPoint(lHand.transform.position);
            angleChange = GetAngleBLines(positionAfterMovement, lHandLastPosition);
            lHandLastPosition = positionAfterMovement;
        }

        if (rHandOnSteering && lHandOnSteering && rHand && lHand)
        {
            positionAfterMovement = FindLocalPoint(rHand.transform.position);
            angleChange = GetAngleBLines(positionAfterMovement, rHandLastPosition);
            rHandLastPosition = positionAfterMovement;

            positionAfterMovement = FindLocalPoint(lHand.transform.position);
            angleChange += GetAngleBLines(positionAfterMovement, lHandLastPosition);
            lHandLastPosition = positionAfterMovement;

            angleChange /= 2;
        }

        return angleChange;
    }

    private float GetAngleBLines(Vector3 positionAfterMovement, Vector3 rHandLastPosition)
    {
        //signed angle to detect whether its left movement or right movement
        return Vector3.SignedAngle(positionAfterMovement, rHandLastPosition, Vector3.forward);
    }

    private Vector3 FindLocalPoint(Vector3 position)
    {
        // Convert the hand positions to local, so we can find the angle easier
        Vector3 pos = transform.InverseTransformPoint(position);
        return new Vector3(pos.x, pos.y, 0f);
    }


}