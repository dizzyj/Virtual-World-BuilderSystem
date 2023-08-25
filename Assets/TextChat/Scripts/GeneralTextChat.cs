using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GeneralTextChat : MonoBehaviour
{

    public int maxMessages = 20; //# of messages to be stored

    public string playerName;
    public GameObject chatPanel, textObject;
    public InputField chatInput;

    public Color playerMessageColor, infoMessageColor; //color of message

    [SerializeField]
    List<Message> messageList = new List<Message>();
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(chatInput.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return)) //send message by hitting Enter key
            {
                //send message typed in input box
                SendMessage(playerName + ": " + chatInput.text, Message.MessageType.playerMessage);
                //reset input box text to be blank
                chatInput.text = "";
            }
        }
        else
        {
            //open the chat window if Enter is pressed
            if (!chatInput.isFocused && Input.GetKeyDown(KeyCode.Return))
            {
                chatInput.ActivateInputField();
            }
        }

        //test the different message color
        if (!chatInput.isFocused)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                SendMessage("pressed tab", Message.MessageType.info);
            }
        }
        
    }

    //send the message typed into the chat window
    public void SendMessage(string text, Message.MessageType messageType)
    {
        //delete the oldest message
        if(messageList.Count >= maxMessages)
        {
            Destroy(messageList[0].textObject.gameObject); //destroy the object as well
            messageList.Remove(messageList[0]);
        }
        
        Message newMessage = new Message();
        newMessage.text = text;

        GameObject newText = Instantiate(textObject, chatPanel.transform); //create clone of textObject, will become the new message

        newMessage.textObject = newText.GetComponent<Text>(); //set the textObject to the clone

        newMessage.textObject.text = newMessage.text; //set the text of the message
        newMessage.textObject.color = MessageTypeColor(messageType); //set the message color

        messageList.Add(newMessage);
    }

    //choose the color of text for the message
    Color MessageTypeColor(Message.MessageType messageType)
    {
        Color color = infoMessageColor;

        //determines the color of the message
        switch (messageType)
        {
            case Message.MessageType.playerMessage:
                color = playerMessageColor;
                break;
        }

        return color;
    }

    [System.Serializable]
    public class Message
    {
        public string text;
        public Text textObject;
        public MessageType messageType;
        public enum MessageType
        {
            playerMessage,
            info
        }
    }
}
