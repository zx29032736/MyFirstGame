using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingPromptController : MonoBehaviour {

    public void RaiseLoadingPrompt()
    {
        this.gameObject.SetActive(true);
    }

    public void CloseLoadingPrompt()
    {
        this.gameObject.SetActive(false);
    }
}
