using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Assets.Networking.Lobby;
using Assets.Utils;
using Grpc.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Interaction.Lobby
{
    class LobbyRoot : MonoBehaviour
    {
        public LobbyPlace[] Places;

        public GameObject StartGameButton;
        private LobbyServiceImpl mService;
        private Server mServer;

        private Channel mChannel;
        private LobbyService.LobbyServiceClient mClient;
        private bool mRecivedStartAction;

        public string GameSceneName;
        public string LoginSceneName;

        void Start()
        {
            if (GameUtils.CurrentMode == GameMode.Client)
            {
                mChannel = new Channel(GameUtils.IP.ToString(), GameUtils.LobbyPort, ChannelCredentials.Insecure);
                mClient = new LobbyService.LobbyServiceClient(mChannel);
                ListenStates();
                ListenStart();
                StartGameButton.SetActive(false);
            }

            if (GameUtils.CurrentMode == GameMode.Server)
            {
                mServer = new Server();
                mServer.Ports.Add(new ServerPort(IPAddress.Any.ToString(), GameUtils.LobbyPort, ServerCredentials.Insecure));
                mService = new LobbyServiceImpl(GameUtils.Nickname);
                mServer.Services.Add(LobbyService.BindService(mService));
                mService.OnUserStateChanged += ServiceOnOnUserStateChanged;
                mServer.Start();
                StartGameButton.SetActive(true);
                ServiceOnOnUserStateChanged(GameUtils.Nickname, true);
            }
        }

        private void ServiceOnOnUserStateChanged(string id, bool state)
        {
            if (state)
            {
                var found = Places.FirstOrDefault(p => !p.IsBusy);
                if (found != null)
                {
                    found.Name = id;
                    found.IsBusy = true;
                }
            }
            else
            {
                var found = Places.FirstOrDefault(p => p.Name == id);
                if (found != null)
                {
                    found.IsBusy = false;
                    found.Name = "";
                }
            }
        }

        private async void ListenStates()
        {
            try
            {
                using (var call = mClient.ListenUserState(new Empty(), cancellationToken: mChannel.ShutdownToken))
                using (var stream = call.ResponseStream)
                {
                    while (await stream.MoveNext(mChannel.ShutdownToken))
                    {
                        var state = stream.Current;
                        ServiceOnOnUserStateChanged(state.ID, state.Connected);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        private async void ListenStart()
        {
            try
            {
                using (var call =
                    mClient.ListenStart(new UserState {ID = GameUtils.Nickname }, cancellationToken: mChannel.ShutdownToken))
                using (var stream = call.ResponseStream)
                {
                    await stream.MoveNext(mChannel.ShutdownToken);
                    mRecivedStartAction = stream.Current.Start;
                }
            }
            catch (Exception e)
            {
                mRecivedStartAction = false;
                Debug.LogError(e);
                throw;
            }
            finally
            {
                Destroy(this);
            }
        }

        public void Leave()
        {
            if (mService != null)
                mService.Leave();

            mRecivedStartAction = false;
            Destroy(this);
        }

        public void StartGame()
        {
            if (GameUtils.CurrentMode != GameMode.Server)
                return;

            mService.StartGame();
            mRecivedStartAction = true;
            Destroy(this);
        }

        void LoadNextScene()
        {
            SceneManager.LoadScene(mRecivedStartAction ? GameSceneName : LoginSceneName);
        }

        async void OnDestroy()
        {
            if (mService != null)
                mService.OnUserStateChanged -= ServiceOnOnUserStateChanged;

            if (mServer != null)
            {
                await mServer.KillAsync();
                LoadNextScene();
            }

            if (mChannel != null)
            {
                await mChannel.ShutdownAsync();
                LoadNextScene();
            }
        }
    }
}
