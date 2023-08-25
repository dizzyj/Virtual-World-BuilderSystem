using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.EventSystems;

//This File controls the UI for:
//Interaction list that pops up when you click an object
//The Counselor Menu that pops up when you click on a computer
//The Escape Menu
public class InteractionWheelController : MonoBehaviour
{
    public GameObject pivot;
    public VisualElement escapeMenu;
    private Transform transformToFollow;
    private VisualElement openWindowBase;
    private VisualElement root;

    private VisualElement interactionBase;

    private Camera mainCamera;

    private Button closeButton;

    List<Interaction> iList;

    private GameObject highlight;
    private Material originalMaterial;
    public Material highlightMaterial;
    private RaycastHit raycastHit;

    // Used for player instance management
    private PlayerInstanceManager playerInstanceManager;
    private CVWNetworkManager networkManager;

    void OnEnable()
    {
        mainCamera = Camera.main;
        root = GetComponent<UIDocument>().rootVisualElement;
        interactionBase = root.Q("base");


        transformToFollow = pivot.transform;

        //setPosition();

        closeInteractions();

        closeButton = interactionBase.Q<Button>("Close");
        closeButton.clicked += () => closeInteractions();

        playerInstanceManager = GetComponent<PlayerInstanceManager>();
        networkManager = CVWNetworkManager.singleton;
    }

    private void Update()
    {
        RaycastHit raycastHit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        //Dehighlights the object
        if(highlight != null)
        {
            highlight.GetComponent<MeshRenderer>().material = originalMaterial;
            highlight = null;
        }

        //highlights object if mouse is over and has Selectable tag
        if (Physics.Raycast(ray, out raycastHit, 100f))
        {
            if(raycastHit.transform != null && raycastHit.transform.CompareTag("Interactable")) //If it hit something and that somethign is selectable
            {
                highlight = raycastHit.transform.gameObject;
                if(highlight.GetComponent<MeshRenderer>().material != highlightMaterial) //If the material isn't already highlight
                {
                    originalMaterial = highlight.GetComponent<MeshRenderer>().material; //Save the original material
                    highlight.GetComponent<MeshRenderer>().material = highlightMaterial; //Add the highlight material
                }
            }
        }

        if (Input.GetButtonDown("Cancel"))
        {
            if (closeInteractions() == false)//Menus area already closed
            {
                openEscapeMenu();
            }

        }

        if (Input.GetButtonDown("Fire1") && (root.childCount == 0))
        {
            clicked();
        }
    }

    private void LateUpdate()
    {
        if (root.childCount != 0)
        {
            setPosition();
        }
    }

    private void openEscapeMenu()
    {

    }

    private void clicked()
    {
        //Get pivot of object clicked on
        RaycastHit raycastHit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        GameObject obj;

        //Tries to find an interaction wheel pivot on object clicked
        //Does not check the childrens children and so on so that it
        //doesn't slow down performance
        if (Physics.Raycast(ray, out raycastHit, 100f))
        {
            if(raycastHit.transform != null)
            {
                obj =  raycastHit.transform.gameObject;
                for (int i = 0; (i < obj.transform.childCount) && (obj.transform.childCount < 10); i++)
                {
                    if (obj.transform.GetChild(i).name == "InteractionWheelPivot")
                    {
                        pivot = obj.transform.GetChild(i).gameObject;
                    }
                }
            }
        }

        if((root.childCount == 0) && (pivot != null))
        {
            setUp();
            
        }
    }

    private void setTitle()
    {
        interactionBase.Q<Label>("Title").text = pivot.GetComponent<Interactions>().getTitle();
    }

    private void setPosition()
    {
        transformToFollow = pivot.transform.transform;

        Vector2 newPosition = RuntimePanelUtils.CameraTransformWorldToPanel(openWindowBase.panel, transformToFollow.position, mainCamera);

        openWindowBase.transform.position = new Vector3(newPosition.x - openWindowBase.layout.width / 2, newPosition.y, 0);
    }

