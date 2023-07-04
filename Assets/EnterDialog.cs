using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterDialog : MonoBehaviour
{
     public GameObject EnterDialogObject;

    // 门碰到 player 之后触发进入下一关的弹框
    private void OnTriggerEnter2D(Collider2D other) {
        
        if(other.tag == "Player"){
            EnterDialogObject.SetActive(true);
    }}
    

    private void OnTriggerExit2D(Collider2D other) {
        
            if(other.tag == "Player"){
            EnterDialogObject.SetActive(false);
    }
    } 
         

}
