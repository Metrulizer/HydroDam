using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayMode : MonoBehaviour
{
    private bool hasVR;
    private bool useVR;
    private Camera[] arr_camera;

    /*    public GameObject ;*/
    public Camera m_camera;
    public Camera ui_camera;
    public playerCamera m_playerCamera;

    /*    private float mouseX;
        private float mouseY;
        public float MouseSensitivity = 180f;*/

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        arr_camera = new Camera[2] { m_camera, ui_camera };
        //useVR = (m_camera.stereoTargetEye == UnityEngine.StereoTargetEyeMask.Both);

        // For some bizarre reason, IsHmdPresent returns true when there isn't a VR
        Debug.Log("Valve.VR.OpenVR.IsHmdPresent() = " + Valve.VR.OpenVR.IsHmdPresent());
        Debug.Log("IsRuntimeInstalled: " + Valve.VR.OpenVR.IsRuntimeInstalled());

        Valve.VR.OpenVR.Compositor.SetTrackingSpace(Valve.VR.ETrackingUniverseOrigin.TrackingUniverseSeated);

        // And this code will crash because of openvr
        Debug.Log("Has Device Name = " + UnityEngine.XR.XRSettings.loadedDeviceName);
        hasVR = (UnityEngine.XR.XRSettings.loadedDeviceName != "");
        useVR = hasVR;
        if (!hasVR) UnityEngine.XR.XRSettings.enabled = false;  // <could improve performance by disabling this background. untested at runtime 12feb@1911
        m_playerCamera.enabled = useVR ? false : true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.LogWarning("VR Toggler is currently broken. It appears to be related to the URP Camera Overlays.");
            VR_Toggle();
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            VR_Recenter();
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            foreach (Transform child in ui_camera.gameObject.transform.GetChild(0))
                child.gameObject.SetActive(!ui_camera.gameObject.transform.GetChild(0).GetChild(2).gameObject.activeSelf);
        }
    }

    private void VR_Toggle()
    {
        if (hasVR)
        {
            useVR ^= true;
            // Set mode
            // BROKEN DUE TO CAMERA OVERLAYS
            foreach (Camera i in arr_camera)
            {
                i.stereoTargetEye =
                    useVR ?
                    UnityEngine.StereoTargetEyeMask.Both :
                    UnityEngine.StereoTargetEyeMask.None;

                // Set camera option of either
                i.nearClipPlane = useVR ? 0.01f : 0.1f;  // prevents light shimmering
                i.allowHDR = !useVR;

                // Set non-VR options
                if (!useVR)
                {
                    i.fieldOfView = 60;
                    i.ResetAspect();
                }
            }
            m_playerCamera.enabled = !useVR;

            //VR_Recenter();
            //m_camera.allowHDR = false;
            Debug.Log("F1: VR Mode " + useVR);
        }
        else Debug.Log("F1: VR Presence = " + hasVR);
    }
    private void VR_Recenter()
    {

        if (useVR)
        {   // VR overrides any change in rotation, needs a moment of deactivation
            foreach (Camera i in arr_camera)
            {
                i.stereoTargetEye = UnityEngine.StereoTargetEyeMask.None;
                i.transform.rotation = i.transform.parent.rotation;
                i.stereoTargetEye = UnityEngine.StereoTargetEyeMask.Both;
            }
        }
        else
            foreach (Camera i in arr_camera)
            {
                i.transform.rotation = i.transform.parent.rotation;
            }

        if (hasVR) Valve.VR.OpenVR.System.ResetSeatedZeroPose();

        Debug.Log("F2: Recenter");
    }
}
