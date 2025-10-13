using UnityEngine;
using System;
using System.Collections;

public class FreeCoinsManager : MonoBehaviour
{
    public static FreeCoinsManager Instance { get; private set; }

    [Header("Configurable Settings")]
    public float rewardIntervalMinutes = 10f;
    public int coinRewardAmount = 50;

    private const string LastClaimTimeKey = "FreeCoins_LastClaimTime";
    private bool isRewardAvailable = false;
    private double lastClaimUnixTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        LoadLastClaimTime();
        CheckRewardOnStart();
        StartCoroutine(CheckRewardAvailability());
    }

    public void ClaimReward()
    {
        if (!isRewardAvailable)
        {
            Debug.Log("Reward not available yet!");
            return;
        }

        GiveCoins(coinRewardAmount);
        lastClaimUnixTime = SharedUtils.GetUnixTime();
        SaveLastClaimTime();
        isRewardAvailable = false;
        ScheduleNotificationForNextReward();


    }

    private void ScheduleNotificationForNextReward()
    {
        string message = "Free Coins Reward Is Ready!!!";
        float remainingTime = GetTimeRemaining();
        bool trySendPush = true;
        GameManager.Instance.ScheduleNotification(message, remainingTime, trySendPush);
    }

    public bool IsRewardAvailable()
    {
        return isRewardAvailable;
    }

    private void GiveCoins(int amount)
    {
        Debug.Log($"Player received {amount} coins!");
        GameManager.Instance.ClaimFreeCoinsReward(amount);
    }

    private void CheckRewardOnStart()
    {
        if (lastClaimUnixTime <= 0) // First time playing
        {
            isRewardAvailable = true;
        }
        else
        {
            double elapsed = SharedUtils.GetUnixTime() - lastClaimUnixTime;
            double waitTime = rewardIntervalMinutes * 60f;

            if (elapsed >= waitTime)
            {
                isRewardAvailable = true;
            }
        }
    }

    private IEnumerator CheckRewardAvailability()
    {
        while (true)
        {
            if (!isRewardAvailable)
            {
                double elapsed = SharedUtils.GetUnixTime() - lastClaimUnixTime;
                if (elapsed >= rewardIntervalMinutes * 60f)
                {
                    isRewardAvailable = true;
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public float GetTimeRemaining()
    {
        if (isRewardAvailable) return 0f;

        double elapsed = SharedUtils.GetUnixTime() - lastClaimUnixTime;
        double totalWait = rewardIntervalMinutes * 60f;

        return Mathf.Max(0f, (float)(totalWait - elapsed));
    }

    public string GetTimeRemainingString()
    {
        return SharedUtils.FormatTime((int)Math.Ceiling(GetTimeRemaining()));
    }

    private void SaveLastClaimTime()
    {
        PlayerPrefs.SetString(LastClaimTimeKey, lastClaimUnixTime.ToString());
        PlayerPrefs.Save();
    }

    private void LoadLastClaimTime()
    {
        if (PlayerPrefs.HasKey(LastClaimTimeKey))
        {
            double.TryParse(PlayerPrefs.GetString(LastClaimTimeKey), out lastClaimUnixTime);
        }
        else
        {
            lastClaimUnixTime = 0;
        }
    }


}
