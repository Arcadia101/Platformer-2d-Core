using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<CharacterStats>().TakeDamage(1);
            if (other.GetComponent<CharacterStats>()._currentHealth > 0)
            {
                other.GetComponent<CharacterStats>().Respawn();
            }
        }
        else
        {
            other.GetComponent<CharacterStats>().Death();
        }
    }
}
