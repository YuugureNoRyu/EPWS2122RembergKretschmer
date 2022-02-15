using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingManager : MonoBehaviour
{
   public static bool freieBewegungAktiv;
   public static bool phobiaModus;

   public void Phobiawechel()
{
if(phobiaModus==true)
    {phobiaModus = false;
    }
    else{phobiaModus =true;}
}
    public void BewegungAendern()
{
    if(freieBewegungAktiv==true)
    { freieBewegungAktiv=false;}
    else 
    {freieBewegungAktiv=true;}
}

  
}
