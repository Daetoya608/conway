// MatchManager.cs
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public GridManager grid;
    public Simulation sim;
    public int seedsPerPlayer = 20;

    public int WhiteScore { get; private set; }
    public int BlackScore { get; private set; }

    public PlayerTurn Turn { get; private set; } = PlayerTurn.White;
    public int WhiteSeedsLeft { get; private set; }
    public int BlackSeedsLeft { get; private set; }

    public bool PvPEnabled { get; private set; }
    public System.Action OnPlacementFinished;
    public System.Action<string> OnMatchEnded;

	public System.Action OnTurnChanged;


	void ToggleTurn()
	{
    	Turn = (Turn == PlayerTurn.White) ? PlayerTurn.Black : PlayerTurn.White;
    	OnTurnChanged?.Invoke();
	}

	void SetTurn(PlayerTurn t)
	{
    	Turn = t;
    	OnTurnChanged?.Invoke();
	}


	// Узнать, сколько сидов осталось у активного игрока
	public int SeedsLeftForCurrentPlayer()
	{
    	return (Turn == PlayerTurn.White) ? WhiteSeedsLeft : BlackSeedsLeft;
	}

	// Списать N сидов у активного игрока, сменить ход и завершить расстановку при необходимости
	public void ConsumeSeeds(int count)
	{
    	if (!PvPEnabled || sim.State != SimState.Placement) return;
    	if (count <= 0) return;

    	if (Turn == PlayerTurn.White)
        	WhiteSeedsLeft = Mathf.Max(0, WhiteSeedsLeft - count);
    	else
        	BlackSeedsLeft = Mathf.Max(0, BlackSeedsLeft - count);

    	// Оба закончили — конец placement
    	if (WhiteSeedsLeft <= 0 && BlackSeedsLeft <= 0)
    	{
        	sim.SetState(SimState.Editing);
        	OnPlacementFinished?.Invoke();
        	return;
    	}

    	// Если у обоих остались — чередуем ход
    	if (WhiteSeedsLeft > 0 && BlackSeedsLeft > 0)
    	{
        	ToggleTurn();
    	}
    	else
    	{
        	// У одного сидов нет — ход у того, у кого ещё есть
        	if (WhiteSeedsLeft <= 0 && BlackSeedsLeft > 0) SetTurn(PlayerTurn.Black);
        	if (BlackSeedsLeft <= 0 && WhiteSeedsLeft > 0) SetTurn(PlayerTurn.White);
    	}
	}


    void Awake()
    {
        sim.OnScored += (w,b) => { WhiteScore+=w; BlackScore+=b; CheckEnd(); };
    }

    public void NewPvPMatch(int seeds)
    {
        PvPEnabled = true;
        WhiteScore = BlackScore = 0;
        WhiteSeedsLeft = BlackSeedsLeft = seeds;
        SetTurn(PlayerTurn.White);
    }

    public void EndMatchByButton()
    {
        EndMatch();
    }

    void CheckEnd()
    {
        if (!PvPEnabled) return;

        // Нет живых?
        if (!AnyAlive())
        {
            EndMatch();
        }
    }

    bool AnyAlive()
    {
        for (int x=0; x<grid.width; x++)
        for (int y=0; y<grid.height; y++)
            if (grid.AliveNow[x,y]) return true;
        return false;
    }

    void EndMatch()
	{
    	PvPEnabled = false;

    	string result = (WhiteScore > BlackScore) ? "Победили Зеленые" :
                    	(BlackScore > WhiteScore) ? "Победили Оранжевые" :
                    	"Ничья";

    	// показываем сообщение
    	OnMatchEnded?.Invoke($"{result}\nЗеленые: {WhiteScore} — Оранжевые: {BlackScore}");

    	sim.Pause();

    	// 🔄 сброс очков после матча
    	WhiteScore = 0;
    	BlackScore = 0;
    	WhiteSeedsLeft = 0;
    	BlackSeedsLeft = 0;
    	Turn = PlayerTurn.White;
}


    public bool TryPlaceAt(Vector2Int index)
	{
    	if (!PvPEnabled || sim.State != SimState.Placement) return false;
    	if (grid.AliveNow[index.x, index.y]) return false; // занято

    	var owner = (Turn == PlayerTurn.White) ? Owner.White : Owner.Black;
    	grid.SetCell(index.x, index.y, true, owner, instant: true);

    	// списываем 1 сид у текущего игрока
    	if (Turn == PlayerTurn.White) WhiteSeedsLeft--;
    	else BlackSeedsLeft--;

    	// оба закончили — завершаем расстановку
    	if (WhiteSeedsLeft <= 0 && BlackSeedsLeft <= 0)
    	{
        	sim.SetState(SimState.Editing);
        	OnPlacementFinished?.Invoke(); // UI запустит симуляцию/что нужно
        	return true;
    	}

    	// если у обоих остались сиды — чередуем ход каждый раз
    	if (WhiteSeedsLeft > 0 && BlackSeedsLeft > 0)
    	{
        	ToggleTurn();
    	}
    	else
    	{
        	// у одного сидов нет — ход у того, у кого ещё есть
        	if (WhiteSeedsLeft <= 0 && BlackSeedsLeft > 0) SetTurn(PlayerTurn.Black);
        	if (BlackSeedsLeft <= 0 && WhiteSeedsLeft > 0) SetTurn(PlayerTurn.White);
    	}
    	return true;
	}


	public void DisablePvP()
	{
    	PvPEnabled = false;
    	Turn = PlayerTurn.White;      // сброс хода на дефолт
    	// (опц.) можно обнулить очки/семена, если нужно жёстко рестартить:
    	WhiteScore = 0; BlackScore = 0;
    	WhiteSeedsLeft = 0; BlackSeedsLeft = 0;
	}

}
