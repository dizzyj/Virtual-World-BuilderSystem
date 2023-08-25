using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactions : MonoBehaviour
{


    //Have a bool for every type of trigger so that you can put this component on a
    //Game Object and check the types if interactions that it has
    public string titleOfInteractions;
    [Header("Emotes")]
    public bool clap;
    public bool wave;
    public bool raiseHand;
    public bool lowerHand;
    [Header("Interactions")]
    public bool sit;
    public bool counselorComputer;
    public List<Interaction> getInteractions()
    {
        GameObject gameObject = this.gameObject;
        
        List<Interaction> InteractionsList = new List<Interaction>();

        //Emotes
        if (clap) { InteractionsList.Add(new Interaction("Clap",gameObject)); }
        if (wave) { InteractionsList.Add(new Interaction("Wave",gameObject)); }
        if (raiseHand) { InteractionsList.Add(new Interaction("Raise Hand",gameObject)); }
        if (lowerHand) { InteractionsList.Add(new Interaction("Lower Hand",gameObject)); }

        //Interactions
        if (sit) { InteractionsList.Add(new Interaction("Sit", gameObject)); }
        if (counselorComputer) { InteractionsList.Add(new Interaction("Counselor Computer", gameObject)); }

        return InteractionsList;
    }

    public string getTitle()
    {
        return titleOfInteractions;
    }

}
