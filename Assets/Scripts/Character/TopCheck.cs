using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NameList;
using QxFramework.Core;


public class TopCheck : MonoBehaviour
{
    private Collider2D col2D;
    private int rep;
    private void Awake()
    {
        col2D = transform.parent.GetComponent<Collider2D>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))// (collision.gameObject.layer == Utils.NameToLayer(Layer.Player))
        {
            if (rep == 0)
                Physics2D.IgnoreCollision(collision, col2D, true);//transform.parent.gameObject.layer = Utils.NameToLayer(Layer.EnemyIgnorePlayer);
            rep++;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))// (collision.gameObject.layer == Utils.NameToLayer(Layer.Player))
        {
            rep--;
            if (rep == 0)
                Physics2D.IgnoreCollision(collision, col2D, false);//transform.parent.gameObject.layer = Utils.NameToLayer(Layer.Enemy);
        }
    }
}
