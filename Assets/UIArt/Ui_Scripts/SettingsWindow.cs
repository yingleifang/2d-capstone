using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsWindow : MonoBehaviour
{
    public GameObject settingsPanel;
    // Start is called before the first frame update
    void Start()
    {
        //transform.localScale = Vector2.zero;
    }

    public void SettingsOpen()
    {
        settingsPanel.SetActive(true);
        LeanTween.scale(settingsPanel, new Vector3(1, 1, 1), 0.3f);
    }

    public void SettingsClose()
    {
        LeanTween.scale(settingsPanel, new Vector3(0, 0, 0), 0.3f).setOnComplete(() => settingsPanel.SetActive(false));
    }


}
