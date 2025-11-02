using UnityEngine;

namespace KMines
{
    public class GameTimer : MonoBehaviour
    {
        public WinLoseManager rules;
        public TimerUI timerUI;

        bool active;
        float timeLeft;

        public void StartLevelTimer(bool timed, float seconds)
        {
            if (!timed || seconds <= 0f)
            {
                active = false;
                timeLeft = 0f;

                if (timerUI != null)
                    timerUI.HideTimer();

                return;
            }

            StartTimer(seconds);
        }

        public void StartTimer(float seconds)
        {
            active = true;
            timeLeft = Mathf.Max(0f, seconds);

            if (timerUI != null)
                timerUI.ShowTimer(timeLeft);
        }

        void Update()
        {
            if (!active) return;

            timeLeft -= Time.deltaTime;
            if (timeLeft < 0f) timeLeft = 0f;

            if (timerUI != null)
                timerUI.UpdateTimer(timeLeft);

            if (timeLeft <= 0f)
            {
                active = false;

                if (rules != null)
                    rules.OnTimeExpired();
            }
        }

        public void AddTime(float seconds)
        {
            if (!active) return;
            if (seconds <= 0f) return;

            timeLeft += seconds;

            if (timerUI != null)
                timerUI.UpdateTimer(timeLeft);
        }

        public bool IsActive()
        {
            return active;
        }

        public float GetTimeLeft()
        {
            return timeLeft;
        }

        // NY: så Board kan stoppa timern vid death
        public void StopTimer()
        {
            active = false;
            if (timerUI != null)
                timerUI.UpdateTimer(timeLeft);
        }

        public void ClearTimerUIRef()
        {
            timerUI = null;
        }
    }
}
