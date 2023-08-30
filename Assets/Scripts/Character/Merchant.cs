using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Merchant : MonoBehaviour
{
    private void Start()
    {
        List<int> init = new List<int> { 1, 2, 3, 7, 8 };
        for (int i = 0; i < 3; ++i)
        {
            int id = Random.Range(1, init.Count);
            int effectId = init[id];
            init.RemoveAt(id);
            transform.Find(i.ToString()).Find("Trigger").GetComponent<ShopInteractive>().SetItem(effectId);
        }
    }
}
