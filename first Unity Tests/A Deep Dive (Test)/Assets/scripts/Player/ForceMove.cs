using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceMove : MonoBehaviour
{
 public GameObject Player;
 public void Update()
    {
        Player.transform.position=transform.TransformPoint(0,0,1);
    }
}
