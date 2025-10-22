using System.Collections.Generic;
using UnityEngine;

public class PatternLibrary : MonoBehaviour
{
    public PatternAsset[] patterns;
    public GridManager grid;
    public MatchManager match;
    public Simulation sim; // NEW: чтобы знать, идёт ли Placement

    public PatternAsset ActivePattern { get; private set; }

    public void ArmPattern(PatternAsset p)  => ActivePattern = p;
    public void ClearActivePattern()        => ActivePattern = null;

    // Якорь: верхний-левый (как мы настроили ранее)
    public void PlacePattern(PatternAsset p, Vector2Int anchorTL)
    {
        var lines = p.ascii.Replace("\r","").Split('\n');
        int ph = Mathf.Min(lines.Length, p.height);

        // Собираем кандидатов (только живые точки паттерна, в границах и на пустых клетках)
        var candidates = new List<Vector2Int>();

        for (int y = 0; y < ph; y++)
        {
            var line = lines[y];
            int pw = Mathf.Min(line.Length, p.width);

            for (int x = 0; x < pw; x++)
            {
                if (line[x] != 'O') continue;

                int gx = anchorTL.x + x;   // top-left anchor
                int gy = anchorTL.y - y;   // ASCII идёт сверху вниз

                if (!grid.InBounds(gx, gy)) continue;
                if (grid.AliveNow[gx, gy]) continue; // не перекрываем существующие

                candidates.Add(new Vector2Int(gx, gy));
            }
        }

        if (candidates.Count == 0) return;

        // Определяем владельца
        var owner = match.PvPEnabled
            ? (match.Turn == PlayerTurn.White ? Owner.White : Owner.Black)
            : Owner.White;

        int toPlace = candidates.Count;

        // Если Placement (PvP расстановка) — ограничиваем по оставшимся сидам
        if (match.PvPEnabled && sim != null && sim.State == SimState.Placement)
        {
            int seedsLeft = match.SeedsLeftForCurrentPlayer();
            if (seedsLeft <= 0) return;
            toPlace = Mathf.Min(toPlace, seedsLeft);
        }

        int placed = 0;
        for (int i = 0; i < toPlace; i++)
        {
            var c = candidates[i];
            grid.SetCell(c.x, c.y, true, owner, instant: true);
            placed++;
        }

        // Списываем семена и переключаем ход/завершаем расстановку
        if (match.PvPEnabled && sim != null && sim.State == SimState.Placement)
        {
            match.ConsumeSeeds(placed);
        }
    }
}
