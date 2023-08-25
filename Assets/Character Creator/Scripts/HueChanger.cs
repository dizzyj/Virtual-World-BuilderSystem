using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HueChanger : MonoBehaviour
{

    public CharacterCustomization avatar;
    public List<Slider> Sliders;
    public int materialIndex; //skin, lips, nails, eyewhites, iris, teeth, eyebrows
    // public ChangeHairType hair;
    public Material mat;
    private Color baseColor;
    float h, s, v;
    private Player player;


    void Start()
    {
        avatar = CharacterCustomization.Instance;
        if (avatar.skmr == null) return;

        materialIndex = materialIndex % avatar.skmr.materials.Length;
        mat = avatar.skmr.materials[materialIndex];
        baseColor = mat.color;
        // grabbing and setting the default values. 
        SetColor(baseColor);
        
        // if (hair == null)
        // {
        //
        // }
        // else
        // {
        //     if (hair.mat == null) return;
        //     mat = hair.mat;
        //     baseColor = mat.color;
        //     // grabbing and setting the default values. 
        //     SetColor(baseColor);
        // }
    }

    private void Update()
    {
        if (player != Player.GetLocalPlayer())
        {
            player = Player.GetLocalPlayer();
        }
    }

    public void SaveColorJson()
    {
        Dictionary<string, float> colorValues = new Dictionary<string, float>();
        colorValues.Add("hue", h);
        colorValues.Add("saturation", s);
        colorValues.Add("brightness", v);

        string json = JsonConvert.SerializeObject(colorValues, Formatting.Indented);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/color.json", json);
        Debug.Log("wrote color json to: " + Application.persistentDataPath);
        
        player.CmdUpdateColor(json);
    }

    public void LoadColorJson()
    {
        string json = System.IO.File.ReadAllText(Application.persistentDataPath+"/color.json");

        /* uncomment this to apply changes locally. this same code is run by the ClientRPC */
        
        // Dictionary<string, float> jsonData = JsonConvert.DeserializeObject<Dictionary<string, float>>(json);
        // ChangeHue(jsonData["hue"]);
        // ChangeSat(jsonData["saturation"]);
        // ChangeVal(jsonData["brightness"]);

        player.CmdUpdateColor(json);
    }
    

    public void SetUp()
    {
        avatar = CharacterCustomization.Instance;
        if (avatar.skmr == null) return;

        materialIndex = materialIndex % avatar.skmr.materials.Length;
        mat = avatar.skmr.materials[materialIndex];
        baseColor = mat.color;
        // grabbing and setting the default values. 
        SetColor(baseColor);
    }

    public void ChangeHue(float hue)
    {
        h = hue;
        UpdateColor();
    }

    public void ChangeSat(float sat)
    {
        s = sat;
        UpdateColor();
    }

    public void ChangeVal(float value)
    {
        v = value;
        UpdateColor();
    }

    public Color GetColor()
    {
        if (mat == null) return baseColor;
        return mat.color;
        //return Color.HSVToRGB(h, s, v);
    }
    public void SetColor(Color rgb)
    {
        Color.RGBToHSV(rgb, out h, out s, out v);
        ChangeHue(h);
        Sliders[0].value = h;
        ChangeSat(s);
        Sliders[1].value = s;
        ChangeVal(v);
        Sliders[2].value = v;
    }

    private void UpdateColor()
    {
        if (mat == null)
        {
            // if (hair != null)
            // {
            //     mat = hair.mat;
            // }
            if (avatar != null && avatar.skmr != null)
            {
                mat = avatar.skmr.materials[materialIndex];
            }
            return;
        }
        mat.SetColor("_Color", Color.HSVToRGB(h, s, v));
        // if (hair != null && hair.mat == null)
        // {
        //     hair.mat = mat;
        // }
        if (avatar != null && avatar.skmr != null)
        {
            avatar.skmr.materials[materialIndex] = mat;
        }
        
    }
}
