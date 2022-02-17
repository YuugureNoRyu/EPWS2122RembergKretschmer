using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingManager : MonoBehaviour
{
   public static bool freeMove;
   public static bool phobiaModus;

   public void Phobiawechel()
{
if(phobiaModus==true)
    {phobiaModus = false;
     Debug.Log(phobiaModus);
    }
    else{phobiaModus =true;
    Debug.Log(phobiaModus);}
}
    public void BewegungAendern()
{
    if(freeMove==true)
    { freeMove=false;
    Debug.Log(freeMove);}
    else 
    {freeMove=true;
      Debug.Log(freeMove);}
}

  
}
