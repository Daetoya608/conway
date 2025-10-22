using UnityEngine;

[CreateAssetMenu(menuName = "Life/PatternAsset")]
public class PatternAsset : ScriptableObject
{
    public string patternName;   // название, например "Glider"
    public int width;
    public int height;

    [TextArea(5,20)]
    public string ascii; // узор в виде текста: '.' - мертвая, 'O' - живая
}