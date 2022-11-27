using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.OpenXR.Input;

public class WheelController : MonoBehaviour
{

    [SerializeField] private WheelCollider wheelFL;
    [SerializeField] private WheelCollider wheelFR;
    [SerializeField] private WheelCollider wheelRL;
    [SerializeField] private WheelCollider wheelRR;

    [SerializeField] private Transform wheelFLTransform;
    [SerializeField] private Transform wheelFRTransform;
    [SerializeField] private Transform wheelRLTransform;
    [SerializeField] private Transform wheelRRTransform;

    public float acceleration = 500f;
    public float breakingForce = 200f;
    public float maxSpeed = 15f;

    private float currentAcceleration = 0f;
    private float currentBreakforce = 0f;
    private float currentTurnAngle = 0f;
    private InputDevice rightController;
    private InputDevice leftController;

    private List<InputDevice> rdevices = new List<InputDevice>();
    private List<InputDevice> ldevices = new List<InputDevice>();

    private bool breakDown = false;
    [SerializeField] private SteeringWheel steering;


    [SerializeField] private Rigidbody carRigidBody;
    [SerializeField] private XRGrabInteractable grabInteractable;

    [SerializeField] private Camera camera;
    [SerializeField] private GameObject xrOrigin;


    public float dir;

    [SerializeField] private AudioSource[] audioSources;

    private void Start()
    {
        if(!Application.isEditor)
        {
            GetRightController();
            GetLeftController();
            camera.enabled = false;
        }
        else
        {
            camera.enabled = true;
            xrOrigin.SetActive(false);
        }
    }

    private void GetRightController()
    {
        var characteristic = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(characteristic, rdevices);
        rightController = rdevices.FirstOrDefault();
        Debug.Log("Controller : " + rightController.name);
    }

    private void GetLeftController()
    {
        var characteristic = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(characteristic, ldevices);
        leftController = ldevices.FirstOrDefault();
        
        Debug.Log("Controller : " + leftController.name);
    }

    void FixedUpdate()
    {
        if(!Application.isEditor)
        {
            XRView();
        }
        else
        {
            EditorView();
        }

    }
    void EditorView()
    {
        float angle = 40f * Input.GetAxis("Horizontal");
        float speed  = Input.GetAxis("Vertical");
        float torque = acceleration * 1.5f * Input.GetAxis("Vertical");
        bool isBreakDown = false;

        wheelFL.steerAngle = angle;
        wheelFR.steerAngle = angle;

       

        if (Input.GetKey(KeyCode.Space))
        {
            isBreakDown = true;
            
        }
        else
        {
            isBreakDown = false;
        }

        if (!isBreakDown)
        {
            audioSources[0].volume = speed;
            wheelFR.motorTorque = torque;
            wheelFL.motorTorque = torque;
            wheelRL.brakeTorque = 0f;
            wheelRR.brakeTorque = 0f;
            
        }
        else
        {
            wheelRL.brakeTorque = breakingForce * 1.5f;
            wheelRR.brakeTorque = breakingForce * 1.5f;
            audioSources[1].volume = 0.5f;
           
        }
    }
    void XRView()
    {
        if (ldevices.Count == 0 || rdevices.Count == 0)
        {
            GetRightController();
            GetLeftController();
        }

        if (carRigidBody.velocity.magnitude < 0.5f)
        {
            grabInteractable.enabled = true;
        }
        else
        {
            grabInteractable.enabled = false;
        }

        ApplyAcceleration();
        ApplyBreak();
        TurnVehicle();
        UpdateWheel(wheelFLTransform, wheelFL);
        UpdateWheel(wheelFRTransform, wheelFR);
        UpdateWheel(wheelRLTransform, wheelRL);
        UpdateWheel(wheelRRTransform, wheelRR);
    }

    private void TurnVehicle()
    {
        currentTurnAngle = -steering.GetSteeringAngle() / 10;
        if(Mathf.Abs(currentTurnAngle) >  10f && audioSources[1].volume != 0.2f)
        {
            audioSources[1].volume = 0.2f;
        }
        wheelFL.steerAngle = wheelFR.steerAngle = currentTurnAngle;
    }

    private void ApplyBreak()
    {
        leftController.TryGetFeatureValue(CommonUsages.trigger, out float breakForce);
       
        Debug.Log("Breakforce : " + breakForce);
        if (breakForce > 0.5f)
        {
            currentBreakforce = breakingForce * breakingForce;
            breakDown = true;
            if (carRigidBody.velocity.magnitude > 2f)
                audioSources[1].volume = 0.5f;
        }
        else
        {
            currentBreakforce = 0f;
            breakDown = false;
            audioSources[1].volume = 0f;
        }

        if (breakDown)
        {
            wheelRL.brakeTorque = currentBreakforce;
            wheelRR.brakeTorque = currentBreakforce;
        }
    }

    private void ApplyAcceleration()
    {
        rightController.TryGetFeatureValue(CommonUsages.trigger, out float btnValue);


        float speed = 1 - Mathf.Clamp(carRigidBody.velocity.magnitude / maxSpeed, 0, 1);
        Debug.LogWarning("Speed : " + carRigidBody.velocity.magnitude);

        if (btnValue > 0.5f)
        {
            currentAcceleration = acceleration * speed * dir;
            audioSources[0].volume = speed;
        }
        else
        {
            if (currentAcceleration != 0)
            {
                AutoBreak(true);
                currentAcceleration = 0f;
            }
            else if (carRigidBody.velocity.magnitude <= 0.5f)
            {
                Debug.LogWarning("Disable");
                AutoBreak(false);
            }
            audioSources[0].volume = 0.1f;
        }



        wheelFL.motorTorque = currentAcceleration;
        wheelFR.motorTorque = currentAcceleration;

        if (!breakDown)
        {
            wheelRL.motorTorque = currentAcceleration;
            wheelRR.motorTorque = currentAcceleration;
        }

    }

    private void AutoBreak(bool enable)
    {
       
        if(!enable)
        {
            wheelFL.brakeTorque = wheelFR.brakeTorque = wheelRL.brakeTorque = wheelRR.brakeTorque = 0f;
        }

        else
        {
            wheelFL.brakeTorque = breakingForce / 2;
            wheelFR.brakeTorque = breakingForce / 2;
            wheelRL.brakeTorque = breakingForce / 2;
            wheelRR.brakeTorque = breakingForce / 2;
        }
    }

    private void UpdateWheel(Transform wheel, WheelCollider col)
    {
        Vector3 pos;
        Quaternion rot;

        col.GetWorldPose(out pos, out rot);

        wheel.position = pos;
        wheel.rotation = rot;

    }

    
}
