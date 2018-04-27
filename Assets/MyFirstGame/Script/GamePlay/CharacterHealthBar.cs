using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterHealthBar : MonoBehaviour {

    public Image healthImg;
    public Unit Myunit;
    public bool isUpdatingPos = true;
    RectTransform myCanvasRT;
    RectTransform myRT;

    public void Init(Unit unit, RectTransform canvasRT , bool UpdatePos = true)
    {
        this.Myunit = unit;
        this.myCanvasRT = canvasRT;
        this.isUpdatingPos = UpdatePos;
        gameObject.SetActive(true);
        myRT = GetComponent<RectTransform>();
    }

	void Update ()
    {
        if (Myunit.savedCharacter == null)
        {
            Debug.Log("1");
            return;
        }
            
        if (Myunit.savedCharacter.PlayerVitals.MaxHealth == 0)
        {
            Debug.Log("2");
            return;
        }
        float f1 = ((float)Myunit.savedCharacter.PlayerVitals.Health) / ((float)Myunit.savedCharacter.PlayerVitals.MaxHealth);

        healthImg.fillAmount = f1;

        if (isUpdatingPos)
        {
            Vector3 myHealthPos = new Vector3(Myunit.transform.position.x, Myunit.transform.position.y - 0.5f, Myunit.transform.position.z);
            myRT.anchoredPosition = UIManager.WorldToUI(myCanvasRT, myHealthPos);
        }

    }
}
