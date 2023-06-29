using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Shapes2D;

public class ChatItem : MonoBehaviour
{
    public Shapes2D.Shape userAvatarImg;
    public TMP_Text chatTextUI;
    public TMP_Text rankTextUI;
    public TMP_Text senderNameTextUI;

    private void Start() {
        GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }
    public void Delete()
    {
        Destroy(gameObject);
    } 

    public void SetData(GlobalChatPayload payload)
    {
        StartCoroutine(loadSpriteImageFromUrl(userAvatarImg, payload.avatarUrl));
        chatTextUI.text = payload.chatText;
        rankTextUI.text = payload.senderRank == -1 ? "-" : payload.senderRank.ToString();
        senderNameTextUI.text = payload.senderName;
    }

    public float GetHeight()
    {
        return gameObject.GetComponent<RectTransform>().rect.height;
    }

    IEnumerator loadSpriteImageFromUrl(Shapes2D.Shape target, string URL)
    {

        WWW www = new WWW(URL);
        while (!www.isDone)
        {
        // Debug.Log("Download image on progress" + www.progress);
        yield return null;
        }

        if (!string.IsNullOrEmpty(www.error))
        {
        Debug.Log("[ChatItem.loadSpriteImageFormUrl] Download failed");
        }
        else
        {
        Debug.Log("Download succes");
        Texture2D texture = new Texture2D(1, 1);
        www.LoadImageIntoTexture(texture);

        target.settings.fillTexture = texture;
        }
    }
}
