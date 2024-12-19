using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForHostPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject waitForHostPanel;

    public static GameObject WFSPInstance;

    void Start()
    {
        WFSPInstance = waitForHostPanel;
    }

    public static void BeginGame()
    {
        NetworkController.instance.StartGame();
        WFSPInstance.SetActive(false);
    }
}
