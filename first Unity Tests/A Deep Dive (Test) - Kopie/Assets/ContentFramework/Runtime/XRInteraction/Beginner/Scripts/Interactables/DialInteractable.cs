using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using UnityEngine;
using System;

using wovr.ContentFramework;

/// <summary>
/// Custom interactable than will rotation around a given axis. It can be limited in range and also be made either
/// continuous or snapping to integer steps.
/// Rotation can be driven either by controller rotation (e.g. rotating a volume dial) or controller movement (e.g.
/// pulling down a lever)
/// </summary>
public class DialInteractable : XRBaseInteractable
{
    public enum InteractionType
    {
        ControllerRotation,
        ControllerPull
    }
    
    [Serializable]
    public class DialTurnedAngleEvent : UnityEvent<float> { }
    [Serializable]
    public class DialTurnedStepEvent : UnityEvent<int> { }

    [Serializable]
    public class DialChangedEvent : UnityEvent<DialInteractable> { }

    public InteractionType DialType = InteractionType.ControllerRotation;
    
    public Rigidbody RotatingRigidbody;
    public Vector3 LocalRotationAxis;
    public Vector3 LocalAxisStart;
    public float RotationAngleMaximum;

    [Tooltip("If 0, this is a float dial going from 0 to 1, if not 0, that dial is int with that many steps")]
    public int Steps = 0;
    public bool SnapOnRelease = true;

    public AudioClip SnapAudioClip;

    public UnityEvent OnMinAngle;
    public UnityEvent OnMaxAngle;
    public DialTurnedAngleEvent OnDialAngleChanged;
    public DialTurnedStepEvent OnDialStepChanged;
    public DialChangedEvent OnDialChanged;

    public float CurrentAngle { get; private set; }
    public int CurrentStep { get; private set; }

    private XRBaseInteractor m_GrabbingInteractor;
    private Quaternion m_GrabbedRotation;
    
    private Vector3 m_StartingWorldAxis;
    private Vector3 m_WorldAxisStart;
    private Quaternion m_Rotation;
    
    private float m_SyncAngle;
    private float m_StepSize;

    private Transform m_SyncTransform;
    private Transform m_OriginalTransform;

    /// <summary>
    /// Init some values.
    /// </summary>
    protected override void Start()
    {
        LocalAxisStart.Normalize();
        LocalRotationAxis.Normalize();
        
        if (RotatingRigidbody == null)
        {
            RotatingRigidbody = GetComponentInChildren<Rigidbody>();
        }
        
        CurrentAngle = 0;
        m_Rotation = transform.rotation;

        var obj = new GameObject("Dial_Start_Copy");
        m_OriginalTransform = obj.transform;
        m_OriginalTransform.SetParent(transform.parent);
        m_OriginalTransform.localRotation = transform.localRotation;
        m_OriginalTransform.localPosition = transform.localPosition;
        
        if (Steps > 0) m_StepSize = RotationAngleMaximum / Steps;
        else m_StepSize = 0.0f;
        base.Start();
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        if (!ContentNetworking.IsMaster())
            return;

        if (isSelected)
        {
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed)
            {
                m_StartingWorldAxis = m_OriginalTransform.TransformDirection(LocalAxisStart);
                m_WorldAxisStart = m_SyncTransform.TransformDirection(LocalAxisStart);
                var worldRotationAxis = m_SyncTransform.TransformDirection(LocalRotationAxis);

                var angle = 0.0f;
                var m_NewRight = m_WorldAxisStart;

                if (DialType == InteractionType.ControllerRotation)
                {
                    var difference = m_GrabbingInteractor.transform.rotation * Quaternion.Inverse(m_GrabbedRotation);
                    m_NewRight = difference * m_WorldAxisStart;
                    //get the new angle between the original right and this new right along the up axis
                    angle = Vector3.SignedAngle(m_StartingWorldAxis, m_NewRight, worldRotationAxis);
                    if (angle < 0) angle = 360 + angle;
                }
                else
                {
                    var centerToController = m_GrabbingInteractor.transform.position - transform.position;
                    centerToController.Normalize();

                    m_NewRight = centerToController;
                    angle = Vector3.SignedAngle(m_StartingWorldAxis, m_NewRight, worldRotationAxis);
                    if (angle < 0) angle = 360 + angle;
                }

                m_SyncAngle = angle;
                var finalAngle = ProcessAngle(angle);
                //first, we use the raw angle to move the sync transform, that allow to keep the proper current rotation
                //even if we snap during rotation
                m_NewRight = Quaternion.AngleAxis(angle, worldRotationAxis) * m_StartingWorldAxis;
                angle = Vector3.SignedAngle(m_WorldAxisStart, m_NewRight, worldRotationAxis);
                var newRot = Quaternion.AngleAxis(angle, worldRotationAxis) * m_SyncTransform.rotation;

                //then we redo it but this time using finalAngle, that will snap if needed.
                m_NewRight = Quaternion.AngleAxis(finalAngle, worldRotationAxis) * m_StartingWorldAxis;
                CurrentAngle = finalAngle;

                finalAngle = Vector3.SignedAngle(m_WorldAxisStart, m_NewRight, worldRotationAxis);
                m_Rotation = Quaternion.AngleAxis(finalAngle, worldRotationAxis) * m_SyncTransform.rotation;

                ApplyRotation(m_Rotation);

                m_SyncTransform.transform.rotation = newRot;
                m_GrabbedRotation = m_GrabbingInteractor.transform.rotation;
            }
        }
    }

    private void ApplyRotation(Quaternion newRBRotation)
    {
        if (RotatingRigidbody != null)
            RotatingRigidbody.MoveRotation(newRBRotation);
        else
            transform.rotation = newRBRotation;
    }

    private float ProcessAngle(float angle)
    {
        //if the angle is < 0 or > to the max rotation, we clamp but TO THE CLOSEST (a simple clamp would clamp
        // of an angle of 350 for a 0-180 angle range would clamp to 180, when we want to clamp to 0)
        if (angle > RotationAngleMaximum)
        {
            var upDiff = 360 - angle;
            var lowerDiff = angle - RotationAngleMaximum;

            if (upDiff < lowerDiff)
            {
                angle = 0;
                OnMinAngle.Invoke();
            }
            else
            {
                angle = RotationAngleMaximum;
                OnMaxAngle.Invoke();
            }
        }

        var finalAngle = angle;
        if (!SnapOnRelease && Steps > 0)
        {
            var step = Mathf.RoundToInt(angle / m_StepSize);
            finalAngle = step * m_StepSize;

            if (!Mathf.Approximately(finalAngle, CurrentAngle))
            {
                SFXPlayer.Instance.PlaySFX(SnapAudioClip, transform.position, new SFXPlayer.PlayParameters()
                {
                    Pitch = UnityEngine.Random.Range(0.9f, 1.1f),
                    SourceID = -1,
                    Volume = 1.0f
                }, 0.0f);

                OnDialStepChanged.Invoke(step);
                OnDialChanged.Invoke(this);
                CurrentStep = step;
            }
        }

        OnDialAngleChanged.Invoke(finalAngle);
        OnDialChanged.Invoke(this);
        return angle;
    }

