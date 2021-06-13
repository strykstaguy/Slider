using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public GameObject item;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        item.GetComponent<Item>().TriggerCutscene(item);
    }
}
