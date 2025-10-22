// GridManager.cs
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid")]
    public int width = 50;
    public int height = 30;
    public float cellSize = 0.5f;
    public bool wrap = false;

    [Header("Refs")]
    public Cell cellPrefab;

    public Cell[,] Cells { get; private set; }
    public bool[,] AliveNow { get; private set; }
    public Owner[,] Owners { get; private set; }

    Transform _root;

	[ContextMenu("Create Grid")]
    public void CreateGrid()
    {
        ClearGrid();

        AliveNow = new bool[width, height];
        Owners   = new Owner[width, height];
        Cells    = new Cell[width, height];

        _root = new GameObject("CellsRoot").transform;
        _root.SetParent(transform, false);

        Vector2 origin = new Vector2(-width * cellSize * 0.5f, -height * cellSize * 0.5f);

        for (int x=0; x<width; x++)
        for (int y=0; y<height; y++)
        {
            var c = Instantiate(cellPrefab, _root);
            c.transform.localPosition = origin + new Vector2((x+0.5f)*cellSize, (y+0.5f)*cellSize);
            c.transform.localScale = Vector3.one * (cellSize * 0.95f);
            c.Init(new Vector2Int(x,y));
            Cells[x,y] = c;
        }
    }

	[ContextMenu("Clear Grid")]
    public void ClearGrid()
    {
        if (_root != null) DestroyImmediate(_root.gameObject);
    }

    public void SetCell(int x,int y, bool alive, Owner owner, bool instant=false)
    {
        AliveNow[x,y] = alive;
        Owners[x,y]   = alive ? owner : Owner.None;
        Cells[x,y].SetVisual(alive, Owners[x,y], instant);
    }

    public bool InBounds(int x,int y) => x>=0 && y>=0 && x<width && y<height;

    public void Randomize(float fill01 = 0.15f)
    {
        var rand = new System.Random();
        for (int x=0; x<width; x++)
        for (int y=0; y<height; y++)
        {
            bool alive = rand.NextDouble() < fill01;
            var owner = alive ? (rand.Next(2)==0 ? Owner.White : Owner.Black) : Owner.None;
            SetCell(x,y,alive,owner, instant:true);
        }
    }

	public void Randomize(float fill01 = 0.15f, bool singleColor = false)
	{
    	var rand = new System.Random();
    	for (int x=0; x<width; x++)
    	for (int y=0; y<height; y++)
    	{
        	bool alive = rand.NextDouble() < fill01;
        	Owner owner = Owner.None;
        	if (alive)
        	{
            	owner = singleColor ? Owner.White
                                	: (rand.Next(2)==0 ? Owner.White : Owner.Black);
        	}
        	SetCell(x,y, alive, owner, instant:true);
    	}
	}


    public (int aliveCount, int white, int black) CountNeighbors(int cx,int cy)
    {
        int alive=0, w=0, b=0;
        for (int dx=-1; dx<=1; dx++)
        for (int dy=-1; dy<=1; dy++)
        {
            if (dx==0 && dy==0) continue;
            int nx = cx+dx, ny=cy+dy;
            if (wrap)
            {
                nx = (nx + width) % width;
                ny = (ny + height) % height;
            }
            if (!InBounds(nx,ny)) continue;

            if (AliveNow[nx,ny])
            {
                alive++;
                if (Owners[nx,ny]==Owner.White) w++;
                else if (Owners[nx,ny]==Owner.Black) b++;
            }
        }
        return (alive, w, b);
    }

	void Start()
	{
    	// Создаём сетку автоматически, если ещё не создана
    	if (Cells == null || AliveNow == null || Owners == null)
        	CreateGrid();
	}
}
