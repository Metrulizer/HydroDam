using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerForces : MonoBehaviour
{
    Rigidbody _rb;
    /*private float distToGround;*/
    public float magnitude = 10f;
    private float tl_Forward;     // Z axis
    private float rt_Horizontal;   // Y axis roll
    private float tl_Depth;        // Y axis translate
    private Vector3 movement;
    //private float mouseX;
    //private float mouseY;
    //public float MouseSensitivity = 180f;
    // TODO replace air with water
    public float drag_xz = 2f;

    //private Collider my_collider;
    private bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        // get the distance to ground
        /*distToGround = GetComponent<Collider>().bounds.extents.y;*/
        //my_collider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        tl_Forward = Input.GetAxis("Vertical");
        rt_Horizontal = Input.GetAxis("Horizontal");

        tl_Depth = Input.GetKey(KeyCode.Space) ? 1 :
            Input.GetKey(KeyCode.LeftControl) ? -1 : 0;

        //movement = new Vector3(moveHorizontal, 0, moveVertical);
        movement = new Vector3(0, tl_Depth, tl_Forward);

        _rb.AddRelativeForce(movement * magnitude * Time.fixedDeltaTime);
        transform.Rotate(new Vector3(0, rt_Horizontal, 0));

        //https://answers.unity.com/questions/49001/how-is-drag-applied-to-force.html
        // TODO replace air with water
        Vector3 velocity_y = new Vector3(0, _rb.velocity.y, 0);
        Vector3 velocity_xz = _rb.velocity - velocity_y;

        float multiplier = 1.0f - drag_xz * Time.fixedDeltaTime;
        if (multiplier < 0.0f) multiplier = 0.0f;
        velocity_xz = multiplier * velocity_xz;
        _rb.velocity = velocity_xz + velocity_y;
    }
    ////////////////////////////////////////////////////////////////////////
    void Update()
    {
        ////////////////////////////////////////////////////////////////////////

        // Get Mouse Input
        //mouseX = Input.GetAxis("Mouse X") * MouseSensitivity * Time.deltaTime;
        //mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.deltaTime;

        // Set Transformation: yaw on body but pitch only on head
        //transform.Rotate(0, mouseX, 0);
        //transform.Find("[CameraRig]").Find("Camera").Rotate(mouseY, mouseX, 0); 
/*        Vector3 relativePos = new Vector3(mouseY, mouseX, 0);
        Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);*/
        //transform.Find("[CameraRig]").Find("Camera").rotation *= Quaternion.Euler(mouseY, mouseX, 0);

        if (Input.GetKeyDown(KeyCode.Space))
            if(isGrounded)
                _rb.AddRelativeForce(new Vector3(0, 1000, 0));
    }

    //https://answers.unity.com/questions/1411531/how-to-jump-on-only-ground-colliders.html
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Terrain"))
        {
            isGrounded = true;
        }        
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Terrain"))
        {
            isGrounded = false;
        }
    }

    /*    bool IsGrounded()
        {
            return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.2f);
        }*/

}
