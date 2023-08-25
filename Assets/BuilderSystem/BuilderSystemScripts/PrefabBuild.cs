using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Mirror;

public class PrefabBuild : NetworkBehaviour
{
    //list of objects
    public GameObject prefabUI;
    public GameObject prefabParent;
    public GameObject target;
    public GameObject targetEdit;
    List<GameObject> prefabBuilderList = new List<GameObject>();
    List<GameObject> prefabInventory = new List<GameObject>();
    public bool buildermode = false;
    public bool editing = false;
    int prefabInventoryMax = 3;
    int maxlistsize = 10;
    private Player player;
    BManager bm;
    private NetworkManager nm;
    private void Start()
    {
        player = Player.GetLocalPlayer();
        bm = player.GetComponent<BManager>();
        nm = FindObjectOfType<NetworkManager>();
    }
    private void FixedUpdate()
    {
        if (buildermode)
        {
            if(Input.GetMouseButtonDown(0)&& target != null)
            {
                //mouse over target
                //click target, if target is not in list add, if in list remove. 
                if (!prefabBuilderList.Contains(target))
                {
                    if (prefabBuilderList.Count <= maxlistsize)
                    {
                        prefabBuilderList.Add(target);
                        //if in list highlight
                        target.GetComponent<ModifiableObject>().groupHighlight = true;
                        target.GetComponent<Renderer>().material.color = target.GetComponent<ModifiableObject>().groupColor;
                    }
                }
                else
                {
                    prefabBuilderList.Remove(target);
                    target.GetComponent<ModifiableObject>().groupHighlight = false;
                }
            }
        }
        else
        {   
            //clear highlight
            //clear list
        }
        if (editing)
        {
            if (Input.GetMouseButtonDown(0) && target != null)
            {
                // set edit bool
                // check if target is a prefab parent
                if (target.transform.parent != null && target.transform.parent.gameObject.GetComponent<PrefabParent>() != null)
                {
                    editing = false;
                    targetEdit = target.transform.parent.gameObject;
                    foreach (Transform child in targetEdit.transform)
                    {
                        // itterate through children set to movable
                        child.GetComponent<ModifiableObject>().isGroup = false;
                        // color children red
                        child.GetComponent<Renderer>().material.color = child.GetComponent<ModifiableObject>().EditColor;
                        child.GetComponent<ModifiableObject>().editing = true;
                    }
                    prefabUI.GetComponent<groupbutton>().SaveEditButton.gameObject.SetActive(true);
                    prefabUI.GetComponent<groupbutton>().EditButton.gameObject.SetActive(false);
                }
                else
                {
                    print("Object is not a saved group");
                }
            }
        }
    }
    public void SavePrefab()
    {
        prefabUI = GameObject.FindGameObjectWithTag("BuilderUI");
        //check finish button
        buildermode = false;
        //do stuff
        //create empty game object and set all objects in list to child of empty object
        prefabParent = new GameObject("prefab");
        prefabParent.AddComponent<PrefabParent>();
        NetworkIdentity ni = prefabParent.AddComponent<NetworkIdentity>();
        NetworkTransform ntp = prefabParent.AddComponent<NetworkTransform>();
        //ni.AssignClientAuthority(connectionToClient);
        ntp.clientAuthority = true;
        GameObject ParentObj = Instantiate(prefabParent);
        ParentObj.name = "prefab";
        NetworkServer.Spawn(ParentObj);
        GameObject.Destroy(prefabParent);
        for (int i= 0; i < prefabBuilderList.Count; i++)
        {
            if(i == 0)
            {
                ParentObj.transform.position = prefabBuilderList[i].transform.position;
            }
            //clear highlight
            prefabBuilderList[i].GetComponent<ModifiableObject>().groupHighlight = false;
            prefabBuilderList[i].GetComponent<ModifiableObject>().isGroup = true;
            prefabBuilderList[i].GetComponent<Renderer>().material.color = prefabBuilderList[i].GetComponent<ModifiableObject>().savedColor;
            //set parent
            prefabBuilderList[i].transform.SetParent(ParentObj.transform);
            //remove network identity and transform
            //Destroy(prefabBuilderList[i].GetComponent<NetworkIdentity>());
            //add network child to parent

            //CMDMakeTransformChild(ParentObj, prefabBuilderList[i]);
        }
        prefabBuilderList.Clear();
        //add to resources folder

        bool taken = true;
        string localPath = "";
        int j = 0;
        while (taken)
        {
            localPath = "prefabs/" + prefabParent.name + j.ToString();
            
            if(Resources.Load(localPath)!=null)
            {
                Debug.Log("resource found");
                    j++;
            }
            else
            {
                Debug.Log("resource not found");
                break;
            }
           
            
        }
        bool prefabSuccess;
        ParentObj.AddComponent<NetworkIdentity>();
        ParentObj.AddComponent<ModifiableObject>();
        string savePath = "Assets/Resources/prefabs/";
        Debug.Log(savePath + ParentObj.name + ".prefab");
        PrefabUtility.SaveAsPrefabAsset(ParentObj, savePath+ ParentObj.name+ j.ToString()+ ".prefab", out prefabSuccess);
        if (prefabSuccess)
        {
            Debug.Log("prefab saved successful");
        } else
        {
            Debug.Log("couldnt save prefab");
        }
        if(prefabInventory.Count <= prefabInventoryMax ) 
        {
            prefabInventory.Add(ParentObj);
            switch (prefabInventory.Count)
            {
                case 1:
                    prefabUI.GetComponent<groupbutton>().Prefab1.gameObject.SetActive(true);
                    break;
                case 2:
                    prefabUI.GetComponent<groupbutton>().Prefab2.gameObject.SetActive(true);
                    break;
            }   
        }
        else
        {
            print("Too many prefabs in inventory");
        }
    }
    public void SaveEditPrefab()
    {
        prefabUI.GetComponent<groupbutton>().SaveEditButton.gameObject.SetActive(false);
        prefabUI.GetComponent<groupbutton>().EditButton.gameObject.SetActive(true);
        //targetEdit.transform.position = targetEdit.transform.GetChild(0).position; // just so grouped moving isn't too strange
        foreach (Transform child in targetEdit.transform)
        {
            // itterate through children set to movable
            child.GetComponent<ModifiableObject>().isGroup = true;
            child.GetComponent<ModifiableObject>().editing = true;
            // color children red
            child.GetComponent<Renderer>().material.color = child.GetComponent<ModifiableObject>().savedColor;
        }
    }
    public void EditPrefab()
    {
        prefabUI = GameObject.FindGameObjectWithTag("BuilderUI");
        editing = true;
    }
    [Command(requiresAuthority = false)]
    void CMDMakeTransformChild(GameObject parent, GameObject go)
    {
        NetworkTransformChild ntc = parent.AddComponent<NetworkTransformChild>();
        ntc.target = go.transform;
    }
}