    private bool closeInteractions()
    {
        //VisualElement container = interactionBase.Q<VisualElement>("InteractionsContainer");
        //container.Clear();
        if (root.childCount > 0)
        {
            root.Clear();
            pivot = null;
            return false; //Already closed
        }
        else
        {
            root.Clear();
            pivot = null;
            return true;
        }

    }

    private void setUp()
    {
        //Get list of interactions from the pivot
        Interactions interactions = pivot.GetComponent<Interactions>();
        iList = interactions.getInteractions();

        //Check if you need to open the interaction wheel
        switch (iList[0].getType())
        {
            case "Counselor Computer":
                createCounselorMenu();
                break;
            default:
                createWheel(iList);
                break;
        }
    }



    private void getCode()
    {
        //copy code to clipboard
        Debug.Log("Code not written for getting waiting room code");
    }

    private void endSession()
    {
        //end the counseling session by returning the patient to the waiting room
        Debug.Log("Code not written to return patient to waiting room");
    }

    private void letPatientIn(DropdownField selector, List<uint> players)
    {
        uint playerSelected =  players[selector.index];
        // Debug.Log("Letting in " + playerSelected.playerName);
        playerInstanceManager.MovePlayerToMyScene(playerSelected);
    }

    private void createCounselorMenu()
    {
        StartCoroutine(CreateCounselorMenuDelayed());
    }

    IEnumerator CreateCounselorMenuDelayed() {
        VisualTreeAsset counselorMenuTemplate = Resources.Load<VisualTreeAsset>("CounselorMenu");
        VisualElement counselorMenu = counselorMenuTemplate.Instantiate();
        Button letInButton = counselorMenu.Q<Button>("LetIn");
        //Sets this Visual Element as the one that needs to be moved
        openWindowBase = counselorMenu.Q<VisualElement>("base");
        

        root.Add(counselorMenu);

        Button closeButton = counselorMenu.Q<Button>("Close");
        closeButton.clicked += () => closeInteractions();
        Button getCodeButton = counselorMenu.Q<Button>("CopyCode");
        getCodeButton.clicked += () => getCode();
        Button endSessionButton = counselorMenu.Q<Button>("EndSession");
        endSessionButton.clicked += () => endSession();
        DropdownField patientSelector = counselorMenu.Q<DropdownField>("PatientSelector");


        // Get all players in the waiting room
        Counselor counselor = GetComponent<Counselor>();
        counselor.UpdatePlayersInWaitingRoomList();

        while(!counselor.newListLoaded)
            yield return null;
        
        OnlinePlayersData playersInWaitingRoomData = counselor.GetAllPlayersInWaitingRoom();

        letInButton.clicked += () => letPatientIn(patientSelector, playersInWaitingRoomData.playerIds);
        patientSelector.choices = playersInWaitingRoomData.playerNames;
    }

    private void createWheel(List<Interaction> iList)
    {   
        VisualTreeAsset iwTemplate = Resources.Load<VisualTreeAsset>("InteractionWheelTemplate");
        interactionBase = iwTemplate.Instantiate();
        openWindowBase = interactionBase.Q<VisualElement>("base");  //Sets this Visual Element as the one that needs to be moved

        interactionBase.Q<Button>("Close").clicked += () => closeInteractions();

        root.Add(interactionBase);

        setTitle();
        interactionBase.style.display = DisplayStyle.Flex;
        PlayerInteractionController interactionController = GetComponent<PlayerInteractionController>();
        VisualElement container = interactionBase.Q<VisualElement>("InteractionsContainer");
        foreach (Interaction i in iList)
        {
            //Create a new button from the template
            VisualTreeAsset optionTemplate = Resources.Load<VisualTreeAsset>("iwOption");
            VisualElement option = optionTemplate.Instantiate();

            //Add it to the container
            container.Add(option);

            //Change the text and icon
            Button b = option.Q<Button>("OptionButton");
            b.text = i.getType();
            b.clickable.clicked += () => interactionController.processInteraction(i);
            b.clickable.clicked += () => closeInteractions();


        }


        
    }
}
 