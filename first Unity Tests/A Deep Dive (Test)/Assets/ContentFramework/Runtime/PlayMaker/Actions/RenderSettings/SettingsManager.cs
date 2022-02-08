using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{

    [SerializeField] bool phobiaMode;
    [SerializeField] bool teleportsystem;
    // Start is called before the first frame update
    void Start()
    {if(phobiaMode==true)
    {RenderSettings.fog = false;
    }
    else{RenderSettings.fog =true;}
  
    }

 
}
