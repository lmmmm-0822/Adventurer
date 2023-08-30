using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraChangeMoveArea : MonoBehaviour
{
    [SerializeField]
    private Collider2D moveArea;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
            CameraControl.Instance.ChangeMoveArea(moveArea);
    }
}
