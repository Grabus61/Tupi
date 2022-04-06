using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Epic.OnlineServices.Lobby;
using UnityEngine.UI;
using TMPro;

public class CustomLobbyUI : EOSLobbyUI
{
    private void OnEnable()
    {
        base.OnEnable();
        FindLobbiesSucceeded += OnLobbyFoundSucceeded;
    }

    private void OnDisable()
    {
        base.OnDisable();
        FindLobbiesSucceeded -= OnLobbyFoundSucceeded;
    }
    public void Create(TMP_Text code)
    {
        string name = code.text;
        LobbyPermissionLevel permissionLevel = LobbyPermissionLevel.Publicadvertised;
        AttributeData[] data =  new AttributeData[] { new AttributeData { Key = AttributeKeys[0], Value = name }, new AttributeData { Key = AttributeKeys[1], Value = code.text } };
        CreateLobby(8, permissionLevel, false, data);
        Debug.Log("Lobby Created");
    }

    public void Join(TMP_Text code)
    {
        LobbySearch search = new LobbySearch();
        LobbySearchSetParameterOptions options = new LobbySearchSetParameterOptions()
        {
            Parameter = new AttributeData() { Key = AttributeKeys[1], Value = code.text },
            ComparisonOp = 0
        };
        search.SetParameter(options);
        FindLobbies(search);
    }

    void OnLobbyFoundSucceeded(List<LobbyDetails> detailsList)
    {
        JoinLobby(detailsList[0], AttributeKeys);
    }
}
