using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using System;

public class HostGameUI : MonoBehaviour
{
    public GameObject connectMenu;
    public TMP_InputField PortInput;
    public TMP_InputField UsernameInput;

    [SerializeField]
    private GameObject networkButtons;

    [SerializeField]
    private GameObject startGameButton;

    public void ToggleMenu()
    {
        if (connectMenu != null)
        {
            if (connectMenu.activeSelf)
            {
                connectMenu.SetActive(false);
            }
            else
            {
                connectMenu.SetActive(true);
            }
        }
    }

    public void Host()
    {
        ToggleMenu();
        String username = convertFieldToString(UsernameInput);

        if (!IsValidUsername(username)) {
            Debug.Log("Invalid username typed in.");
            return;
        }

        int port;
        if (int.TryParse(convertFieldToString(PortInput), out port))
        {
            Debug.Log($"Parsed value: {port}");
        }
        else
        {
            Debug.Log("The input could not be parsed as an integer.");
            return;
        }

        NetworkController networkController = NetworkController.instance;
        networkController.HostGame(port, username);
        networkButtons.SetActive(false);
        startGameButton.SetActive(true);
    }

    private string convertFieldToString(TMP_InputField tmpComponent)
    {
        string text = tmpComponent.text;
        return text;
    }

    private static bool IsValidUsername(string s)
    {
        if (s.Length > 12 || s.Length <= 0) {
            return false;
        }

        foreach (var c in s)
        {
            if (!char.IsLetterOrDigit(c))
            {
                return false;
            }
        }

        return true;
    }
}
