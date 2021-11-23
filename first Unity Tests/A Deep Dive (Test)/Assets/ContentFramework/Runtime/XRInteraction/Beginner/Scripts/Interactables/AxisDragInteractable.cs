using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using wovr.ContentFramework;
using Random = UnityEngine.Random;

/// <summary>
/// Custom interactable that can be dragged along an axis. Can either be continuous or snap to integer steps.
/// </summary>
public class AxisDragInteractable : XRBaseInteractable
{
    [Serializable]
    public class DragDistanceEvent : UnityEvent<float> {}

    [Serializable]
    public class DragStepEvent : UnityEvent<int> {}

    [Tooltip("The Rigidbody that will be moved. If null will try to grab one on that object or its children")]
    public Rigidbody MovingRigidbody;

    public Vector3 LocalAxis;
    public float AxisLength;
    
    public Vector3 StartOffset;

    [Tooltip("If 0, then this is a float [0,1] range slider, otherwise there is an integer slider")]
    public int Steps = 0;
    public bool SnapOnlyOnRelease = true;
    
    public bool ReturnOnFree;
    public float ReturnSpeed;

    public AudioClip SnapAudioClip;

    public UnityEvent OnStartEnter;
    public UnityEvent OnStartExit;
    public UnityEvent OnEndEnter;
    public UnityEvent OnEndExit;
    public DragDistanceEvent OnDragDistance;
    public DragStepEvent OnDragStep;
    
    Vector3 m_EndPoint;
    Vector3 m_StartPoint;
    Vector3 m_GrabbedOffset;
    float m_CurrentDistance;
    int m_CurrentStep;
    XRBaseInteractor m_GrabbingInteractor;

    float m_StepLength;
    private bool m_AtStart;
    private bool m_AtEnd;
    private bool m_ReturnToDefault;
    private Vector3 m_TargetPoint;
    private bool m_WasSelected;
    private float m_StartMag;
    private float m_EndMag;

    // Start is called before the first frame update
    protected override void Start()
    {
        LocalAxis.Normalize();

        //Length can't be negative, a negative length just mean an inverted axis, so fix that
        if (AxisLength < 0)
        {
            LocalAxis *= -1;
            AxisLength *= -1;
        }

        if (Steps == 0)
        {
            m_StepLength = 0.0f;
        }
        else
        {
            m_StepLength = AxisLength / Steps;
        }
        
        m_StartPoint = transform.position;
        transform.position += transform.TransformDirection(StartOffset);

        m_TargetPoint = transform.position;
        m_StartMag = (m_TargetPoint - m_StartPoint).magnitude;
        m_EndMag = (m_TargetPoint - m_EndPoint).magnitude;

        m_EndPoint = transform.position + transform.TransformDirection(LocalAxis) * AxisLength;
        
        if (MovingRigidbody == null)
        {
            MovingRigidbody = GetComponentInChildren<Rigidbody>();
        }

        m_CurrentStep = 0;
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
                m_WasSelected = true;
                var WorldAxis = transform.TransformDirection(LocalAxis);
                var distance = m_GrabbingInteractor.transform.position - transform.position - m_GrabbedOffset;
                var projected = Vector3.Dot(distance, WorldAxis);

                //ajust projected to clamp it to steps if there is steps
                if (Steps != 0 && !SnapOnlyOnRelease)
                {
                    var steps = Mathf.RoundToInt(projected / m_StepLength);
                    projected = steps * m_StepLength;
                }
                
                if (projected > 0)
                    m_TargetPoint = Vector3.MoveTowards(transform.position, m_EndPoint, projected);
                else
                    m_TargetPoint = Vector3.MoveTowards(transform.position, m_StartPoint, -projected);

                if (Steps > 0)
                {
                    var posStep = Mathf.RoundToInt((m_TargetPoint - m_StartPoint).magnitude / m_StepLength);
                    if (posStep != m_CurrentStep)
                    {
                        SFXPlayer.Instance.PlaySFX(SnapAudioClip, transform.position, new SFXPlayer.PlayParameters()
                        {
                            Pitch = Random.Range(0.9f, 1.1f),
                            SourceID = -1,
                            Volume = 1.0f
                        }, 0.0f);
                        OnDragStep.Invoke(posStep);
                    }

                    m_CurrentStep = posStep;
                }

                m_StartMag = (m_TargetPoint - m_StartPoint).magnitude;
                m_EndMag = (m_TargetPoint - m_EndPoint).magnitude;

                AdjustPosition(m_TargetPoint);
                CheckStartEndPosition(m_StartMag, m_EndMag);
            }
        }
        else
        {
            if (ReturnOnFree && m_WasSelected)
                ReturnToDefault();
        }
    }

    private void ReturnToDefault()
    {
        m_ReturnToDefault = true;
        m_TargetPoint = Vector3.MoveTowards(transform.position, m_StartPoint + transform.TransformDirection(StartOffset), ReturnSpeed * Time.deltaTime);
        var move = m_TargetPoint - transform.position;

        if (MovingRigidbody != null)
            MovingRigidbody.MovePosition(MovingRigidbody.position + move);
        else
            transform.position = transform.position + move;
        OnStartExit.Invoke();
        OnEndExit.Invoke();
    }

    /// <summary>
    /// Adjust position of the draggable.
    /// </summary>
    /// <param name="targetPoint"></param>
    private void AdjustPosition(Vector3 targetPoint)
    {
        var move = targetPoint - transform.position;
        if (MovingRigidbody != null)
            MovingRigidbody.MovePosition(MovingRigidbody.position + move);
        else
            transform.position = transform.position + move;
    }

    /// <summary>
    /// Check callback invoking.
    /// </summary>
    /// <param name="targetPoint"></param>
    private void CheckStartEndPosition(float startMag, float endMag)
    {
        OnDragDistance.Invoke(startMag);
        if (startMag == 0)
        {
            m_AtStart = true;
            OnStartEnter.Invoke();
        }
        else if (m_AtStart)
        {
            m_AtStart = false;
            OnStartExit.Invoke();
        }
        
        if (endMag == 0)
        {
            m_AtEnd = true;
            OnEndEnter.Invoke();
        }
        else if (m_AtEnd)
        {
            m_AtEnd = false;
            OnEndExit.Invoke();
        }
    }

