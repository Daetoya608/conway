// Cell.cs
using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Color deadColor = new Color(0.15f,0.15f,0.15f,1f);
    [SerializeField] private Color whiteColor = Color.white;
    [SerializeField] private Color blackColor = Color.black;

    public bool Alive { get; private set; }
    public Owner Owner { get; private set; } = Owner.None;
    public Vector2Int Index { get; private set; }

    // NEW: запоминаем базовый масштаб (заданный GridManager'ом)
    private Vector3 baseScale = Vector3.one;

    public void Init(Vector2Int index)
    {
        Index = index;
        baseScale = transform.localScale; // важн: после того, как GridManager выставил scale под cellSize
        SetVisual(false, Owner.None, instant:true);
    }

    public void SetVisual(bool alive, Owner owner, bool instant=false)
    {
        bool stateChanged = (Alive != alive) || (alive && Owner != owner);

        Alive = alive;
        Owner = alive ? owner : Owner.None;

        var targetColor = !alive ? deadColor : (Owner == Owner.White ? whiteColor : blackColor);
        
		// ----------------

		if (instant)
		{
    		sr.color = targetColor;
		}
		else
		{
    		// плавный переход цвета
    		LeanTween.value(gameObject, sr.color, targetColor, 0.25f)
        	.setOnUpdate((Color c) => sr.color = c)
        	.setEaseOutQuad();
		}


		// ----------------

        if (!instant && stateChanged)
        {
            if (alive)
            {
                transform.localScale = baseScale * 0.2f;
                LeanTween.scale(gameObject, baseScale, 0.12f).setEaseOutBack();
            }
            else
            {
                LeanTween.scale(gameObject, baseScale * 0.6f, 0.08f)
                         .setEaseInQuad()
                         .setOnComplete(()=> transform.localScale = baseScale);
            }
        }
        else
        {
            // гарантируем корректный размер, если нет анимации
            transform.localScale = baseScale;
        }
    }
}
