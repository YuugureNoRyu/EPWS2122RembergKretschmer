using HutongGames.PlayMaker;

namespace wovr.ContentFramework
{
#if PHOTON_UNITY_NETWORKING
    public class NetworkingStateAction : FsmStateAction, Photon.Realtime.IOnEventCallback
#else
    public class NetworkingStateAction : FsmStateAction
#endif
	{
        private readonly byte CUSTOM_EVENT_CODE = 151;
        public bool OneShotEvent = true;

#if PHOTON_UNITY_NETWORKING
        /// <summary>
        /// Add callback target on awake.
        /// </summary>
        public override void Awake()
        {
            base.Awake();
            Photon.Pun.PhotonNetwork.AddCallbackTarget(this);
        }
#endif

        /// <summary>
        /// Send a custom networking event.
        /// </summary>
        protected virtual void SendCustomEvent()
        {
#if PHOTON_UNITY_NETWORKING
            if (ContentNetworking.IsMaster())
            {
                var options = new Photon.Realtime.RaiseEventOptions() { Receivers = Photon.Realtime.ReceiverGroup.Others };
                Photon.Pun.PhotonNetwork.RaiseEvent(CUSTOM_EVENT_CODE, Owner.name + State.Name, options, ExitGames.Client.Photon.SendOptions.SendReliable);
                if (OneShotEvent)
                    Photon.Pun.PhotonNetwork.RemoveCallbackTarget(this);
            }
#endif
        }

        /// <summary>
        /// Trigger the custom networking event.
        /// </summary>
        protected virtual void TriggerCustomEvent()
        {
#if PHOTON_UNITY_NETWORKING
            if (OneShotEvent)
                Photon.Pun.PhotonNetwork.RemoveCallbackTarget(this);
#endif
        }

#if PHOTON_UNITY_NETWORKING
        /// <summary>
        /// Photon event callback.
        /// </summary>
        /// <param name="photonEvent"></param>
        public void OnEvent(ExitGames.Client.Photon.EventData photonEvent)
        {
            var eventCode = photonEvent.Code;
            if (eventCode == CUSTOM_EVENT_CODE && Owner != null)
            {
                if (Owner != null)
                {
                    var sendName = (string)photonEvent.CustomData;
                    var name = Owner.name + State.Name;
                    if (sendName.Equals(name))
                        TriggerCustomEvent();
                }
                else
                    Photon.Pun.PhotonNetwork.RemoveCallbackTarget(this);
            }
        }
#endif
    }
}
