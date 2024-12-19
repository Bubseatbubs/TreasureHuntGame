using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StartGameButton : MonoBehaviour
{
    [SerializeField]
    private GameObject button;

    public void BeginGame()
    {
        TCPHost.instance.SendDataToClients("WaitForHostPanel:BeginGame");
        NetworkController.instance.StartGame();

        button.SetActive(false);
    }
}
