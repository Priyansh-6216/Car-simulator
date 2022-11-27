using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.OpenXR.Input;

public class GearLever : MonoBehaviour
{/*
    [SerializeField] InputActionReference rightHapticAction;*/
    HingeJoint hinge;
    [SerializeField] WheelController wheelController;
    public string curGear;
    // Start is called before the first frame update
    void Start()
    {
        hinge = GetComponent<HingeJoint>();
    }

    // Update is called once per frame
    public void Update()
    {
        if(Application.isEditor)
        {
            if(Input.GetKeyDown(KeyCode.R))
            {
                ReverseMove();
            }
            if (Input.GetKeyDown(KeyCode.N))
            {
                NeutralMove();
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                ForwardMove();
            }

        }
        else
        {

            float rot = hinge.angle;
            Debug.Log(rot);
            if (rot <= 40 && rot >= 13)
            {
                Debug.Log("+ve");
                ReverseMove();
                //OpenXRInput.SendHapticImpulse(rightHapticAction, 0.3f, 0.1f, UnityEngine.InputSystem.XR.XRController.rightHand); //Right Hand Haptic Impulse

            }
            else if (rot <= -13 && rot >= -40)
            {
                Debug.Log("-ve");
                ForwardMove();
                //OpenXRInput.SendHapticImpulse(rightHapticAction, 0.3f, 0.1f, UnityEngine.InputSystem.XR.XRController.rightHand); //Right Hand Haptic Impulse
            }
            else if (rot > -13 && rot < 13)
            {
                Debug.Log("+/-ve");
                NeutralMove();
                //OpenXRInput.SendHapticImpulse(rightHapticAction, 0.3f, 0.1f, UnityEngine.InputSystem.XR.XRController.rightHand); //Right Hand Haptic Impulse
            }
        }

    }

    void ForwardMove()
    {
        curGear = "D";
        wheelController.dir = 1f;
    }
    void ReverseMove()
    {
        curGear = "R";
        wheelController.dir = -1f;
    }

    void NeutralMove()
    {
        curGear = "N";
        wheelController.dir = 0f;
    }
    public void OnDeselect()
    {
        float rot = hinge.angle;
        Debug.Log(rot);
        if (rot <= 40 && rot >= 13)
        {
            
            transform.localRotation = Quaternion.Euler(40f, transform.localEulerAngles.y, transform.localEulerAngles.z);

        }
        else if (rot <= -13 && rot >= -40)
        {
            transform.localRotation = Quaternion.Euler(-40f, transform.localEulerAngles.y, transform.localEulerAngles.z);
        }
        else if (rot > -13 && rot < 13)
        {
            transform.localRotation = Quaternion.Euler(0f, transform.localEulerAngles.y, transform.localEulerAngles.z);
        }
    }
}
