using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabHolder : MonoBehaviour
{
    public List<GameObject> prefabs;

    private void Start()
    {
    }

    public List<GameObject> getPrefabs()
    {
        return prefabs;
    }
}
