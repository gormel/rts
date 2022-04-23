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
using UnityEngine.UI;

namespace Assets.Interaction.Lobby
{
    class LobbyRoot : MonoBehaviour
    {
        public LobbyPlace[] Places;

        private int mTeam = 1;
        public Text CurrentTeamText;
        public GameObject StartGameButton;
        public GameObject AddBotButton;
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
                AddBotButton.SetActive(false);
            }

            if (GameUtils.CurrentMode == GameMode.Server)
            {
                mServer = new Server();
                mServer.Ports.Add(new ServerPort(IPAddress.Any.ToString(), GameUtils.LobbyPort, ServerCredentials.Insecure));
                mService = new LobbyServiceImpl(GameUtils.Nickname, mTeam, GameUtils.MaxPlayers);
                mServer.Services.Add(LobbyService.BindService(mService));
                mService.OnUserStateChanged += ServiceOnOnUserStateChanged;
                mServer.Start();
                StartGameButton.SetActive(true);
                AddBotButton.SetActive(true);
                ServiceOnOnUserStateChanged(new UserState() { ID = GameUtils.Nickname, Connected = true, Team = 1 });
            }
        }

        public void AddBot()
        {
            if (GameUtils.CurrentMode == GameMode.Server) 
                mService.AddBot();
        }

        public void RemoveBot(string botId)
        {
            if (GameUtils.CurrentMode == GameMode.Server) 
                mService.RemoveBot(botId);
        }

        public void SetBotTeam(string botId, int team)
        {
            if (GameUtils.CurrentMode == GameMode.Server) 
                mService.SetBotTeam(botId, team);
        }

        public void ChangeTeam()
        {
            mTeam = Math.Max((mTeam + 1) % (GameUtils.MaxPlayers + 1), 1);
            var updatedState = new UserState {ID = GameUtils.Nickname, Connected = true, Team = mTeam};
            if (GameUtils.CurrentMode == GameMode.Client)
                mClient.UpdateStateAsync(updatedState);
            
            if (GameUtils.CurrentMode == GameMode.Server)
                mService.SetHostTeam(mTeam);
        }

        private void ServiceOnOnUserStateChanged(UserState state)
        {
            if (state.ID == GameUtils.Nickname)
            {
                CurrentTeamText.text = state.Team.ToString();
                GameUtils.Team = state.Team;
            }
            
            if (state.Connected)
            {
                var found = Places.FirstOrDefault(p => p.Name == state.ID);
                if (found == null)
                    found = Places.FirstOrDefault(p => !p.IsBusy);
                
                if (found != null)
                {
                    found.Name = state.ID;
                    found.IsBusy = true;
                    found.Team = state.Team;
                    found.IsBot = state.IsBot;
                }

                if (state.IsBot) 
                    GameUtils.BotPlayers.AddOrUpdate(state.ID, state, (n, s) => state);
                else
                    GameUtils.RegistredPlayers.AddOrUpdate(state.ID, state, (n, t) => state);
            }
            else
            {
                var found = Places.FirstOrDefault(p => p.Name == state.ID);
                if (found != null)
                {
                    found.IsBusy = false;
                    found.Name = "";
                    found.Team = 1;
                }

                if (state.IsBot)
                    GameUtils.BotPlayers.TryRemove(state.ID, out _);
                else
                    GameUtils.RegistredPlayers.TryRemove(state.ID, out _);
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
                        ServiceOnOnUserStateChanged(state);
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
                    mClient.ListenStart(new UserState { ID = GameUtils.Nickname, Connected = true, Team = mTeam }, cancellationToken: mChannel.ShutdownToken))
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
