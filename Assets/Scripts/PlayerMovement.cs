using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 6;
    
    private GameObject cameraRotationAxis;
    private GameObject playerCamera;
    private CharacterController characterController;

    void Awake(){
        cameraRotationAxis = GameObject.Find("Camera Rotation Axis");
        playerCamera = GameObject.Find("Player Camera");
        characterController = GetComponent<CharacterController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    private void Move(){
        float hInput = Input.GetAxisRaw("Horizontal") * speed;
        float vInput = Input.GetAxisRaw("Vertical") * speed;

        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);

        Vector3 rightMovement = playerCamera.transform.right * hInput;
        Vector3 forwardMovement = forward * vInput;

        characterController.SimpleMove(Vector3.Normalize(forwardMovement + rightMovement) * speed);
        
        bool rotateLeft = false;
        bool rotateRight = false;
        if (Input.GetKey("q")){
            rotateLeft = true;
        }
        if (Input.GetKey("e")){
            rotateRight = true;
        }
        if (rotateLeft ^ rotateRight){
            if (rotateLeft){
                cameraRotationAxis.transform.Rotate(0,-80f * Time.deltaTime,0);
            }
            if (rotateRight){
                cameraRotationAxis.transform.Rotate(0,80f * Time.deltaTime,0);
            }
        }
    }
}
