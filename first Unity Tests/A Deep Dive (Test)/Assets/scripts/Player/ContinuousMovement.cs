using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ContinuousMovement : MonoBehaviour
{public XRNode inputSource;
private Vector2 inputAxis;
private XRRig rig;
public float additionalHeight=0.2f;
public float speed =1;
public static bool freieBewegungAktiv;
private CharacterController character;
    // Start is called before the first frame update
    void Start()
    {
        character= GetComponent<CharacterController>();
        rig=GetComponent<XRRig>();
    }

    // Update is called once per frame
    void Update()
    {if(freieBewegungAktiv==true)
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
{ if(freieBewegungAktiv==true)
  { CapsuleFollowHeadset();
       Quaternion headYaw=Quaternion.Euler(0,rig.cameraGameObject.transform.eulerAngles.y,0);
    Vector3 direction = headYaw * new Vector3(inputAxis.x, 0,inputAxis.y);
    character.Move(direction * Time.fixedDeltaTime * speed);
}
}
public void BewegungAendern()
{
    if(freieBewegungAktiv==true)
    { freieBewegungAktiv=false;}
    else 
    {freieBewegungAktiv=true;}
}
}