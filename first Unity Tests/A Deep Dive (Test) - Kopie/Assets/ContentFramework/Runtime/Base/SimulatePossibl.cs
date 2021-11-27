#if PHOTON_UNITY_NETWORKING
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;
#endif

using UnityEngine.SceneManagement;
using UnityEngine;

namespace wovr.ContentFramework
{
#if PHOTON_UNITY_NETWORKING
    public class SimulatePossibl : MonoBehaviourPunCallbacks
#else
    public class SimulatePossibl : MonoBehaviour
#endif
    {
#if PHOTON_UNITY_NETWORKING
        [SerializeField] private bool m_MultiUser = true;

        private TypedLobby m_Lobby;
        private RoomOptions m_RoomOptions;
#endif
#if ADDRESSABLES_EXISTS && MULTICLIENT_EXISTS
        [SerializeField] private bool m_LoadCorporate = true;
        [SerializeField] private string m_CustomerKey = "wov";
#endif
        [SerializeField] private int m_SceneToLoad = 0;

        /// <summary>
        /// Init values.
        /// </summary>
        private async void Start()
		{
#if PHOTON_UNITY_NETWORKING
            if (m_MultiUser)
            {
                m_Lobby = new TypedLobby("myLobby", LobbyType.SqlLobby);
                m_RoomOptions = new RoomOptions
                {
                    MaxPlayers = 6,
                    CustomRoomProperties = new Hashtable() { { "C0", 0 } },
                    CustomRoomPropertiesForLobby = new string[] { "C0" },
                    PublishUserId = true
                };

                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.NickName = Random.Range(0, 999999).ToString();
                PhotonNetwork.GameVersion = "1";

                while (PhotonNetwork.CurrentRoom == null)
                    await System.Threading.Tasks.Task.Yield();
            }
#endif
#if ADDRESSABLES_EXISTS && MULTICLIENT_EXISTS
            if (m_LoadCorporate)
                await LoadCorporate();
#endif
            SceneManager.LoadScene(m_SceneToLoad);
        }

#if ADDRESSABLES_EXISTS && MULTICLIENT_EXISTS
        /// <summary>
        /// Load the corporate design bundles.
        /// </summary>
        private async System.Threading.Tasks.Task LoadCorporate()
        {
            Multiclient.Customer.Key = m_CustomerKey;
            await UnityEngine.AddressableAssets.Addressables.InitializeAsync().Task;
            await UnityEngine.AddressableAssets.Addressables.LoadContentCatalogAsync("https://assetbundles.obs.otc.t-systems.com/corporate-design/Android/catalog_corporate.json").Task;
        }
#endif

#if PHOTON_UNITY_NETWORKING
        /// <summary>
        /// Callback if connect to master.
        /// </summary>
        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            PhotonNetwork.JoinLobby(m_Lobby);
        }

        /// <summary>
        /// Callback if the lobby is joined.
        /// </summary>
        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
            PhotonNetwork.JoinOrCreateRoom("Testing", m_RoomOptions, m_Lobby);
        }
#endif
    }
}
