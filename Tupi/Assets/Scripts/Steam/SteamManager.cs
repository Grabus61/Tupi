using UnityEngine;
using Steamworks;

public class SteamManager : MonoBehaviour
{
    public uint appID = 480; //480 = spacewar

    private void Start() {
        try
        {
            Steamworks.SteamClient.Init(appID, true);
            Debug.Log("Steam Connection Success!");
        }
        catch ( System.Exception e )
        {
            Debug.LogError("Cannot connect to steam: " + e.Message);
        }
    }

    void Update()
    {
        Steamworks.SteamClient.RunCallbacks();
    }

    private void OnApplicationQuit() {
        try
        {
            Steamworks.SteamClient.Shutdown();
            Debug.Log("Steam Shutdown Success!");
        }
        catch ( System.Exception e )
        {
            Debug.LogError("Cannot shutdown: " + e.Message);
        }
    }
}
