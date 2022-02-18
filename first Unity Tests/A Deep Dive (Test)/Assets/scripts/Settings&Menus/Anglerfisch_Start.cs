using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anglerfisch_Start : MonoBehaviour
{public  Animator AFisch;
    // Start is called before the first frame update
    void Start()
    {
      AFisch=gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    public void GoFisch()
    {
        AFisch.enabled=true;
    }
}
