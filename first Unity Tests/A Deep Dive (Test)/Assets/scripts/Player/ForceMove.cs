using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ForceMove : MonoBehaviour
{    public float targetTime = 10f;
     
    public XRNode inputSource;
private XRRig rig;
private CharacterController character;
public float speed =1;
public bool sceneStarten=false;


    

 
 public void Update(){

  targetTime -= Time.deltaTime;
 
 if (targetTime >= 0.0f)
 {
     character= GetComponent<CharacterController>();
        rig=GetComponent<XRRig>();

    
    
         Quaternion headYaw=Quaternion.Euler(0,rig.cameraGameObject.transform.eulerAngles.y,0);
         Vector3 direction = headYaw * new Vector3(0, -1f,0);
         
    character.Move(direction * Time.fixedDeltaTime * speed);
    
 }
 
 
 
 
}
}


