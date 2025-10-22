// CameraController2D.cs
using UnityEngine;

public class CameraController2D : MonoBehaviour
{
    public void FrameGrid(GridManager grid)
    {
        var cam = GetComponent<Camera>();
        cam.orthographicSize = Mathf.Max(grid.width, grid.height) * grid.cellSize * 0.55f;
        transform.position = new Vector3(0,0,-10);
    }
}