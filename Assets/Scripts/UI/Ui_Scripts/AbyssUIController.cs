using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbyssUIController : MonoBehaviour
{
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject startButton;
    public GameObject settingsButton;
    public GameObject quitButton;
    public GameObject creditsButton;

    // Start is called before the first frame update
    void Start()
    {
        //transform.localScale = Vector2.zero;
        LeanTween.scale(settingsPanel, new Vector3(0, 0, 0), 0.3f);
        settingsPanel.SetActive(false);

        LeanTween.scale(creditsPanel, new Vector3(0, 0, 0), 0.3f);
        creditsPanel.SetActive(false);

        startButton.transform.localScale = new Vector3(0, 0, 0);
        settingsButton.transform.localScale = new Vector3(0, 0, 0);
        quitButton.transform.localScale = new Vector3(0, 0, 0);
        creditsButton.transform.localScale = new Vector3(0, 0, 0);

        LeanTween.scale(startButton, new Vector3(1, 1, 1), 0.3f).setDelay(0.2f);
        LeanTween.scale(settingsButton, new Vector3(1, 1, 1), 0.3f).setDelay(0.4f);
        LeanTween.scale(quitButton, new Vector3(1, 1, 1), 0.3f).setDelay(0.6f);
        LeanTween.scale(creditsButton, new Vector3(1, 1, 1), 0.3f).setDelay(0.6f);


    }

    public void ButtonSelect(GameObject button)
    {
        LeanTween.scale(button, new Vector3(1.2f, 1.2f, 1.2f), 0.1f);
    }

    public void ButtonDeselect(GameObject button)
    {
        LeanTween.scale(button, new Vector3(1, 1, 1), 0.1f);
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

    public void CreditsOpen()
    {
        creditsPanel.SetActive(true);
        LeanTween.scale(creditsPanel, new Vector3(1, 1, 1), 0.3f);
    }

    public void CreditsClose()
    {
        LeanTween.scale(creditsPanel, new Vector3(0, 0, 0), 0.3f).setOnComplete(() => creditsPanel.SetActive(false));
    }
}
