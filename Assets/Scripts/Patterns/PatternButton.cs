using UnityEngine;
using UnityEngine.UI;

public class PatternButton : MonoBehaviour
{
    public PatternAsset pattern;
    public PatternLibrary lib;
    Button _btn;

    void Awake()
    {
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(()=> {
            if (lib != null && pattern != null)
                lib.ArmPattern(pattern); // активируем режим постановки
        });
    }
}