using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogChange : MonoBehaviour
{
    
public void FogChanger()
{
if(RenderSettings.fog==true)
    {RenderSettings.fog = false;
    }
    else{RenderSettings.fog =true;}
}
    
}
