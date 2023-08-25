using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Linq;

//get reference to local player instead of applying in editor
//remove net identity from child object

public class BManager : NetworkBehaviour
{
    public Dictionary<string, int> objectMap;
    //list of objects spawned by local player
    public List<GameObject> spawnedObjects;
    //spawn limit for each player
    public int SPAWN_LIMIT = 100;
    private NetworkManager nm;


    private void Start()
    {
        spawnedObjects = new List<GameObject>();
        nm = FindObjectOfType<NetworkManager>();
        var loadedObjects = Resources.LoadAll("prefabs", typeof(GameObject)).Cast<GameObject>();
        var i = 0;
        objectMap = new Dictionary<string, int>();

        foreach (var go in loadedObjects)
             {
                 Debug.Log(go.name);
            nm.spawnPrefabs.Add(go);
            objectMap.Add(go.name, i);
                i++;
             }
        //Resources.Load<GameObject>("Capsule");
        nm.spawnPrefabs.ForEach(delegate (GameObject ob)
        {
            Debug.Log(ob.name);

        });

        //objectMap.Add("Cube", 0);
        //objectMap.Add("Sphere", 1);
        //objectMap.Add("Capsule", 2);
        //objectMap.Add("Cylinder", 3);

    }

    private void FixedUpdate()
    {
        if(!isLocalPlayer)
        {
            return;
        }
        
    }


    /*
     * called from the inventory buttons, is passed a gameobject checks whether the named prefab exists on the network
     * if it does, the index that is mapped so that the dictionary is in the same order as network spawnable prefabs
     * checks the dictionary and passes the index to CmdSpawnObj (you cannot pass gameobjects to commands)
     * 
     * need:
     * remove dictionary and map directly from networkmanager
    */
    public void CreatePrefab( GameObject curPrefab)
    {
        int prefabNum = -1;
        if (objectMap.ContainsKey(curPrefab.name))
        {
            
            prefabNum = objectMap[curPrefab.name];
            Debug.Log(curPrefab.name + "exist at" + prefabNum);
        }

        if(prefabNum>=0 )
        {
            CmdSpawnObj(prefabNum);

        }
    }

    //calls server command which deletes all objects belonging to local player
    public void DeleteAll()
    {
        CmdDeleteObjects();
    }

    //spawns the prefab that correlates to prefabIndex on networkmanager
    //if prefab does not exist 'null prefab' is logged
    //all objects are added to a list of objects spawned by the current player
    [Command(requiresAuthority = false)]
    void CmdSpawnObj(int prefabIndex)
    {
        GameObject curPrefab = nm.spawnPrefabs[prefabIndex];
        Debug.Log(curPrefab.name);

        if (curPrefab != null && spawnedObjects.Count < SPAWN_LIMIT)
        {

            Vector3 spawnPos = transform.position + transform.forward * 3;
            Quaternion rot = Quaternion.identity;//transform.rotation;
            GameObject go = Instantiate(curPrefab, spawnPos, rot);
            
            NetworkServer.Spawn(go);
            if(go.GetComponent<MeshRenderer>() != null)
            {
                go.GetComponent<MeshRenderer>().enabled = true;

            }
            foreach(Transform child in go.transform)
            {
                if(child.gameObject.GetComponent<MeshRenderer>() != null)
                {
                    child.gameObject.GetComponent<MeshRenderer>().enabled = true;
                }
            }
            spawnedObjects.Add(go);
            NetworkIdentity netIdentity = go.GetComponent<NetworkIdentity>();
            netIdentity.AssignClientAuthority(connectionToClient);
            ModifiableObject modObj = go.GetComponent<ModifiableObject>();
            modObj.owner = Player.GetLocalPlayer();
            

        }
        else
        {
            Debug.Log("null prefab");
           
        }
    }

    //deletes all objects spawned by local player
    [Command]
    void CmdDeleteObjects()
    {
        spawnedObjects.ForEach(delegate (GameObject obj)
        {
            NetworkServer.Destroy(obj);
        });
    }
}
