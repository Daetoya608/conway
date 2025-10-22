// Assets/Scripts/Core/LeanTweenInit.cs
using UnityEngine;

public class LeanTweenInit : MonoBehaviour
{
    void Awake()
    {
        // Хватает с большим запасом для сотен тысяч твинов
        // Параметр: maxSimultaneousTweens
        LeanTween.init(100000);
    }
}