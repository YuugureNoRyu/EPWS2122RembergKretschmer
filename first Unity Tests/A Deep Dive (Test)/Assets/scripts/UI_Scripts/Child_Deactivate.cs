using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Child_Deactivate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
 public void AllesAus()
 {
     for (int i = 0; i < transform.childCount; i++)
     {
         transform.GetChild(i).gameObject.SetActive(false);
     }
 }
  public void AllesAn()
 {
     for (int i = 0; i < transform.childCount; i++)
     {
         transform.GetChild(i).gameObject.SetActive(true);
     }
 }
    // Update is called once per frame
    void Update()
    {
        
    }
}
