using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectHolder : MonoBehaviour
{
 void Awake()
 {DontDestroyOnLoad(gameObject);}
}
