using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeRoom : MonoBehaviour
{
    [SerializeField]
    private GameObject _roomBody;

    public void Delete()
    {
        Destroy(gameObject);
    }
}
