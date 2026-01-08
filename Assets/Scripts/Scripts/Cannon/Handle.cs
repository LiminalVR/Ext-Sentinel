using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Handle : MonoBehaviour
{
    private void OnTriggerEnter(Collider hand)
    {
        //if (hand.CompareTag("Player") && !GetComponentInParent<Cannon>().initialGrab)
        //{
        //    GetComponentInParent<Cannon>().grabHandleComplete = false;
        //    GetComponentInParent<Cannon>().initialGrab = true;
        //}
    }
}
