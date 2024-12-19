using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeCell : MonoBehaviour
{
    [SerializeField]
    private GameObject _leftWall;

    [SerializeField]
    private GameObject _rightWall;

    [SerializeField]
    private GameObject _frontWall;

    [SerializeField]
    private GameObject _backWall;

    [SerializeField]
    private GameObject _unvisitedBlock;

    [SerializeField]
    private Transform cellTransform;

    [SerializeField]
    private GameObject cellBackground;

    public bool IsVisited { get; private set; }

    public bool IsCleared { get; private set; }
    public bool IsRoomCell { get; private set; }
    public bool IsCenterCell { get; private set; }
    public float x { get; private set; }
    public float y { get; private set; }
    public HashSet<MazeCell> connections = new HashSet<MazeCell>();

    public void Visit() {
        IsVisited = true;
        _unvisitedBlock.SetActive(false);
    }

    public void ClearLeftWall() {
        _leftWall.SetActive(false);
    }

    public void ClearRightWall() {
        _rightWall.SetActive(false);
    }

    public void ClearFrontWall() {
        _frontWall.SetActive(false);
    }

    public void ClearBackWall() {
        _backWall.SetActive(false);
    }

    public void ChangeToRoomCell() {
        ClearAllWalls();
        IsRoomCell = true;
    }

    public void ChangeToCenterCell() {
        ClearAllWalls();
        int centerLayer = LayerMask.NameToLayer("Center");
        cellBackground.layer = centerLayer;
        IsCenterCell = true;
    }

    private void ClearAllWalls() {
        ClearLeftWall();
        ClearRightWall();
        ClearFrontWall();
        ClearBackWall();
    }

    public void RemoveRandomWall()
    {
        int wall = Random.Range(0, 3);
        if (wall == 0) ClearLeftWall();
        else if (wall == 1) ClearRightWall();
        else if (wall == 2) ClearFrontWall();
        else if (wall == 3) ClearBackWall();
    }

    public Vector2 GetWorldPosition()
    {
        return cellTransform.position;
    }

    public void AddConnection(MazeCell cell)
    {
        connections.Add(cell);
    }

    public void DrawConnections()
    {
        foreach (MazeCell connection in connections)
        {
            Debug.DrawLine(GetWorldPosition(), connection.GetWorldPosition(), Color.red, 100f);
        }
    }

    public void SetPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public void Delete()
    {
        Destroy(gameObject);
    }
}
