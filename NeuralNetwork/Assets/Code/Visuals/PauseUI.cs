using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class PauseUI : MonoBehaviour
{
    [SerializeField] GameObject pauseUI;
    [SerializeField] GameObject[] pauseUIWindows;
    [SerializeField] GameObject stopLearningButton;
    int activeUI = 0;

    void Awake()
    {
        pauseUI.SetActive(false);
    }

    public void StopLearning()
    {
        Trainer.Instance.enabled = false;
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
