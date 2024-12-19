using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChatMessageInputField : MonoBehaviour
{
    public TMP_InputField inputField; // Assign the TMP Input Field in the Inspector

    void Start()
    {
        inputField.onEndEdit.AddListener(HandleEndEdit);
    }

    void HandleEndEdit(string text)
    {
        // Check if Enter was pressed
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ChatManager.instance.SendChatMessage(text);
            inputField.text = "";
            inputField.Select();
            inputField.ActivateInputField();
        }
    }
}
