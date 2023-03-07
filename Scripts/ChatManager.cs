using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChatManager : MonoBehaviour
{
    public static ChatManager instance;
    public List<ChatItem> chatItems = new List<ChatItem>();
    public Transform chatContent;
    public Animator animator;
    private bool extendedChatPanel;

    public TMP_InputField inputField;

    //TMP_IF.interactable = true;

    //TMP_IF.text = "whatever you want";
    // nameField.Select();

    public RectTransform scrollViewRect;
    public void Awake()
    {
        instance = this;
        extendedChatPanel = false;

    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.Return)){
            SendGlobalChat(inputField.text);
            inputField.text = "";
            inputField.Select();
            inputField.ActivateInputField();
        }
    }

    public void Reset()
    {
        int count = chatItems.Count;
        for (int i = 0; i < count; i++)
        {

            chatItems[i].Delete();
        }
        chatItems.Clear();
    }

    private void OnEnable()
    {
       
    }

    private void OnDisable()
    {

    }

    private void AddChatItem(GlobalChatPayload payload)
    {
        GameObject chatItemObj = GameObject.Instantiate(Resources.Load("items/ChatItem"), chatContent) as GameObject;
        chatItemObj.GetComponent<ChatItem>().SetData(payload);
        chatItemObj.transform.SetParent(chatContent);
        chatItems.Add(chatItemObj.GetComponent<ChatItem>());
        AdjustChatContentList();
    }

    private void AdjustChatContentList()
    {
        float stackChatItemHeight = 0;
        foreach (var item in chatItems)
        {
            stackChatItemHeight += item.GetHeight();
        }

        if(scrollViewRect.rect.height < stackChatItemHeight){
            chatItems[0].Delete();
            chatItems.RemoveAt(0);
        }
    }

    public void VisibleChatPanel(bool visible){
        animator.SetBool("show_panel", visible);
        extendedChatPanel = visible;
        if(visible)
        {
            inputField.Select();
        }
    }

    public void SendGlobalChat(string chatText)
    {
        if(chatText.Trim() == "")
        {
            return;
        }
        //GlobalChatPayload payload = new GlobalChatPayload("killer", "", 54, chatText);
        //ReceiveGlobalChat(payload);
        SocketGameManager.instance.SendGlobalChat(chatText);
    }

    IEnumerator delay(GlobalChatPayload payload)
    {
        yield return new WaitForSeconds(0.5f);
        ReceiveGlobalChat(payload);
    }

    public void ReceiveGlobalChat(GlobalChatPayload payload)
    {
        if(!extendedChatPanel)
        {
            animator.Play("receive_chat", 0, 0);
        }
        //inputField.Select();
        AddChatItem(payload);
    }

}
