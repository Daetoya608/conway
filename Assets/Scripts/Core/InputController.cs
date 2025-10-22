// InputController.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class InputController : MonoBehaviour
{
    public Camera cam;
    public GridManager grid;
    public Simulation sim;
    public MatchManager match;
	public PatternLibrary patterns;


    void Update()
    {
        // 0) если сетка ещё не создана — игнор кликов
        if (grid.Cells == null) return;

        // 1) Зум
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll, 2f, 100f);
        }

        // 2) Панорамирование (пробел + ЛКМ)
        if (Input.GetKey(KeyCode.Space) && Input.GetMouseButton(0))
        {
            Vector3 delta = -new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0) * (cam.orthographicSize * 0.1f);
            cam.transform.position += delta;
            return;
        }

        // 3) Клик мыши
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            // если клик по UI — ничего не делаем
            if (IsPointerOverUI()) return;

            // В PvP режиме запрещаем редактирование, если не Placement
			if (match.PvPEnabled && sim.State != SimState.Placement) return;

            // координаты мира → индекс клетки
            Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0;
            var idx = WorldToIndex(world);
            if (idx.x < 0) return; // вне поля

            // 3a) Если «вооружён» паттерн — ставим его и выходим
            if (patterns != null && patterns.ActivePattern != null)
            {
                patterns.PlacePattern(patterns.ActivePattern, idx);
                patterns.ClearActivePattern();
                return;
            }

            // 3b) Фаза расстановки (PvP placement)
            if (sim.State == SimState.Placement)
            {
                if (Input.GetMouseButtonDown(0))
                    match.TryPlaceAt(idx);
                return;
            }

            // 3c) Обычное редактирование в паузе
            if (Input.GetMouseButtonDown(0))
            {
                // ЛКМ — оживить; вне PvP всегда белые
                var owner = match.PvPEnabled ?
                    ((match.Turn == PlayerTurn.White) ? Owner.White : Owner.Black)
                    : Owner.White;
                grid.SetCell(idx.x, idx.y, true, owner);
            }
            else if (Input.GetMouseButtonDown(1))
            {
                // ПКМ — убить
                grid.SetCell(idx.x, idx.y, false, Owner.None);
            }
        }
    }

	// NEW: игнорируем клики, если курсор над UI
    bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    // UPDATED: учитываем позицию Grid в мире
    Vector2Int WorldToIndex(Vector3 world)
    {
        Vector2 gridPos = (Vector2)grid.transform.position;
        Vector2 originWorld = gridPos + new Vector2(
            -grid.width * grid.cellSize * 0.5f,
            -grid.height * grid.cellSize * 0.5f
        );

        Vector2 local = (Vector2)world - originWorld;
        int x = Mathf.FloorToInt(local.x / grid.cellSize);
        int y = Mathf.FloorToInt(local.y / grid.cellSize);
        if (!grid.InBounds(x, y)) return new Vector2Int(-1, -1);
        return new Vector2Int(x, y);
    }
}