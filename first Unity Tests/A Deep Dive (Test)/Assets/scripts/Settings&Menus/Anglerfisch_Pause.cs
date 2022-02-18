using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anglerfisch_Pause : MonoBehaviour
{public  Animator AFisch;
    // Start is called before the first frame update
    void Start()
    {
      AFisch=gameObject.GetComponent<Animator>();
       AFisch.enabled=false;
    }

   
}
