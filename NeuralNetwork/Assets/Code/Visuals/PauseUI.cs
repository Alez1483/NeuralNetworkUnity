using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class PauseUI : MonoBehaviour
{
    [SerializeField] GameObject pauseUI;
    [SerializeField] GameObject[] pauseUIWindows;
    [SerializeField] GameObject stopLearningButton;
    int activeUI = 0;

    [SerializeField] Color rightColor, wrongColor, neutralColor;
    public static Color RightColor;
    public static Color WrongColor;
    public static Color NeutralColor;

    void Awake()
    {
        RightColor = rightColor;
        WrongColor = wrongColor;
        NeutralColor = neutralColor;

        pauseUI.SetActive(false);
    }

    public void StopLearning()
    {
        Trainer.Instance.enabled = false;
        pauseUIWindows[activeUI].SetActive(true);
        pauseUI.SetActive(true);
        stopLearningButton.SetActive(false);
    }

    public void ContinueLearning()
    {
        Trainer.Instance.enabled = true;
        pauseUI.SetActive(false);
        stopLearningButton.SetActive(true);
    }

    public void NextUI()
    {
        pauseUIWindows[activeUI].SetActive(false);
        activeUI = (activeUI + 1) % pauseUIWindows.Length;
        pauseUIWindows[activeUI].SetActive(true);
    }
    public void PreviousUI()
    {
        pauseUIWindows[activeUI].SetActive(false);
        activeUI = (activeUI + pauseUIWindows.Length - 1) % pauseUIWindows.Length;
        pauseUIWindows[activeUI].SetActive(true);
    }
}
