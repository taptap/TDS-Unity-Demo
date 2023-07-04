using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Frog : MonoBehaviour
{
    private Rigidbody2D rb;
    public  Transform leftpoint,rightpoint;
    public float Speed;
    private float leftx,rightx;
    private bool faceleft = true;

    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        leftx = leftpoint.position.x;
        rightx = rightpoint.position.x;
        Destroy(leftpoint.gameObject);
        Destroy(rightpoint.gameObject);

    }

    void Update()
    {
        Movement();
    }
    void Movement(){
        if (faceleft){
            //青蛙向左移动
            rb.velocity = new Vector2(-Speed,rb.velocity.y);
            if(transform.position.x < leftx){
                transform.localScale = new Vector3(-1,1,1);
                faceleft = false;
            }
        }else {
            //向右移动
            rb.velocity = new Vector2(Speed,rb.velocity.y);
            if(transform.position.x > rightx){
                transform.localScale = new Vector3(1,1,1);
                faceleft = true;
            }
        }


    }
}
