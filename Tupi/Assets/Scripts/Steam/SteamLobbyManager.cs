using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Steamworks;
using Steamworks.Data;
using TMPro;

public class SteamLobbyManager : MonoBehaviour
{
    private Lobby currentLobby;
    public int maxPlayers = 4;

    public UnityEvent OnCreate;
    public UnityEvent OnJoin;
    public UnityEvent OnLeave;

    //Objects for lobby
    public GameObject inLobbyFriend;
    public Transform playerContent;

    public GameObject chatMessage;
    public Transform chatContent;
    public TMP_InputField chatInput;

    Dictionary<SteamId, GameObject> inLobby = new Dictionary<SteamId, GameObject>();

    private void Start() {
        Steamworks.SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        Steamworks.SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        Steamworks.SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        Steamworks.SteamMatchmaking.OnChatMessage += OnChatMessage;
        Steamworks.SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;    
    }

    #region Lobby Callback Events
    public void OnLobbyCreated(Result result, Lobby lobby){
        if(result != Result.OK){
            Debug.LogWarning("Failed to create lobby: " + result);
        }
        else{
            Debug.Log("Lobby Create Success!");
            OnCreate.Invoke();
        }    
    }

    public async void OnLobbyMemberJoined(Lobby lobby, Friend friend){
        Debug.Log($"{friend.Name} joined the lobby");
        GameObject obj = Instantiate(inLobbyFriend, playerContent);
        obj.GetComponentInChildren<Text>().text = friend.Name;
        obj.GetComponentInChildren<RawImage>().texture = await SteamFriendsManager.GetTextureFromSteamIdAsync(friend.Id);
        inLobby.Add(friend.Id, obj);
    }

    public async void OnLobbyEntered(Lobby lobby){
        Debug.Log("Client Entered the Lobby!");
        GameObject obj = Instantiate(inLobbyFriend, playerContent);
        obj.GetComponentInChildren<TMP_Text>().text = SteamClient.Name;
        obj.GetComponentInChildren<RawImage>().texture = await SteamFriendsManager.GetTextureFromSteamIdAsync(SteamClient.SteamId);
        inLobby.Add(SteamClient.SteamId, obj);

        foreach(var friend in currentLobby.Members){
            if(friend.Id != SteamClient.SteamId){
                GameObject friendobj = Instantiate(inLobbyFriend, playerContent);
                friendobj.GetComponentInChildren<Text>().text = friend.Name;
                friendobj.GetComponentInChildren<RawImage>().texture = await SteamFriendsManager.GetTextureFromSteamIdAsync(friend.Id);
                inLobby.Add(friend.Id, friendobj);
            }
        }

        OnJoin.Invoke();
    }

    public async void OnGameLobbyJoinRequested(Lobby joinedLobby, SteamId id){
        RoomEnter joinedLobbySuccess = await joinedLobby.Join();
        if(joinedLobbySuccess != RoomEnter.Success){
            Debug.Log("Join Lobby Failed");
        }
        else{
            currentLobby = joinedLobby;
        }
    }

    public void OnChatMessage(Lobby lobby, Friend friend, string message){
        GameObject messageObj = Instantiate(chatMessage, chatContent);
        messageObj.GetComponentInChildren<TMP_Text>().text = $"<b>{friend.Name}</b> {message.TrimStart(' ')}";
    }
    #endregion
    
    public async void CreateLobbyAsync(TMP_Text name){
        bool result = await CreateLobby(name.text);
        if(!result){
            //UI ERROR
        }

        Debug.Log(currentLobby.Owner);
    }

    // Create lobby function
    private async Task<bool> CreateLobby(string name){
        try
        {
            var createLobbyOutput = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);
            if(!createLobbyOutput.HasValue){
                Debug.LogWarning("Lobby Created but not Correctly Instatiated.");
                return false;
            }

            currentLobby = createLobbyOutput.Value;
            currentLobby.SetPublic();
            currentLobby.SetJoinable(true);
            currentLobby.SetData("name", name);
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

        if(message.text.Length != 1 && Input.GetKeyDown(KeyCode.Return)){
            result = currentLobby.SendChatString(message.text);
        }

        if(result == false){
            Debug.LogWarning("Send Chat Message Failed!");
        }
        else{
            chatInput.text = "";
            chatInput.ActivateInputField();
        }
        
    }
    
    
    #region UI
    public void setLobbyNameText(TMP_Text name){
        name.text = currentLobby.GetData("name");
    }
    #endregion
}
