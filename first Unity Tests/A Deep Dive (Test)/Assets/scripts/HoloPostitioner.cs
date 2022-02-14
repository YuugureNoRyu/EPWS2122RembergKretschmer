using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloPostitioner : MonoBehaviour
{
  public GameObject Holomodell;
public Vector3 pos;
    // Update is called once per frame
    void Update()
    {
        
    
            Holomodell.transform.position=transform.TransformPoint(pos);
        
    }
}
