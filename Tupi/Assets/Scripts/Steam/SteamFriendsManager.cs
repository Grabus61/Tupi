using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SteamFriendsManager : MonoBehaviour
{
    public RawImage profilePicture;
    public TMP_Text nickname;

    async void Start(){
        if(!SteamClient.IsValid) return; // return if steam connection fails
        
        nickname.text = SteamClient.Name;
        Debug.Log(SteamClient.Name);

        var ppImage = await SteamFriends.GetLargeAvatarAsync(SteamClient.SteamId);
        profilePicture.texture = GetTextureFromImage(ppImage.Value);
    }

    // Convert steam image to in game texture
    public static Texture2D GetTextureFromImage(Steamworks.Data.Image image){
        Texture2D texture = new Texture2D((int)image.Width, (int)image.Height);

        for(int x = 0; x < image.Width; x++){
            for(int y = 0; y < image.Height; y++){
                Steamworks.Data.Color p = image.GetPixel(x,y);
                texture.SetPixel(x, (int)image.Height - y, new Color(p.r / 255.0f, p.g / 255.0f, p.b / 255.0f, p.a / 255.0f));
            }
        }
        texture.Apply();

        return texture;
    }

    public static async System.Threading.Tasks.Task<Texture2D> GetTextureFromSteamIdAsync(SteamId id) {
        var img = await SteamFriends.GetLargeAvatarAsync(SteamClient.SteamId);
        Steamworks.Data.Image image = img.Value;

        Texture2D texture = new Texture2D((int) image.Width, (int)image.Height);
        
        for (int x = 0; x < image.Width; x++)
            for (int y = 0; y < image. Height; y++){
                var p = image.GetPixel(x, y);
                texture. SetPixel(x, (int)image. Height - y, new Color(p.r / 255.0f, p.g / 255.0f, p.b / 255.0f, p.a / 255.0f));
            }
        texture.Apply();
        return texture;
    }
}
