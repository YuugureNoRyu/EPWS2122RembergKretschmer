using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ContinuousMovement : MonoBehaviour
{
public XRNode inputSource;
private Vector2 inputAxis;
public XRRig rig;
public float additionalHeight=0.2f;
public float speed =1;
public bool freieBewegungAktiv;
private CharacterController character;
public GameObject Teleporter;
public GameObject Player;
public GameObject Telelaser;

    // Start is called before the first frame update
    void Start()
    {Player=GameObject.FindWithTag("Player");
        character= Player.GetComponent<CharacterController>();
        rig=Player.GetComponent<XRRig>();
        Teleporter=GameObject.FindWithTag("Locomotion");
       freieBewegungAktiv= SettingManager.freeMove;
       Telelaser=GameObject.FindWithTag("RechtsTele");
    }

    // Update is called once per frame
    void Update()
    {if(SettingManager.freeMove==true)
    {
       InputDevice device = InputDevices.GetDeviceAtXRNode(inputSource);
       device.TryGetFeatureValue(CommonUsages.primary2DAxis,out inputAxis);
    }}
    void CapsuleFollowHeadset()
    {
        character.height =rig.cameraInRigSpaceHeight +additionalHeight;
        Vector3 capsuleCenter=transform.InverseTransformPoint(rig.cameraGameObject.transform.position);
        character.center=new Vector3(capsuleCenter.x,character.height/2+character.skinWidth,capsuleCenter.z);
    }
    public void FixedUpdate()
{ if(SettingManager.freeMove==true)
  { CapsuleFollowHeadset();
       Quaternion headYaw=Quaternion.Euler(0,rig.cameraGameObject.transform.eulerAngles.y,0);
    Vector3 direction = headYaw * new Vector3(inputAxis.x, 0,inputAxis.y);
    character.Move(direction * Time.fixedDeltaTime * speed);
}
}
public void BewegungAendern()
{
    if(SettingManager.freeMove==true)
    { SettingManager.freeMove=false;
    Teleporter.SetActive(true);
    Telelaser.SetActive(true);
    }
    else 
    {
        SettingManager.freeMove=true;
    Teleporter.SetActive(false);
    Telelaser.SetActive(false);
    
    }
}
}