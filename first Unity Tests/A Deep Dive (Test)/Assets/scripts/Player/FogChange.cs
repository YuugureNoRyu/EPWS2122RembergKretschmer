using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogChange : MonoBehaviour
{
    void Start()
    {
        if(SettingManager.phobiaModus==true)
        {
            RenderSettings.fog=false;
        }
        else
        {RenderSettings.fog=true;}
    }
public void FogChanger()
{
if(RenderSettings.fog==true||SettingManager.phobiaModus==true)
    {RenderSettings.fog = false;
    SettingManager.phobiaModus=false;
    }
    else{RenderSettings.fog =true;
    SettingManager.phobiaModus=true;
    }

}
    
}
