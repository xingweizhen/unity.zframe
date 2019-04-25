using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    private void Start()
    {
        if (AssetsMgr.Instance == null) {
            var prefab = Resources.Load("AssetsMgr", typeof(GameObject)) as GameObject;
            Instantiate(prefab);
        }
    }
}
