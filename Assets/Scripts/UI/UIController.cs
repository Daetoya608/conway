// UIController.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public GridManager grid;
    public Simulation sim;
    public MatchManager match;
    public CameraController2D camCtl;

    [Header("Core UI")]
    public Button btnPlay, btnPause, btnStep, btnRandom, btnClear, btnEndMatch;
    public Slider speedSlider;
    public TMP_InputField widthInput, heightInput;
    public Toggle wrapToggle;

    [Header("PvP")]
    public Toggle pvpToggle;
    public TMP_InputField seedsInput;
    public Button startPlacementBtn;

    [Header("HUD")]
    public TMP_Text whiteScoreTxt, blackScoreTxt, turnTxt;

    [Header("Results")]
    public GameObject resultsPanel;
    public TMP_Text resultsText;
    //public Button resultsCloseBtn;
    //public Button resultsRestartBtn;

    void UpdateModeText()
    {
        if (turnTxt == null) return;
        // Если в UI включен тоггл PvP (или матч уже активен), показываем PvP
        if (pvpToggle != null && pvpToggle.isOn)
        {
            // До начала расстановки (ещё не нажали Start Placement)
            if (!match.PvPEnabled && sim.State != SimState.Placement)
            {
                turnTxt.text = "PvP — подготовка (нажмите Start Duel)";
            }
            else
            {
                // Во время расстановки — показываем чей ход
                if (sim.State == SimState.Placement)
                {
                    turnTxt.text = (match.Turn == PlayerTurn.White)
                        ? "PvP — Ход: Зеленые"
                        : "PvP — Ход: Оранжевые";
                }
                else
                {
                    // После расстановки — идёт симуляция
                    turnTxt.text = "PvP — идёт симуляция";
                }
            }
        }
        else
        {
            // Обычный одиночный режим
            turnTxt.text = "Режим: Свободный";
        }
    }
    void Start()
    {
        turnTxt.gameObject.SetActive(true);
        UpdateModeText();
        btnPlay.onClick.AddListener(()=> { sim.StartSim(); UpdateModeText(); });
        btnPause.onClick.AddListener(()=> { sim.Pause();    UpdateModeText(); });
        btnStep.onClick.AddListener(()=> sim.StepOnce());
        btnRandom.onClick.AddListener(() =>
        {
            bool singleColor = !pvpToggle.isOn;   // в обычном режиме — только белые
            grid.Randomize(0.15f, singleColor);
        });
        btnClear.onClick.AddListener(()=> { grid.CreateGrid(); camCtl.FrameGrid(grid); ResetScores(); });

        speedSlider.onValueChanged.AddListener(v => sim.stepDelay = Mathf.Lerp(0.02f, 0.5f, 1f-v));

        wrapToggle.onValueChanged.AddListener(v => grid.wrap = v);
        
        match.OnTurnChanged += () => { UpdateHUDScores(); UpdateModeText(); };               // NEW
        match.OnPlacementFinished += ()=>
        {
            UpdateHUD(); UpdateModeText(); }; // на всякий случай

        btnEndMatch.onClick.AddListener(()=> match.EndMatchByButton());

        match.OnPlacementFinished += ()=> {
            sim.StartSim();
        };
        match.OnMatchEnded += (msg) =>
        {
            resultsPanel.SetActive(true);
            resultsText.text = msg;
            ResetScores();
            UpdateModeText(); // матч завершён, режим поменялся
        };


        sim.OnScored += (w,b)=> { UpdateHUDScores(); };

        // Размеры
        widthInput.onEndEdit.AddListener(_=> RebuildGrid());
        heightInput.onEndEdit.AddListener(_=> RebuildGrid());

        // init
        grid.CreateGrid();
        camCtl.FrameGrid(grid);
        ResetScores();
        resultsPanel.SetActive(false);
        
        pvpToggle.onValueChanged.AddListener(on =>
        {
            startPlacementBtn.interactable = on;
            if (!on)
            {
                match.DisablePvP();
                sim.SetState(SimState.Editing);
            }
            UpdateHUDScores();
            UpdateModeText();
        });

        // при старте сцены сразу выставим корректное состояние
        startPlacementBtn.interactable = pvpToggle.isOn;
        turnTxt.gameObject.SetActive(true);

        startPlacementBtn.onClick.AddListener(()=>{
            if (!pvpToggle.isOn) return;
            int seeds = int.TryParse(seedsInput.text, out var n) ? n : 20;
            match.NewPvPMatch(seeds);
            sim.SetState(SimState.Placement);
            UpdateModeText();
            UpdateHUD(); // сразу покажем "Ход: Белые"
        });
    }

    void ResetScores()
    {
        // Нули при новой сетке
        UpdateHUDScores(true);
    }

    public void HideResults()
    {
        resultsPanel.SetActive(false);
    }

    public void RestartAndHide()
    {
        // мягкий рестарт поля и статусов
        sim.Pause();
        grid.CreateGrid();
        camCtl.FrameGrid(grid);

        // если PvP был — обнулим состояние (без обязательного сброса очков, на твой вкус)
        match.DisablePvP();
        ResetScores();                // нули в HUD
        turnTxt.text = "Режим: Свободный";

        resultsPanel.SetActive(false);
    }
    
    void Update()
    {
        if (resultsPanel != null && resultsPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            HideResults();
    }
    
    void UpdateHUD()
    {
        UpdateHUDScores();
        UpdateModeText(); // ← режим рисуем централизованно
    }

    void UpdateHUDScores(bool reset = false)
    {
        if (reset)
        {
            whiteScoreTxt.text = "Зеленые: 0";
            blackScoreTxt.text = "Оранжевые: 0";
        }
        else
        {
            whiteScoreTxt.text = $"Зеленые: {match.WhiteScore}";
            blackScoreTxt.text = $"Оранжевые: {match.BlackScore}";
        }

        bool pvpActive = match != null && match.PvPEnabled;
        blackScoreTxt.gameObject.SetActive(pvpActive);
    }
    

    void RebuildGrid()
    {
        int w = int.TryParse(widthInput.text, out var _w) ? Mathf.Clamp(_w,10,250) : 50;
        int h = int.TryParse(heightInput.text, out var _h) ? Mathf.Clamp(_h,10,250) : 30;
        grid.width=w; grid.height=h;
        grid.CreateGrid();
        camCtl.FrameGrid(grid);
    }
}
