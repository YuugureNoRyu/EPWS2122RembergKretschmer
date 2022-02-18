using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnglerfischSpawn : MonoBehaviour
{
    public static bool WinteriaInteragiert=false;
    public static bool QualleInteragiert=false;
    public static bool VampierfischInteragiert=false;
    public bool spawnable=true;
    public float targetTime=10f;
     public GameObject StirnLampe;
     public int bigboy=0;
public int cooldown=1;     

 void Start()
{ 
 
this.GetComponent<BoxCollider>().enabled=false;
}



public void Update()
{
if(WinteriaInteragiert&&QualleInteragiert&&VampierfischInteragiert&&spawnable)
{cooldown=0;
Debug.Log("Anglerfisch gespawnt");
StirnLampe=GameObject.FindWithTag("KopfLampe");}
if(cooldown==0)
{targetTime -= Time.deltaTime;
LampeAusfallen();}
if(bigboy==1)
{go();}

}
    
    public void Timer()
    {
targetTime -= Time.deltaTime;
    }
public void Qualle()
{QualleInteragiert=true;
 Debug.Log("Qualle Check");}

public void Winteria()
{WinteriaInteragiert=true;
 Debug.Log("Winteria Check");}

public void Vampierfisch()
{VampierfischInteragiert=true;
 Debug.Log("Vampirtintenfisch Check");}


public void LampeAusfallen()
{

 
 if (targetTime >= 0.0f)
 {StirnLampe.GetComponent<Light>().enabled=false;
 Debug.Log("LichtAus");}
 else{StirnLampe.GetComponent<Light>().enabled=true;
 Debug.Log("LichtAn");
 bigboy=1;
 }
}
public void go()
{
    
this.transform.position=new Vector3(-8,2.5f,-20);
this.GetComponent<Anglerfisch_Start>().GoFisch();
this.GetComponent<BoxCollider>().enabled=true;
spawnable=false;
}
}

