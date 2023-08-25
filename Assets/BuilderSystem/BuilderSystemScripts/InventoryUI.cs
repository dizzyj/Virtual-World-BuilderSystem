using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : NetworkBehaviour
{

    [Header("Inventory canvas")]
    public KeyCode openInventory = KeyCode.V;
    public GameObject inventoryUI;
    public GameObject modifyUi;
    [Space(10)]
    private Player p;
    private BManager b;
    private PrefabHolder prefabHolder;
    [Header("Spawning buttons")]
    
    public Button deleteAll;
    public GameObject buttonPrefab;
    public List<GameObject> createdButtons;
    private NetworkManager nm;
    private void Start()
    {
        nm = FindObjectOfType<NetworkManager>();

        prefabHolder = GetComponent<PrefabHolder>();
        inventoryUI.SetActive(false);
        //modifyUi.SetActive(false);
        deleteAll.onClick.AddListener(UiDeleteAll);
        
    }

    /*onclick of buttons, gets local player and their bManager and then calls
     * BManager.CreatePrefab() with the selected prefab
    */
    public void uiSpawn(GameObject curPrefab)
    {
        p = Player.GetLocalPlayer();
        b = p.GetComponent<BManager>();
        b.CreatePrefab( curPrefab);

    }

    /*gets all prefabs stored in prefabholder.cs (will be converted to database) and
     * loops through them creating a button for each with an onclick listener that will
     * call uiSpawn with the current prefab
     * 
     * Need:
     * pull prefabs from dabase or library rather than script
     * automatically add prefabs to networkmanager
     * fix button layouts
     */
    public void CreateButtons()
    {
        Debug.Log("entered creaateButtons");
        List<GameObject> pf = nm.spawnPrefabs;
        Debug.Log(pf);
        int i = 0;

        pf.ForEach(delegate (GameObject gameObject)
        {
            //Debug.Log("creating button");
            GameObject button = Instantiate(buttonPrefab, inventoryUI.transform);
            createdButtons.Add(button);
            button.transform.SetParent(inventoryUI.transform);
            button.transform.GetComponentInChildren<TMP_Text>().text = gameObject.name;//Changing text
            button.GetComponent<Button>().onClick.AddListener(delegate { uiSpawn(gameObject); });

            RectTransform buttonTrans = button.GetComponent<RectTransform>();
            buttonTrans.anchoredPosition = new Vector2(0, (i*buttonTrans.rect.height)+80 );

            i++;
        });
    }

    //deletes buttons whenever the inventory is closed (in case a prefab is saved during runtime
    private void DeleteButtons()
    {
        //Debug.Log("destroying bttns");
        createdButtons.ForEach(delegate (GameObject button)
        {
            //Debug.Log("destroying " + button);
            if(button != null)
            {
                Destroy(button);

            }
        });
    }

    //calls deleteall from
    public void UiDeleteAll()
    {
        p = Player.GetLocalPlayer();
        b = p.GetComponent<BManager>();
        b.DeleteAll();
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(openInventory))
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
            if (inventoryUI.activeSelf)
            {
                CreateButtons();
            }
            if(!inventoryUI.activeSelf)
            {
                DeleteButtons();
            }
        }
    }
}
