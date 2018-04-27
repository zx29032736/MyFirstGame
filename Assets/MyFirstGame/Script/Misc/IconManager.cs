using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconManager : MonoBehaviour {

    public List<Icon> iconRepository;
    public List<Icon> characterRepository;
    public Sprite defaultIcon; // use this if we get a request for a non-existent icon;

    public enum IconTypes { Spell, Class, Item, Status, Encounter, Level, Misc, Characater }

    public Sprite GetIconById(string id)
    {
        Icon icon = iconRepository.Find((i) => { return i.id == id; });
        if (icon != null)
        {
            return icon.sprite;
        }
        else
        {
            return defaultIcon;
        }
    }

    public Sprite GetFullCharacterById(string id)
    {
        Icon icon = characterRepository.Find((i) => { return i.id == id; });
        if (icon != null)
        {
            return icon.sprite;
        }
        else
        {
            return defaultIcon;
        }
    }
}

[System.Serializable]
public class Icon
{
    public string id;
    public Sprite sprite;
    public IconManager.IconTypes type;
}

