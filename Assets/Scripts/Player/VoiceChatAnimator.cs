using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Dissonance;

public class VoiceChatAnimator : NetworkBehaviour
{
    // Dissonance variables
    private DissonanceComms comms;
    private VoicePlayerState localPlayerVoiceState;

    // Mirror Variables
    private NetworkAnimator networkAnim;

    private int lastTalkingState = -1;
    private int numOfTalkingStates = 2;

    void Start()
    {
        if (!isLocalPlayer) return;
        
        // Get components
        networkAnim = GetComponent<NetworkAnimator>();
        comms = GameObject.Find("DissonanceSetup").GetComponent<DissonanceComms>();
        localPlayerVoiceState = comms.FindPlayer(comms.LocalPlayerName);

        // Set OnStartedSpeaking function
        localPlayerVoiceState.OnStartedSpeaking += player => {
            StartTalking();
        };


        // Set OnStoppedSpeaking function
        localPlayerVoiceState.OnStoppedSpeaking += player => {
            StopTalking();
        };
        

    }

    // Starts user talking animation
    private void StartTalking() {
        if (AnimatorsAreNull()) return;

        networkAnim.SetTrigger("TalkingTrigger");
        networkAnim.animator.SetBool("Talking", true);

        // Set the talking state to a random state
        networkAnim.animator.SetInteger("TalkingState", GetRandomTalkingState());
    }

    // Stops user talking animation
    private void StopTalking() {
        if (AnimatorsAreNull()) return;

        networkAnim.ResetTrigger("TalkingTrigger");
        networkAnim.animator.SetBool("Talking", false);
    }

    // True if either network animator or actual animator are null, false if not
    private bool AnimatorsAreNull() {
        return networkAnim == null || networkAnim.animator == null;
    }

    
    // Get a random integer in the range of 0 to numOfTalkingStates
    private int GetRandomTalkingState() {
        int randTalkingState = Random.Range(0, numOfTalkingStates);
        while (randTalkingState == lastTalkingState) {
            randTalkingState = Random.Range(0, numOfTalkingStates);
        }
        lastTalkingState = randTalkingState;
        return randTalkingState;
    }
}
