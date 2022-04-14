using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Steamworks;
using Steamworks.Data;
using TMPro;
using Mirror;


public class SteamLobbyManager : NetworkBehaviour
{   
    public static SteamLobbyManager instance;
    
    public NetworkManager network;
    private Lobby currentLobby;
    private int maxPlayers = 4;
    private const string HostAddressKey = "HostAddress";

    //Events
    public UnityEvent OnCreate;
    public UnityEvent OnJoin;
    public UnityEvent OnLeave;

    //Objects for lobby
    public GameObject lobbyObject;
    public Transform lobbyContent;

    //Objects for in lobby player list
    public GameObject inLobbyFriend;
    public Transform playerContent;

    //Objects for in game chat messages
    public GameObject chatMessage;
    public Transform chatContent;
    public TMP_InputField chatInput;

    Dictionary<SteamId, GameObject> inLobby = new Dictionary<SteamId, GameObject>();

    #region Unity Methods

    private void Awake() {
        instance = this;
    }

    private void Start() {
        //Callback Events
        Steamworks.SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        Steamworks.SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        Steamworks.SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        Steamworks.SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        Steamworks.SteamMatchmaking.OnChatMessage += OnChatMessage;
        Steamworks.SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;    
    }

    #endregion

    #region Lobby Callback Events
    public void OnLobbyCreated(Result result, Lobby lobby){
        if(result != Result.OK){
            #if UNITY_EDITOR
            Debug.LogWarning("Failed to create lobby: " + result);
            #endif
        }
        else{
            #if UNITY_EDITOR
            Debug.Log("Lobby Create Success!");
            #endif
            network.StartHost();

            lobby.SetData(HostAddressKey, SteamClient.SteamId.ToString());

            OnCreate.Invoke();
        }    
    }

    public async void OnLobbyMemberJoined(Lobby lobby, Friend friend){
        #if UNITY_EDITOR
        if(friend.Id != SteamClient.SteamId){
            Debug.Log($"{friend.Name} joined the lobby");
        }
        #endif
        GameObject obj = Instantiate(inLobbyFriend, playerContent);
        obj.name = friend.Name;
        obj.GetComponentInChildren<TMP_Text>().text = friend.Name;
        obj.GetComponentInChildren<RawImage>().texture = await SteamFriendsManager.GetTextureFromSteamIdAsync(friend.Id);
        inLobby.Add(friend.Id, obj);
    }

    public void OnLobbyMemberLeave(Lobby lobby, Friend friend){
        Destroy(inLobby[friend.Id].gameObject);
        inLobby.Remove(friend.Id);

        GameObject messageObj = Instantiate(chatMessage, chatContent);
        messageObj.GetComponentInChildren<TMP_Text>().text = $"<b>{friend.Name}</b> left the lobby!";
    }

    public async void OnLobbyEntered(Lobby lobby){
        //Everyone
        currentLobby = lobby;

        //Lobby UI Elements
        foreach(var friend in currentLobby.Members){
            if(friend.Id != SteamClient.SteamId){
                GameObject friendobj = Instantiate(inLobbyFriend, playerContent);
                friendobj.name = friend.Name;
                friendobj.GetComponentInChildren<TMP_Text>().text = friend.Name;
                friendobj.GetComponentInChildren<RawImage>().texture = await SteamFriendsManager.GetTextureFromSteamIdAsync(friend.Id);
                inLobby.Add(friend.Id, friendobj);
            }
        }   

        #if UNITY_EDITOR
            Debug.Log("Client Entered the Lobby!");
        #endif
        GameObject obj = Instantiate(inLobbyFriend, playerContent);
        obj.name = SteamClient.Name;
        obj.GetComponentInChildren<TMP_Text>().text = SteamClient.Name;
        obj.GetComponentInChildren<RawImage>().texture = await SteamFriendsManager.GetTextureFromSteamIdAsync(SteamClient.SteamId);
        inLobby.Add(SteamClient.SteamId, obj);      

        //Clients
        if(NetworkServer.active){return;}
        network.networkAddress = lobby.GetData(HostAddressKey);
        network.StartClient();
    }

