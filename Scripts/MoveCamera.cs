using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    // //getting the camera component
    // private Camera cam;
    // //getting the parent of the camera
    // public Transform cam_parent;
    // //the x and y axis values of the controller or the WSAD
    // float horizontal, vertical;
    // public float Cam_speed;
    // private Quaternion m_camRot, m_Cam_parentRot;
    // [SerializeField]
    // [Range(0.1f, 9.9f)]
    // private float MouseXSens = 2f;
    // [SerializeField]
    // [Range(0.1f, 9.9f)]
    // private float MouseYSens = 2f;
    // // Use this for initialization
    // [SerializeField]
    // private bool HideTheCursor;
    // void Start()
    // {
    //     cam = GetComponent<Camera>();
    //     m_camRot = transform.localRotation;
    //     m_Cam_parentRot = cam_parent.localRotation;
    //     if (HideTheCursor)
    //     {
    //         Cursor.lockState = CursorLockMode.Locked;
    //         Cursor.visible = false;
    //     }
    // }
    // 
    // // Update is called once per frame
    // void Update()
    // {
    //     if (Input.GetButton("Fire2"))
    //     {
    //         Cursor.lockState = CursorLockMode.Locked;
    //         SetRotation();
    //         MoveThecamera();
    //     }
    //     else if (Input.GetButtonUp("Fire2"))
    //     {
    //         Cursor.lockState = CursorLockMode.None;
    //         Cursor.visible = true;
    //     }
    // 
    //     // if(Input.GetKeyDown(KeyCode.Space))
    //     // {
    //     //     cam_parent.position = new Vector3(0f, 4.5f, -4.75f);
    //     //     cam_parent.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
    //     //     cam.transform.rotation = Quaternion.Euler(new Vector3(35f, 0f, 0f));
    //     // }
    // }
    // 
    // //For setting up the rotation of camera with respect to the mouse position
    // void SetRotation()
    // {
    //     //multiplying the current rotation with the input axis values
    //     m_camRot = m_camRot * Quaternion.Euler(-Input.GetAxis("Mouse Y") * MouseXSens, 0f, 0f);
    //     m_Cam_parentRot = m_Cam_parentRot * Quaternion.Euler(0f, Input.GetAxis("Mouse X") * MouseYSens, 0f);
    //     //setting the localRotation values to the desired rotation values
    //     transform.localRotation = m_camRot;
    //     cam_parent.localRotation = m_Cam_parentRot;
    // }
    // void MoveThecamera()
    // {
    //     //taking horizpontal input into the horizontal variable
    //     horizontal = Input.GetAxis("Horizontal");
    //     //changing the parent position based on the camera’s transform.right
    //     //transform.right also consider rotation too
    //     //so that it can move in the ‘right’ direction
    //     cam_parent.transform.position += cam.transform.right * horizontal * Cam_speed * Time.deltaTime;
    //     //taking vertical axis into vertical variable
    //     vertical = Input.GetAxis("Vertical");
    //     //changing the parent position based on the camera’s        transform.forward
    //     //transform is also 0,0,1 but it also consider rotation too
    //     //so that it can move in the forward direction considering the  rotation too
    //     cam_parent.transform.position += cam.transform.forward * vertical * Cam_speed * Time.deltaTime;
    // }
}