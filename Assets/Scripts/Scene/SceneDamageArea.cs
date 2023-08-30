using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneDamageArea : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent<CharacterBase>(out var cB))
        {
            if(cB is Character)
            {
                cB.TempDamage(transform);
            }
        }
    }
}
