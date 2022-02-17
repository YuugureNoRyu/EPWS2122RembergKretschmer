using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildDeactivate : MonoBehaviour
{
 public void AlleAn()
 {
     for (int i = 0; i < transform.childCount; i++)
     {
         transform.GetChild(i).gameObject.SetActive(true);
     }
 }
    
 public void AlleAus()
 {
     for (int i = 0; i < transform.childCount; i++)
     {
         transform.GetChild(i).gameObject.SetActive(false);
     }
 }
}
