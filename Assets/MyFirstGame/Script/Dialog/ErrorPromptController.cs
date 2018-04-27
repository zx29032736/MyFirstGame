using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ErrorPromptController : MonoBehaviour {
    public Image banner;
    public Text title;
    public Text body;
    public Button close;

    public void RaiseErrorDialog(string txt)
    {
        this.body.text = txt;
        this.gameObject.SetActive(true);
    }

    public void CloseErrorDialog()
    {
        GameController.Instance.soundManager.PlaySound(Vector3.zero, GlobalStrings.BUTTON_LEAVE_SOUND_EFFECT);
        this.gameObject.SetActive(false);
    }

}
