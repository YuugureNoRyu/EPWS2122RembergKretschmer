using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnglerfischSpawn : MonoBehaviour
{
    public static bool WinteriaInteragiert=false;
    public static bool QualleInteragiert=false;
    public static bool VampierfischInteragiert=false;
    public static bool Spawn=false;
    public float targetTime=10f;
    public GameObject StirnLampe=GameObject.FindWithTag("StrinLampe");
public int x=0;
    public void Update()
    {
if(WinteriaInteragiert&&QualleInteragiert&&VampierfischInteragiert&&x==0)
{
Spawn=true;

Debug.Log("Anglerfisch gespawnt");
LampeAusfallen();
x++;
}
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
targetTime -= Time.deltaTime;
 
 if (targetTime >= 0.0f)
 {StirnLampe.SetActive(false);}
 else{StirnLampe.SetActive(true);}



}
}