#if PHOTON_UNITY_NETWORKING
    /// <summary>
    /// Serialize the hand animations.
    /// </summary>
    /// <param name="stream"></param>
    protected override void SerializeView(Photon.Pun.PhotonStream stream)
    {
        base.SerializeView(stream);
        if (stream.IsWriting)
        {
            stream.SendNext(m_TargetPoint);
            stream.SendNext(m_StartMag);
            stream.SendNext(m_EndMag);
            stream.SendNext(m_ReturnToDefault);

            m_ReturnToDefault = false;
        }
        else
        {
            AdjustPosition((Vector3)stream.ReceiveNext());

            var start = (float)stream.ReceiveNext();
            var end = (float)stream.ReceiveNext();
            CheckStartEndPosition(start, end);

            var state = (bool)stream.ReceiveNext();
            if (state)
                ReturnToDefault();
        }
    }
#endif

    protected internal override void OnSelectEnter(XRBaseInteractor interactor)
    {
        base.OnSelectEnter(interactor);

        m_GrabbedOffset = interactor.transform.position - transform.position;
        m_GrabbingInteractor = interactor;
    }

    protected internal override void OnSelectExit(XRBaseInteractor interactor)
    {
        base.OnSelectExit(interactor);

        if (SnapOnlyOnRelease && Steps != 0)
        {
            float dist = (transform.position - m_StartPoint).magnitude;
            int step = Mathf.RoundToInt(dist / m_StepLength);
            dist = step * m_StepLength;
            
            transform.position = m_StartPoint + transform.TransformDirection(LocalAxis) * dist;

            if (step != m_CurrentStep)
            {
                SFXPlayer.Instance.PlaySFX(SnapAudioClip, transform.position, new SFXPlayer.PlayParameters()
                {
                    Pitch = Random.Range(0.9f, 1.1f),
                    SourceID = -1,
                    Volume = 1.0f
                }, 0.0f);
                OnDragStep.Invoke(step);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 end = transform.position + transform.TransformDirection(LocalAxis.normalized) * AxisLength;
        
        Gizmos.DrawLine(transform.position, end);
        Gizmos.DrawSphere(end, 0.01f);
    }
}
