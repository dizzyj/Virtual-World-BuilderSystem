using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject inGameButtons;
    [SerializeField] private CVWNetworkManager manager;

    // Update is called once per frame
    void Update()
    {
        if (manager.isNetworkActive) {
            if (!inGameButtons.activeSelf)
                inGameButtons.SetActive(true);
        }

        else
            inGameButtons.SetActive(false);
    }

    private void FixedUpdate()
    {
    }
}
