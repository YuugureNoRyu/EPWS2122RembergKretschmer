using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallActivator : MonoBehaviour
{
 void OnTriggerEnter(Collider other)
 {if(other.CompareTag("Player"))
 {this.GetComponent<ChildDeactivate>().AlleAn();}
 }

 void OnTriggerExit(Collider other)
 {this.GetComponent<ChildDeactivate>().AlleAus();}
}
