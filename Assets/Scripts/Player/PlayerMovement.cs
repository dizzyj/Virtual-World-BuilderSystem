using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private Player player;
    private Camera mainCamera;

    [Header("Movement")]
    public float movementSpeed;
    public float rotationSpeed;
    
    [Header("Camera")]
    public float scrollSpeed = 10f;
    public float defaultFOV = 60f;

    private Rigidbody rb;
    private Vector3 movement; // Used to hold movement from the input
    private bool rotating; // true if rotating, false if not
    private bool rotatePlayer = true;
    private bool canMove = true;


    [Header("Jumping")]
    public float jumpPower;
    public LayerMask groundLayer;
    
    private CapsuleCollider playerCollider;
    private bool grounded;
    private bool jumping;
    
    

    [Header("Animation")]
    public float directionDampening = 0.05f;
    public float rotationDampening = 0.1f;
    Vector3 lastForward;
    
    private Animator animator;

    
    
    // Start is called before the first frame update
    void Start()
    {
        // remember lastForward for animations
        lastForward = transform.forward;

        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<CapsuleCollider>();
        player = GetComponent<Player>();
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        else
        {
            mainCamera.fieldOfView -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
        }
        
        if (CanMove()) {
            GetMovement();
            

            if (IsGrounded()) {
                GetJump();
            }
        }

        Rotate();

        UpdateAnimator();
        
    }

    void FixedUpdate() {
        if (!isLocalPlayer) return;

        if (CanMove()) {
            Move();
        }
    }

    #region Movement

    // Gets and stores player movement. Called from Update
    private void GetMovement() {
        // Get player input and set movement Vector3
        Vector2 movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        movementInput = movementInput.normalized;
        movement = movementInput.x * transform.right * movementSpeed + movementInput.y * transform.forward * movementSpeed;
    }

    // Set the velocity x and z with Vector3 movement. Called from FixedUpdate for proper physics
    private void Move() {
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);
    }

    #endregion

    #region Rotation

    // Rotate the character. 
    private void Rotate() {
        // If RMB ("Fire2") is held, rotate
        if (Input.GetButton("Fire2")) {
            
            // Set mouse to invisible and lock to screen when initiating rotation
            if (!rotating) {
                rotating = true;

                // Disable cursor
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

            }

            // Get rotation from mouse input and rotate player
            float rotateY = Input.GetAxis("Mouse X");

            // If player can rotate, rotate player
            if (rotatePlayer) {
                Vector3 rotation = new Vector3(0f, rotateY, 0f);
                transform.Rotate(rotation * Time.deltaTime * rotationSpeed);
                
                // THE FOLLOWING IS FOR ROTATING THE CAMERA UP AND DOWN

                // float rotateX = Input.GetAxis("Mouse Y");
                // float cameraRotation = rotateX * -rotationSpeed * Time.deltaTime;
                // Camera.main.transform.RotateAround(transform.position, transform.right, cameraRotation);
            }
            // Otherwise, rotate camera around player
            else {
                float cameraHorizontalRotation = rotateY * rotationSpeed * Time.deltaTime;
                mainCamera.transform.RotateAround(transform.position, transform.up, cameraHorizontalRotation);
            }


        }

        // Re-enable mouse when rotation ends
        else if (rotating) {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            rotating = false;
        }
    }
    
    #endregion

    #region Utility Functions

    public void EnableMovement() {
        canMove = true;
    }

    public void DisableMovement() {
        canMove = false;
    }

    public void ChangeToRotateAroundPlayer() {
        rotatePlayer = false;
    }

    public void ChangeToRotatePlayer() {
        rotatePlayer = true;
    }

    public bool CanMove() {
        return canMove;
    }

    private bool RotatingPlayer() {
        return rotatePlayer;
    }

    private bool IsGrounded() {
        grounded = Physics.CheckSphere(transform.position, playerCollider.radius, groundLayer);
        if (jumping && grounded) {
            jumping = false;
        }

        return grounded;
    }

    #endregion

    #region Jumping

    private void GetJump() {
        if (Input.GetButtonDown("Jump") && !jumping) {
            Jump();
        }
    }

    private void Jump() {
        rb.AddForce(Vector3.up * jumpPower);
        jumping = true;
    }


    #endregion

    #region Animator

    void UpdateAnimator() {
        // Turn value so that mouse-rotating the character plays some animation
        // instead of only raw rotating the model.
        float rotation = AnimationDeltaUnclamped(lastForward, transform.forward);
        lastForward = transform.forward;


        // local velocity (based on rotation) for animations
        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);


        animator.SetFloat("DirX", localVelocity.x, directionDampening, Time.deltaTime); // smooth idle<->run transitions
        animator.SetFloat("DirY", localVelocity.y, directionDampening, Time.deltaTime); // smooth idle<->run transitions
        animator.SetFloat("DirZ", localVelocity.z, directionDampening, Time.deltaTime); // smooth idle<->run transitions

        animator.SetFloat("Rotation", rotation, rotationDampening, Time.deltaTime); // smooth turn
        
        animator.SetBool("OnGround", grounded);
    }


    #endregion
    // From CVW:

    // Vector.Angle and Quaternion.FromToRotation and Quaternion.Angle all end
    // up clamping the .eulerAngles.y between 0 and 360, so the first overflow
    // angle from 360->0 would result in a negative value (even though we added
    // something to it), causing a rapid twitch between left and right turn
    // animations.
    //
    // the solution is to use the delta quaternion rotation.
    // when turning by 0.5, it is:
    //   0.5 when turning right (0 + angle)
    //   364.6 when turning left (360 - angle)
    // so if we assume that anything >180 is negative then that works great.
    static float AnimationDeltaUnclamped(Vector3 lastForward, Vector3 currentForward)
    {
        Quaternion rotationDelta = Quaternion.FromToRotation(lastForward, currentForward);
        float turnAngle = rotationDelta.eulerAngles.y;
        return turnAngle >= 180 ? turnAngle - 360 : turnAngle;
    }

    public void ResetCameraHorizontal() {
        Vector3 currentCameraPosition = mainCamera.transform.localPosition;
        mainCamera.transform.localPosition = new Vector3(0, 0, 0);
        mainCamera.transform.localRotation = Quaternion.Euler(0, 0, 0);
        mainCamera.fieldOfView = defaultFOV;
    }
}
