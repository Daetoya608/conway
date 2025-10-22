// Simulation.cs
using System.Collections;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    public GridManager grid;
    public float stepDelay = 0.1f;
    public SimState State { get; private set; } = SimState.Editing;

    bool[,] _nextAlive;
    Owner[,] _nextOwner;

    public System.Action<int,int> OnScored; // (whiteDelta, blackDelta)

    Coroutine _loop;

    public void StartSim()
    {
        if (State == SimState.Running) return;
        State = SimState.Running;
        _loop = StartCoroutine(Run());
    }

    public void Pause()
    {
        if (_loop!=null) StopCoroutine(_loop);
        _loop=null;
        State = SimState.Editing;
    }

    public void StepOnce()
    {
        ApplyStep();
    }

    IEnumerator Run()
    {
        while (true)
        {
            ApplyStep();
            yield return new WaitForSeconds(stepDelay);
        }
    }

    void ApplyStep()
    {
        int wScore=0, bScore=0;
        int W = grid.width, H = grid.height;
        _nextAlive ??= new bool[W,H];
        _nextOwner ??= new Owner[W,H];

        for (int x=0; x<W; x++)
        for (int y=0; y<H; y++)
        {
            var (aliveN, w, b) = grid.CountNeighbors(x,y);
            bool alive = grid.AliveNow[x,y];
            Owner owner = grid.Owners[x,y];

            if (alive)
            {
                bool survive = aliveN==2 || aliveN==3;
                _nextAlive[x,y] = survive;
                _nextOwner[x,y] = survive ? owner : Owner.None;
            }
            else
            {
                bool born = (aliveN==3);
                if (born)
                {
                    Owner bornOwner = (w>b) ? Owner.White :
                                      (b>w) ? Owner.Black : Owner.None;
                    _nextAlive[x,y] = bornOwner != Owner.None; // если ровно 1 и 1 и 1 — не бывает, но оставим проверку
                    _nextOwner[x,y] = bornOwner;

                    if (bornOwner==Owner.White) wScore++;
                    else if (bornOwner==Owner.Black) bScore++;
                }
                else
                {
                    _nextAlive[x,y] = false;
                    _nextOwner[x,y] = Owner.None;
                }
            }
        }

        // Применяем на визуал
        for (int x=0; x<W; x++)
        for (int y=0; y<H; y++)
        {
            bool aliveChanged = _nextAlive[x,y] != grid.AliveNow[x,y];
            bool ownerChanged = _nextOwner[x,y] != grid.Owners[x,y];

            if (aliveChanged || ( _nextAlive[x,y] && ownerChanged ))
            {
                grid.SetCell(x,y, _nextAlive[x,y], _nextOwner[x,y], instant:false);
            }
        }

        OnScored?.Invoke(wScore, bScore);
    }
    
    public void SetState(SimState newState)
    {
        // если выходим из Running — останавливаем корутину
        if (newState != SimState.Running && _loop != null)
        {
            StopCoroutine(_loop);
            _loop = null;
        }
        State = newState;
    }
}