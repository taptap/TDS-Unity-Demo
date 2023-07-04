using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class caneraControl : MonoBehaviour
{
    public Transform player;
    
    void Update()
    {
        transform.position = new Vector3(player.position.x,0,-10f);
    }
}
