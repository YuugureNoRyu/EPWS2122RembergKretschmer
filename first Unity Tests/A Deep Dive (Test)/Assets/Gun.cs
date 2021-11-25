using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
public float speed =40;
public GameObject bullet;
public Transform Waffe;

  public void Schießen()
  {
      GameObject spawnedBullet = Instantiate(bullet,Waffe.position,Waffe.rotation);
      spawnedBullet.GetComponent<Rigidbody>().velocity=speed*Waffe.forward;
      Destroy(spawnedBullet,2);
  }
}