#if PHOTON_UNITY_NETWORKING
    protected override void SerializeView(Photon.Pun.PhotonStream stream)
    {
        base.SerializeView(stream);
        if (stream.IsWriting)
        {
            stream.SendNext(m_SyncAngle);
            stream.SendNext(m_Rotation);
        }
        else
        {
            ProcessAngle((float)stream.ReceiveNext());
            ApplyRotation((Quaternion)stream.ReceiveNext());
        }
    }
#endif

    protected internal override void OnSelectEnter(XRBaseInteractor interactor)
    {
        m_GrabbedRotation = interactor.transform.rotation;
        m_GrabbingInteractor = interactor;

        //create an object that will track the rotation
        var syncObj = new GameObject("TEMP_DialSyncTransform");
        m_SyncTransform = syncObj.transform;
        
        if(RotatingRigidbody != null)
        {
            m_SyncTransform.rotation = RotatingRigidbody.transform.rotation;
            m_SyncTransform.position = RotatingRigidbody.position;
        }
        else
        {
            m_SyncTransform.rotation = transform.rotation;
            m_SyncTransform.position = transform.position;
        }
        
        base.OnSelectEnter(interactor);
    }

    protected internal override void OnSelectExit(XRBaseInteractor interactor)
    {
        base.OnSelectExit(interactor);
        
        if (SnapOnRelease && Steps > 0)
        {
            Vector3 right = transform.TransformDirection(LocalAxisStart);
            Vector3 up = transform.TransformDirection(LocalRotationAxis);
            
            float angle = Vector3.SignedAngle(m_StartingWorldAxis, right, up);
            if (angle < 0) angle = 360 + angle;

            int step = Mathf.RoundToInt(angle / m_StepSize);
            angle = step * m_StepSize;
            
            if (angle != CurrentAngle)
            {
                SFXPlayer.Instance.PlaySFX(SnapAudioClip, transform.position, new SFXPlayer.PlayParameters()
                {
                    Pitch = UnityEngine.Random.Range(0.9f, 1.1f),
                    SourceID = -1,
                    Volume = 1.0f
                }, 0.0f);
                
                OnDialStepChanged.Invoke(step);
                OnDialChanged.Invoke(this);
                CurrentStep = step;
            }
            
            Vector3 newRight = Quaternion.AngleAxis(angle, up) * m_StartingWorldAxis;
            angle = Vector3.SignedAngle(right, newRight, up);

            CurrentAngle = angle;

            if (RotatingRigidbody != null)
            {
                Quaternion newRot = Quaternion.AngleAxis(angle, up) * RotatingRigidbody.rotation;
                RotatingRigidbody.MoveRotation(newRot);
            }
            else
            {
                Quaternion newRot = Quaternion.AngleAxis(angle, up) * transform.rotation;
                transform.rotation = newRot;
            }
        }
        
        Destroy(m_SyncTransform.gameObject);
    }

    public override bool IsSelectableBy(XRBaseInteractor interactor)
    {
        int interactorLayerMask = 1 << interactor.gameObject.layer;
        return base.IsSelectableBy(interactor) && (interactionLayerMask.value & interactorLayerMask) != 0 ;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
        UnityEditor.Handles.DrawSolidArc(transform.position, transform.TransformDirection(LocalRotationAxis), transform.TransformDirection(LocalAxisStart), RotationAngleMaximum, 0.2f );
    }
#endif
    
    
}
