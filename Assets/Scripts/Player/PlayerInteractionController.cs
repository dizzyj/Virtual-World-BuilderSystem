using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerInteractionController : NetworkBehaviour
{

    // Animator Layer Indices
    private int BASE_LAYER = 0;
    private int ARMS_LAYER = 1;
    private int LEFT_ARM_LAYER = 2;
    private int RIGHT_ARM_LAYER = 3;

    private Player player;
    private PlayerMovement playerMovement;
    private Rigidbody rb;
    private CapsuleCollider playerCollider;
    private Animator animator;
    private NetworkAnimator networkAnimator;
    private Seat currentSeat;

    void Start()
    {
        player = GetComponent<Player>();
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
        networkAnimator = GetComponent<NetworkAnimator>();
    }


    void Update()
    {
        if (!isLocalPlayer) return;

        CheckStopSitting();
    }
    
    public void processInteraction(Interaction interaction) {
        if (!isLocalPlayer) return;

        ResetTriggers();
        switch (interaction.getType())
        {
            case "Clap":
                Clap();
                break;
            case "Wave":
                Wave();
                break;
            case "Raise Hand":
                RaiseHand();
                break;
            case "Lower Hand":
                LowerHand();
                break;
            case "Sit":
                StartSitting(interaction);
                break;
        }
    }

    #region Animation Functions

    #endregion

    // Clap if not clapping
    private void Clap() {
        if (!ArmsStateInfo().IsName("Clapping")) {
            networkAnimator.SetTrigger("ClapTrigger");
        }
    }

    // Wave if not waving
    private void Wave() {
        if (!RightArmStateInfo().IsName("Waving")) {
            networkAnimator.SetTrigger("WaveTrigger");
        }
    }

    // Raise hand if hand not being raised and not currently raised
    private void RaiseHand() {
        if (!RightArmStateInfo().IsName("Raise Hand") && !RightArmStateInfo().IsName("Keep Hand Raised")) {
            networkAnimator.SetTrigger("RaiseHandTrigger");
        }
    }

    // Lower hand if hand is being raised or currently raised
    private void LowerHand() {
        if (RightArmStateInfo().IsName("Raise Hand") || RightArmStateInfo().IsName("Keep Hand Raised")) {
            networkAnimator.SetTrigger("LowerHandTrigger");
        }
    }

    // Initiates sitting with the giving interactable
    private void StartSitting(Interaction interactable) {
        // Get the seat
        currentSeat = interactable.getGameObject().GetComponent<Seat>();

        // If a player is already sitting there, return
        if (currentSeat.IsOccupied()) return;

        // Assign player to seat on server
        CmdAssignPlayerToCurrentSeat(currentSeat);
        
        // Get the seat's user sitting point
        Transform desiredUserTransform = currentSeat.userSittingPoint;

        // Disable player movement and change rotation to around the player
        playerMovement.DisableMovement();
        playerMovement.ChangeToRotateAroundPlayer();
        
        // Disable gravity and collision
        rb.useGravity = false;
        playerCollider.enabled = false;

        // Move player to correct position
        transform.position = desiredUserTransform.position;
        transform.rotation = desiredUserTransform.rotation;

        // Set animator settings
        networkAnimator.SetTrigger("SitTrigger");
        networkAnimator.animator.SetBool("Sitting", true);
    }

    // Stop sitting
    private void StopSitting() {
        // Remove player from seat on server
        CmdRemovePlayerFromCurrentSeat(currentSeat);

        // Re-enable gravity and collision
        playerCollider.enabled = true;
        rb.useGravity = true;

        // Re-enable movement and change rotation back to player rotation
        playerMovement.EnableMovement();
        playerMovement.ChangeToRotatePlayer();

        // Reset the camera
        ResetCamera();

        // Set animator settings
        networkAnimator.ResetTrigger("SitTrigger");
        networkAnimator.animator.SetBool("Sitting", false);
    }

    
    #region Helper Functions

    // Reset all triggers
    private void ResetTriggers() {
        networkAnimator.ResetTrigger("ClapTrigger");
        networkAnimator.ResetTrigger("WaveTrigger");
        networkAnimator.ResetTrigger("RaiseHandTrigger");
        networkAnimator.ResetTrigger("LowerHandTrigger");
        networkAnimator.ResetTrigger("SitTrigger");
    }

    // Check if user wants to stop sitting and then stops sitting if desired
    private void CheckStopSitting() {
        bool currentlySitting = BaseStateInfo().IsName("Stand To Sit") || BaseStateInfo().IsName("Sit Idle");
        bool wantsToMove = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).magnitude > 0;
        
        // If currently sitting and wants to move, stop sitting 
        if (currentlySitting) {
            if (wantsToMove) {
                StopSitting();
            }
        }
    }

    // Reset camera to a local position and rotation of (0, 0, 0)
    private void ResetCamera() {
        Camera.main.transform.localPosition = new Vector3(0, 0, 0);
        Camera.main.transform.localRotation = Quaternion.Euler(0, 0, 0);
    }    
    #endregion

    #region Animator State Helper Functions
    private AnimatorStateInfo BaseStateInfo() {
        return animator.GetCurrentAnimatorStateInfo(BASE_LAYER);
    }

    private AnimatorStateInfo ArmsStateInfo() {
        return animator.GetCurrentAnimatorStateInfo(ARMS_LAYER);
    }

    private AnimatorStateInfo LeftArmStateInfo() {
        return animator.GetCurrentAnimatorStateInfo(LEFT_ARM_LAYER);
    }

    private AnimatorStateInfo RightArmStateInfo() {
        return animator.GetCurrentAnimatorStateInfo(RIGHT_ARM_LAYER);
    }

    #endregion

    #region Server Calls

    [Command]
    private void CmdAssignPlayerToCurrentSeat(Seat seat) {
        seat.AssignPlayerToSeat(player);
    }

    [Command]
    private void CmdRemovePlayerFromCurrentSeat(Seat seat) {
        seat.RemovePlayerFromSeat();
    }

    #endregion
}
