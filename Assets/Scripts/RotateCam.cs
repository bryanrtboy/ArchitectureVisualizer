using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RotateCam : MonoBehaviour
{

    SteamVR_Controller.Device device;
    SteamVR_TrackedObject trackedobj;

    public GameObject m_camPivotObject;
    public bool m_rotateY;

    public float multiplier = 30;

    Vector2 touchpad;

//    private float sensitivity = 3.5f;

    void Start()
    {
        trackedobj = GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)trackedobj.index);
    }


    void Update()
    {
        float tiltAroundX = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).x;
        float tiltAroundY = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y;

        if (device.GetTouch(SteamVR_Controller.ButtonMask.Touchpad))
        {
            if (!m_rotateY)
                tiltAroundY = 0;

            m_camPivotObject.transform.Rotate(tiltAroundY * multiplier, tiltAroundX * multiplier, 0);
        }

        if (device.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad))
        {
            m_camPivotObject.transform.Rotate(0, 0, 0);
        }
    }
}