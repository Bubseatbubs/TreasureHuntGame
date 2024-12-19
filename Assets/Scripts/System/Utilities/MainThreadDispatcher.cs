using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

/*
Unity has a limitation on threads. Nothing related to the Unity API can be run
on a thread. As such, the purpose of this class is for threads to make calls to
the main thread to run a specific function that uses the Unity API.
*/
public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> executionQueue = new Queue<Action>();

    // Singleton instance of the dispatcher
    public static MainThreadDispatcher instance;

    void Awake()
    {
        instance = this;
    }

    // Enqueue an action to be executed on the main thread
    public void Enqueue(Action action)
    {
        // Lock while executing so other threads can't access
        lock (executionQueue)
        {
            executionQueue.Enqueue(action);
        }
    }

    // Execute all actions in the queue on the main thread in the Update method
    private void Update()
    {
        lock (executionQueue)
        {
            while (executionQueue.Count > 0)
            {
                executionQueue.Dequeue()?.Invoke();
            }
        }
    }
}