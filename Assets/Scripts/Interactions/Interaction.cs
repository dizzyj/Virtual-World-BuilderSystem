using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Interaction
{
    private string InteractionType;
    private GameObject gameObject;
    private List<GameObject> parameters;


    //Assign type when creating Interaction
    public Interaction(string type, GameObject obj)
    {
        InteractionType = type;
        gameObject = obj;
    }

    public string getType()
    {
        return InteractionType;
    }

    public GameObject getGameObject()
    {
        return gameObject;
    }

    public void SetParameters(List<GameObject> parameters) {
        this.parameters = parameters;
    }

    public List<GameObject> GetParameters(){
        return parameters;
    }
}
