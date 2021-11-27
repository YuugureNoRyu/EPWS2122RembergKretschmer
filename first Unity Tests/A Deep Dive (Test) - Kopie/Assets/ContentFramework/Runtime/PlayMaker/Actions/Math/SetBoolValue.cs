// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Math)]
	[Tooltip("Sets the value of a Bool Variable.")]
	public class SetBoolValue : FsmStateAction
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmBool boolVariable;
		[RequiredField]
		public FsmBool boolValue;
		public bool everyFrame;
#if PHOTON_UNITY_NETWORKING
        public bool sync = true;
#endif

        public override void Reset()
		{
			boolVariable = null;
			boolValue = null;
			everyFrame = false;
		}

		public override void OnEnter()
		{
			boolVariable.Value = boolValue.Value;
			
			if (!everyFrame)
				Finish();

#if PHOTON_UNITY_NETWORKING
            if (sync)
                SyncBoolState();
#endif
        }

#if PHOTON_UNITY_NETWORKING
        private void SyncBoolState()
        {
            //Photon.Realtime.RaiseEventOptions raiseEventOptions = new Photon.Realtime.RaiseEventOptions { Receivers = Photon.Realtime.ReceiverGroup.All };
            //ExitGames.Client.Photon.SendOptions sendOptions = new ExitGames.Client.Photon.SendOptions { Reliability = true };
            //Photon.Pun.PhotonNetwork.RaiseEvent(100, boolVariable.Name, raiseEventOptions, sendOptions);
        }
#endif

        public override void OnUpdate()
		{
			boolVariable.Value = boolValue.Value;
		}
	}
}