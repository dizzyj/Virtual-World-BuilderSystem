using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCustomization : Singleton<CharacterCustomization>
{
    public GameObject target;

    public List<BlendshapeSlider> sliders;

    public GameObject characterUI;

    public HueChanger HueChanger;

    private CharacterCustomization()
    {
        
    }

    private bool isEditing = false;

    public SkinnedMeshRenderer skmr;
    private Mesh mesh;
    private Player player;
    private PlayerMovement playerMovement;
    private bool playerLoaded = false;

    private Dictionary<string, BlendShape> blendShapeData = new Dictionary<string, BlendShape>();


    private void Update()
    {
        if (target == null)
        {
            Initialize();
        }
        
        if (!playerLoaded) return;

        if (Input.GetKeyDown(KeyCode.K))
        {
            characterUI.SetActive(!characterUI.activeInHierarchy);
            isEditing = characterUI.activeInHierarchy;
                
            switch (isEditing)
            {
                case true:
                    playerMovement.DisableMovement();
                    playerMovement.ChangeToRotateAroundPlayer();
                    break;
                case false:
                    playerMovement.EnableMovement();
                    playerMovement.ResetCameraHorizontal();
                    playerMovement.ChangeToRotatePlayer();
                    break;
            }
        }


        

    }

    private void Start()
    {
        Initialize();
    }

    #region Public Functions
    
    public void Initialize()
    {
        if (Player.GetLocalPlayer() == null)
        {
            playerLoaded = false;
            return;
        }
        player = Player.GetLocalPlayer();
        target = player.gameObject;
        playerMovement = target.GetComponentInChildren<PlayerMovement>();
        
        skmr = GetSkinnedMeshRenderer();
        mesh = skmr.sharedMesh;

        playerLoaded = true;
        
        // setup data
        ParseBlendShapesToDictionary();
    }

    public void ChangeBlendshapeValue(string blendshapeName, float value)
    {
        if (!blendShapeData.ContainsKey(blendshapeName))
        {
            Debug.LogError("Blendshape " + blendshapeName + " does not exist");
            return;
        }

        value = Mathf.Clamp(value, -150, 150);

        BlendShape blendShape = blendShapeData[blendshapeName];
        if (target == Player.GetLocalPlayer().gameObject)
        {
            blendShape.currentValue = value;
        }
        
        skmr.SetBlendShapeWeight(blendShape.index, value);
        
        // May need to adjust Clothing...
    }
    
    public Dictionary<string, BlendShape> GetBlendShapes()
    {
        return blendShapeData;
    }

    public string GetBlendShapeJson()
    {
        string json = JsonConvert.SerializeObject(blendShapeData, Formatting.Indented);
        // Debug.Log("Current Blendshape JSON = " + json);
        
        return json;
    }

    public void SaveBlendShapeJson()
    {
        string json = GetBlendShapeJson();
        
        // TODO: Replace with write/update to DB
        System.IO.File.WriteAllText(Application.persistentDataPath + "/blendshapes.json", json);
        
        Debug.Log("wrote blendshape json to: " + Application.persistentDataPath);
        
        player.CmdUpdateBlendShapes(json);
    }

    public void LoadBlendShapeJson()
    {
        // TODO: replace with read from DB
        string json = System.IO.File.ReadAllText(Application.persistentDataPath+"/blendshapes.json");

        Dictionary<string, BlendShape> jsonData = JsonConvert.DeserializeObject<Dictionary<string, BlendShape>>(json);

        foreach (KeyValuePair<string,BlendShape> keyValuePair in jsonData)
        {
            ChangeBlendshapeValue(keyValuePair.Key, keyValuePair.Value.currentValue);
        }
        
        player.CmdUpdateBlendShapes(json);
        
        updateSliders();
    }
    

    #endregion

    #region Private Functions

    private SkinnedMeshRenderer GetSkinnedMeshRenderer()
    {
        SkinnedMeshRenderer _skmr = target.GetComponentInChildren<SkinnedMeshRenderer>();

        return _skmr;
    }

    private void ParseBlendShapesToDictionary()
    {
        List<string> blendshapeNames =
            Enumerable.Range(0, mesh.blendShapeCount).Select(x => mesh.GetBlendShapeName(x)).ToList();

        foreach (string shapeName in blendshapeNames)
        {
            int shapeIndex = mesh.GetBlendShapeIndex(shapeName);

            if (blendShapeData.ContainsKey(shapeName)) {
                // Debug.Log(shapeName + " already exists within the Dictionary!");
            }
            else if (shapeIndex > -1)
            { 
                blendShapeData.Add(shapeName, new BlendShape(shapeIndex));
            }
        }
    }

    private void updateSliders()
    {
        foreach (BlendshapeSlider blendshapeSlider in sliders)
        {
            foreach (KeyValuePair<string,BlendShape> blendShape in blendShapeData)
            {
                if (blendshapeSlider.blendShapeNames.Contains(blendShape.Key))
                {
                    blendshapeSlider.GetComponentInChildren<Slider>().value = blendShape.Value.currentValue;
                }
            }
        }
    }

    #endregion
    
    
}
