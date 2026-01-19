using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevationExit : MonoBehaviour
{
    public Collider2D[] mountainColliders;
    public Collider2D[] boundaryColliders;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            foreach (var mountain in mountainColliders)
            {
                mountain.enabled = true;
            }

            foreach (var boundary in boundaryColliders)
            {
                boundary.enabled = false;
            }
            // 玩家
            collision.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 5; 
        }

    }
}
