using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlendshapeSlider : MonoBehaviour
{
    public string[] blendShapeNames;
    private Slider slider;

    private void Start()
    {
        slider = GetComponent<Slider>();
        
        slider.onValueChanged.AddListener(value =>
        {
            foreach (string shapeName in blendShapeNames)
            {
                CharacterCustomization.Instance.ChangeBlendshapeValue(shapeName, value);
            }
        });
    }
}
