using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

// This is the main object script as of 6/6/2023

public class ModifiableObject : NetworkBehaviour
{
    public Player owner;
    private Player player;
    public Renderer render;
    public Color savedColor = Color.white;
    public Color groupColor = Color.green;
    public Color EditColor = Color.red;
    public GameObject prefab;
    public bool isSelected;
    public bool shrinkMode = false;
    public GameObject spawnCanvas;
    public GameObject modifyCanvas;
    public Component outline;
    // public NetworkTransform nt;
    // public int timesClicked;
    // public bool modifyMode = false;
    // public bool isInteracted;
    // public bool isRed;
    public NetworkIdentity ni;

    // [HideInInspector]
    public Camera cammy;

    public Vector3 screenPoint;
    public Vector3 offset;
    public bool isGroup = false;
    public bool groupHighlight = false;
    public bool editing = false;

    public void Start()
    {
        // isRed = false;
        spawnCanvas = GameObject.Find("Inventory");

        player = Player.GetLocalPlayer();

        //modifyCanvas.SetActive(false);
        //Debug.Log(spawnCanvas + " " + modifyCanvas);
        // isSelected = false;
        render = GetComponent<Renderer>();
        ni = GetComponent<NetworkIdentity>();
        // outline = GetComponent<Outline>().gameObject;

        prefab = this.gameObject;

        //isInteracted = false;
        // render.material.color = Color.white;
        // savedColor = Color.white;
    }

    public void Update()
    {
        cammy = Camera.main;
        //shrinkmode
        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            shrinkMode = !shrinkMode;
        }

        //rotation
        if(isSelected && Input.GetKeyDown(KeyCode.E))
        {
            if (!isGroup)
            {
                prefab.transform.Rotate(0, 45, 0);

            }
            else
            {
                prefab.transform.parent.Rotate(0, 45, 0);
            }
        }else if(isSelected && Input.GetKeyDown(KeyCode.Q))
        {
            
            if (!isGroup)
            {
                prefab.transform.Rotate(0, -45, 0);

            }
            else
            {
                prefab.transform.parent.Rotate(0, -45, 0);
            }
        }