    public async void OnGameLobbyJoinRequested(Lobby joinedLobby, SteamId id){
        RoomEnter joinedLobbySuccess = await joinedLobby.Join();
        if(joinedLobbySuccess != RoomEnter.Success){
            #if UNITY_EDITOR
            Debug.Log("Join Lobby Failed");
            #endif
        }
        else{
            currentLobby = joinedLobby;
            OnJoin.Invoke();
        }
    }

    public void OnChatMessage(Lobby lobby, Friend friend, string message){
        GameObject messageObj = Instantiate(chatMessage, chatContent);
        messageObj.GetComponentInChildren<TMP_Text>().text = $"<b>{friend.Name}</b> {message.TrimStart(' ').TrimStart('\n').TrimEnd(' ')}";
    }
    #endregion
    
    #region Custom Functions for Lobby
    public async void CreateLobbyAsync(TMP_Text name){
        bool result = await CreateLobby(name.text);
        if(!result){
            //UI ERROR
        }
    }

    // Create lobby function
    private async Task<bool> CreateLobby(string name){
        try
        {
            var createLobbyOutput = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);
            if(!createLobbyOutput.HasValue){
                #if UNITY_EDITOR
                Debug.LogWarning("Lobby Created but not Correctly Instatiated.");
                #endif
                return false;
            }

            currentLobby = createLobbyOutput.Value;
            currentLobby.SetPublic();
            currentLobby.SetJoinable(true);
            currentLobby.SetData("name", name);
            currentLobby.SetData("game", "tupi");

            foreach(var user in inLobby.Values){
                Destroy(user);
            }
            inLobby.Clear();

            return true;
        }
        catch (System.Exception e )
        {
            Debug.LogWarning("Failed to Create Lobby: " + e.Message);
            return false;
        }
    }
    
    public void SendChatMessage(TMP_Text message){
        bool result = false;

        if(message.text.Replace("\u200B", "").Replace("\n", "").TrimStart(' ').Length != 0 && Input.GetKeyDown(KeyCode.Return)){
            result = currentLobby.SendChatString(message.text);
        }

        if(result == false){
            #if UNITY_EDITOR
            Debug.LogWarning("Send Chat Message Failed!");
            #endif
        }
        else{
            chatInput.text = "";
            chatInput.ActivateInputField();
        }
    }
    

    public async void SearchForLobby(){
        Lobby[] foundLobbies = await SteamMatchmaking.LobbyList.WithKeyValue("game", "tupi").RequestAsync();

        foreach(Transform obj in lobbyContent){
            Destroy(obj.gameObject);
        }

        foreach(Lobby lobby in foundLobbies){
            var obj = Instantiate(lobbyObject, lobbyContent);
            var texts = obj.GetComponentsInChildren<TMP_Text>();
            texts[0].text = lobby.GetData("name");
            texts[1].text = $"{lobby.MemberCount} / {lobby.MaxMembers}";
            var buttons = obj.GetComponentsInChildren<Button>(); // 0 -> LobbyObject, 1 -> Join Button 
            buttons[1].onClick.AddListener(delegate {JoinLobby(lobby);}); 
        }
    }
    
    public async void JoinLobby(Lobby lobby){
        RoomEnter joinedLobbySuccess = await lobby.Join();
        if(joinedLobbySuccess != RoomEnter.Success){
            #if UNITY_EDITOR
            Debug.Log("Join Lobby Failed");
            #endif
        }
        else{
            currentLobby = lobby;
            OnJoin.Invoke();
        }
    }

    public void LeaveLobby(){
        try
        {   
            
            currentLobby.Leave();
            OnLeave.Invoke();
            foreach(var user in inLobby.Values){
                Destroy(user);
            }
            inLobby.Clear();
            
        }
        catch (System.Exception e)
        {
            #if UNITY_EDITOR
            Debug.LogError("Lobby Leave Failed: " + e.Message);
            #endif
        }
    }

    #endregion

    #region UI
    public void setLobbyNameText(TMP_Text name){
        name.text = currentLobby.GetData("name");
    }
    #endregion
}
