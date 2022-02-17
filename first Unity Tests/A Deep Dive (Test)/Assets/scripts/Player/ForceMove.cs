using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;  
public class ForceMove : MonoBehaviour
{    public float targetTime = 10f;
     public int SceneNumber=1; 
    public XRNode inputSource;
private XRRig rig;
private CharacterController character;
public float speed =1;
public GameObject WasserBox;

 
 public void Update(){

  targetTime -= Time.deltaTime;
 
 if (targetTime >= 0.0f)
 {
     character= GetComponent<CharacterController>();
        rig=GetComponent<XRRig>();

    
    
     
         Vector3 direction =new Vector3(0, -1f,0);
         
    character.Move(direction * Time.fixedDeltaTime * speed);}
   
    else{

  
        SceneManager.LoadScene(SceneNumber);  
  
}   


 }
 
 
 
 
}