        // NEED BETTER KEY BINDS -- NEED TO NOT ALLOW VALUES TO GO LESS THAN 0
        else if(isSelected && Input.GetKeyDown(KeyCode.F))
        {   //scale x
            if(shrinkMode){
                if(!isGroup)
                {
                    Vector3 scaler = new Vector3(prefab.transform.localScale.x - 0.1f, prefab.transform.localScale.y, prefab.transform.localScale.z); 
                    prefab.transform.localScale = scaler;

                }
                else
                {
                    Vector3 scaler = new Vector3(prefab.transform.parent.localScale.x - 0.1f, prefab.transform.parent.localScale.y, prefab.transform.parent.localScale.z);
                    prefab.transform.parent.localScale = scaler;
                }
            }else{
                
                if (!isGroup)
                {
                    Vector3 scaler = new Vector3(prefab.transform.localScale.x + 0.1f, prefab.transform.localScale.y, prefab.transform.localScale.z);
                    prefab.transform.localScale = scaler;

                }
                else
                {
                    Vector3 scaler = new Vector3(prefab.transform.parent.localScale.x + 0.1f, prefab.transform.parent.localScale.y, prefab.transform.parent.localScale.z);
                    prefab.transform.parent.localScale = scaler;
                }
            }
        }
        else if(isSelected && Input.GetKeyDown(KeyCode.R))
        {
            //scale y
            if(shrinkMode){
                
                if (!isGroup)
                {
                    Vector3 scaler = new Vector3(prefab.transform.localScale.x, prefab.transform.localScale.y - 0.1f, prefab.transform.localScale.z);
                    prefab.transform.localScale = scaler;

                }
                else
                {
                    Vector3 scaler = new Vector3(prefab.transform.parent.localScale.x, prefab.transform.parent.localScale.y - 0.1f, prefab.transform.parent.localScale.z);
                    prefab.transform.parent.localScale = scaler;
                }
            }
            else{
                
                if (!isGroup)
                {
                    Vector3 scaler = new Vector3(prefab.transform.localScale.x, prefab.transform.localScale.y + 0.1f, prefab.transform.localScale.z);
                    prefab.transform.localScale = scaler;

                }
                else
                {
                    Vector3 scaler = new Vector3(prefab.transform.parent.localScale.x, prefab.transform.parent.localScale.y + 0.1f, prefab.transform.parent.localScale.z);
                    prefab.transform.parent.localScale = scaler;
                }

            }
        }
        else if(isSelected && Input.GetKeyDown(KeyCode.G))
        {   //scale z
            if(shrinkMode){
                
                if (!isGroup)
                {
                    Vector3 scaler = new Vector3(prefab.transform.localScale.x, prefab.transform.localScale.y, prefab.transform.localScale.z - 0.1f);
                    prefab.transform.localScale = scaler;

                }
                else
                {
                    Vector3 scaler = new Vector3(prefab.transform.parent.localScale.x, prefab.transform.parent.localScale.y, prefab.transform.parent.localScale.z - 0.1f);
                    prefab.transform.parent.localScale = scaler;
                }
            }
            else{
                if (!isGroup)
                {
                    Vector3 scaler = new Vector3(prefab.transform.localScale.x, prefab.transform.localScale.y, prefab.transform.localScale.z + 0.1f);
                    prefab.transform.localScale = scaler;

                }
                else
                {
                    Vector3 scaler = new Vector3(prefab.transform.parent.localScale.x, prefab.transform.parent.localScale.y, prefab.transform.parent.localScale.z + 0.1f);
                    prefab.transform.parent.localScale = scaler;
                }

            }
        }
        else if(isSelected && Input.GetKeyDown(KeyCode.T))
        {   //scale all
            if(shrinkMode){
                if (!isGroup)
                {
                    Vector3 scaler = new Vector3(prefab.transform.localScale.x - 0.1f, prefab.transform.localScale.y - 0.1f, prefab.transform.localScale.z - 0.1f);
                    prefab.transform.localScale = scaler;

                }
                else
                {
                    Vector3 scaler = new Vector3(prefab.transform.parent.localScale.x-0.1f, prefab.transform.parent.localScale.y- 0.1f, prefab.transform.parent.localScale.z - 0.1f);
                    prefab.transform.parent.localScale = scaler;
                }
            }
            else{
                if (!isGroup)
                {
                    Vector3 scaler = new Vector3(prefab.transform.localScale.x + 0.1f, prefab.transform.localScale.y + 0.1f, prefab.transform.localScale.z + 0.1f);
                    prefab.transform.localScale = scaler;

                }
                else
                {
                    Vector3 scaler = new Vector3(prefab.transform.parent.localScale.x + 0.1f, prefab.transform.parent.localScale.y + 0.1f, prefab.transform.parent.localScale.z + 0.1f);
                    prefab.transform.parent.localScale = scaler;
                }
            }
        }
        // else if(isSelected && )

    }


    // public Color savedColor;
    private void OnMouseOver()
    {
        //highlight all grouped objects if in group
        if (isGroup)
        {
            foreach (Transform t in transform.parent.transform)
            {
                t.GetComponent<Renderer>().material.color = Color.cyan;
            }
        }
        if(!groupHighlight && !editing)
        {

        render.material.color = Color.cyan;
        }
        player.GetComponent<PrefabBuild>().target = gameObject;
        //Debug.Log("press z to delete");
        if (Input.GetKeyDown(KeyCode.Z) && isSelected)
        {
            if(owner == Player.GetLocalPlayer())
            {
                //Debug.Log(true);
                CmdDeleteObj();

            }
        }

        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            isSelected = !isSelected;
        }
    }


    public void OnMouseDown()
    {
        // joined world yet? (not character selection?)
        // not over UI? (avoid targeting through windows)
        // and in a state where local player can select things?
        if (Player.GetLocalPlayer() != null &&
            !Utils.IsCursorOverUserInterface()) //from Utils.cs which contains useful functions
        {

            // set indicator in any case
            // (not just the first time, because we might have clicked on the
            //  ground in the mean time. always set it when selecting.)
            //Player.GetLocalPlayer().indicator.SetViaParent(transform);
            Debug.Log(" initial clicked");
            // isSelected = true;
            //Debug.Log(" initial clicked");
            //spawnCanvas.SetActive(false);
            //modifyCanvas.SetActive(true);

            screenPoint = cammy.WorldToScreenPoint(prefab.transform.position);     // get raycast from users cursor
            offset = transform.position - cammy.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));


        }
    }

    
    public float mouseX;
    public float mouseY;
    public float mouseSpeedMultiplier = 8.0f;
    public Vector3 curPosition;

    public void OnMouseDrag(){
        if (owner == Player.GetLocalPlayer())
            {
                mouseX = Input.GetAxis("Mouse X") * mouseSpeedMultiplier;      
                mouseY = Input.GetAxis("Mouse Y") * mouseSpeedMultiplier;        

                // Debug.Log("clicked");
                Vector3 curScreenpoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
                curPosition = cammy.ScreenToWorldPoint(curScreenpoint) + offset;
                //prefab.transform.position = curPosition;
                // CmdMoveObject(curPosition);
                //Debug.Log(true);
            if (isGroup)
            {
                transform.parent.position = curPosition;

            }
            else
            {
                transform.position = curPosition;

            }
            // CmdMoveObject(curPosition);
            //Debug.Log(true);

        }
    }

    public void OnMouseExit()
    {
        //highlight all grouped objects if in group
        if (isGroup)
        {
            if (!isSelected)
            {
                foreach (Transform t in transform.parent.transform)
                {
                    t.GetComponent<Renderer>().material.color = savedColor;
                }
            }

        }
        else
        {
            if (!isSelected && ! editing)
            {
                render.material.color = savedColor;
            }

            if (groupHighlight)
            {
                render.material.color = groupColor;
            }
        }
        
        player.GetComponent<PrefabBuild>().target = null;
        
    }

    [Command(requiresAuthority =false)]
    void CmdDeleteObj()
    {
        NetworkServer.Destroy(this.gameObject);
    }

    [Command(requiresAuthority = false)]
    void CmdMoveObject(Vector3 curPosition)
    {
        RpcMoveObject(curPosition);
    }

    [ClientRpc]
    void RpcMoveObject(Vector3 curPos)
    {
        prefab.transform.position = curPos;        // set objects position to the position calculated
    }
}
