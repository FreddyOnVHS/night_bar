using UnityEngine;
using TMPro;
using NightAtTheBar;

public class DebugHUD : MonoBehaviour
{
    public TMP_Text statusText;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += RefreshHUD;
            RefreshHUD(GameManager.Instance.State);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= RefreshHUD;
    }

    private void RefreshHUD(GameState state)
    {
        if (state == null || GameManager.Instance.Campaign == null)
            return;

        int day = GameManager.Instance.Campaign.CurrentDayIndex + 1;

        int totalMinutes = state.GameMinute;

        int hour24 = (totalMinutes / 60) % 24;
        int minute = totalMinutes % 60;

        string amPm = hour24 >= 12 ? "PM" : "AM";

        int hour12 = hour24 % 12;
        if (hour12 == 0)
            hour12 = 12;

        statusText.text =
            $"Night: {day}\n" +
            $"Time: {hour12}:{minute:00} {amPm}\n" +
            $"Drunk: {Mathf.RoundToInt(state.Drunk)}\n" +
            $"Boredom: {Mathf.RoundToInt(state.Boredom)}";
    }
}