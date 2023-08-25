using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CounselorUIController : MonoBehaviour
{
    private VisualElement root;
    void onEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement.Q("base");
        Button copyCode = root.Q<Button>("CopyCode");
        copyCode.clicked += () => codeClicked();
        Button endSession = root.Q<Button>("EndSession");
        endSession.clicked += () => endClicked();
        Button letIn = root.Q<Button>("LetIn");
        letIn.clicked += () => letInClicked();
        DropdownField patientSelector = root.Q<DropdownField>("PatientSelector");
        populateSelector(patientSelector);
    }

    private void populateSelector(DropdownField selector)
    {
        //Gets the list of patients in the waiting room

        //For test purposes
        List<string> patients = new List<string>();
        patients.Add("Max");
        patients.Add("Ethan");
        patients.Add("Griffin");
        patients.Add("Danny");
        patients.Add("Nick");
        patients.Add("Shane");

        selector.choices = patients;

    }

    private void letInClicked()
    {

    }

    private void codeClicked()
    {

    }

    private void endClicked()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
