using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using System;

/*
Unity has a limitation on threads. Nothing related to the Unity API can be run
on a thread. As such, the purpose of this class is for threads to make calls to
the main thread to run a specific function that uses the Unity API.
*/
public class RandomSeed : MonoBehaviour
{
    public int _seed {get; private set;}

    // Singleton instance of the dispatcher
    public static RandomSeed instance;

    void Start()
    {
        instance = this;
    }

    // Generates a seed if host, or gets seed from host if client
    public void InitializeSeed() {
        if (NetworkController.isHost)
        {
            _seed = UnityEngine.Random.Range(1000000, 9999999);
            Debug.Log($"Seed: {_seed}");
            UnityEngine.Random.InitState(_seed);
        }
        else
        {
            _seed = RequestSeed();

            Debug.Log($"Seed: {_seed}");
            UnityEngine.Random.InitState(_seed);
        }
    }

    int RequestSeed()
    {
        int s = 0;
        string message = TCPConnection.instance.SendAndReceiveDataFromHost("RandomSeed:SendSeed");
        try {
            s = int.Parse(message);
        }
        catch (FormatException) {

        }

        return s;
    }

    public static void SendSeed()
    {
        TCPHost.instance.SendDataToClients($"{instance._seed}");
    }
}