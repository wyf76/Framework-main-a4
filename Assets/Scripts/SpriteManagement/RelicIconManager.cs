using UnityEngine;
using UnityEngine.UI;

public class RelicIconManager : MonoBehaviour
{
    [Tooltip("Assign sprites in the same order as relics.json")]
    public Sprite[] relicSprites;

    [Tooltip("Parent transform under which picked relic icons will appear")]
    public Transform relicIconParent;

    public Sprite GetIcon(int index)
    {
        if (index < 0 || index >= relicSprites.Length) return null;
        return relicSprites[index];
    }

    public void PlaceSprite(int index, Image target)
    {
        var spr = GetIcon(index);
        if (spr == null) { target.enabled = false; return; }
        target.enabled = true;
        target.sprite = spr;
    }


    public void Spawn(int index)
    {
        var go = new GameObject($"RelicIcon_{index}", typeof(Image));
        var img = go.GetComponent<Image>();
        img.sprite = GetIcon(index);
        go.transform.SetParent(relicIconParent, false);
    }

    public int GetCount()
    {
        return relicSprites?.Length ?? 0;
    }

    public Sprite Get(int index)
    {
        return GetIcon(index);
    }
}
