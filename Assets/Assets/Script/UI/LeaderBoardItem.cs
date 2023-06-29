using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Shapes2D;

public class LeaderBoardItem : MonoBehaviour
{
    public List<UnityEngine.Sprite> topRankImg = new List<UnityEngine.Sprite>();
    public GameObject topRankUI;
    public GameObject normalRankUI;
    public TMP_Text rankNumText;
    public TMP_Text nameText;
    public TMP_Text meritText;
    public TMP_Text rewardText;
    public Shapes2D.Shape userAvataImg; 

    public void SetData(int rankIndex, LeaderBoardPlayerPayload player){

        if(rankIndex < 3){
            topRankUI.SetActive(true);
            normalRankUI.SetActive(false);
            topRankUI.GetComponent<Image>().sprite = topRankImg[rankIndex];
        }else{
            topRankUI.SetActive(false);
            normalRankUI.SetActive(true);
            rankNumText.text = (rankIndex + 1).ToString();
        }
        StartCoroutine(loadSpriteImageFromUrl(userAvataImg, player.avata_url));
        nameText.text = player.name;
        meritText.text = player.merit.ToString();
        rewardText.text = player.reward.ToString();
    }

    public void Delete(){
        Destroy(gameObject);
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
        Debug.Log("Download failed");
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
