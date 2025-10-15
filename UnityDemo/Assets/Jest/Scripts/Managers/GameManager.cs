using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using com.unity.jest;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private async void Start()
    {
        await InitJestSDK();
    }

    private async Task InitJestSDK()
    {
        UIManager.Instance?.HidePanel();
        UIManager.Instance?.ShowLoadingSpinner();        

        try
        {
            await JestSDK.Instance.Init();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"InitJestSDK failed: {e}");
            UIManager.Instance?.HideLoadingSpinner();
            return;
        }
        Debug.Log("InitJestSDK Success");
        Dictionary<string, object> payload = JestSDK.Instance.GetEntryPayload();
        if (payload.TryGetValue("Name", out object nameObject))
        {
            JestSDK.Instance.player.Set("Name", nameObject.ToString());
        }

        if (payload.TryGetValue("Level", out object level))
        {
            JestSDK.Instance.player.Set("Level",   int.Parse(level.ToString()));
        }

        if (payload.TryGetValue("Coins", out object coins))
        {
            JestSDK.Instance.player.Set("Coins", int.Parse(coins.ToString()));
        }

        UIManager.Instance.EnableLoginButton(!JestSDK.Instance.player.isRegistered);

        SetupPlayerUI();

        UIManager.Instance?.HideLoadingSpinner();
        UIManager.Instance?.ShowPanel();

    }

    private void SetupPlayerUI()
    {
        JestSDK.Instance.player.TryGet("Coins", out int coins);
        JestSDK.Instance.player.TryGet("Level", out int level);
        JestSDK.Instance.player.TryGet("Name", out string name);
        name ??= "<Set Name>";

        UIManager.Instance.SetCoinsText(coins);
        UIManager.Instance.SetLevelText(level);
        UIManager.Instance.SetNameText(name);
    }

    public void ClaimFreeCoinsReward(int amount)
    {
        AddCoin(amount);
        JestSDK.Instance?.analytics?.CaptureEvent(
            "ClaimedFreeCoins",
            new Dictionary<string, object>
            {
                { "amount", amount },
                { "time", SharedUtils.GetUnixTime().ToString() }
            }
        );
    }

    internal void OnLoginAction()
    {
        JestSDK.Instance.player.TryGet("Coins", out int coins);
        JestSDK.Instance.player.TryGet("Level", out int level);
        JestSDK.Instance.player.TryGet("Name", out string name);
        var payload = new Dictionary<string, object>();
        if (coins != 0)
        {
            payload["Coins"] = coins;
        }
        if (level != 0)
        {
            payload["Level"] = level;
        }
        if (!string.IsNullOrEmpty(name))
        {
            payload["Name"] = name;
        }
        JestSDK.Instance.Login(payload);
    }


    public void ScheduleTestNotification()
    {
        string message = "Come back and play the game!!!";
        float delay = 60;
        bool sendPush = true;
        ScheduleNotification(message, delay, sendPush);
    }



    public void ScheduleNotification(string message, float delaySeconds, bool sendPush)
    {
        JestSDK.Instance?.notifications?.ScheduleNotification(new Notifications.Options
        {
            message = message,
            date = System.DateTime.Now.AddSeconds(delaySeconds),
            attemptPushNotification = sendPush,
            deduplicationKey = System.DateTime.Now.ToString()
        });
    }


    public void AddCoin(int amount = 1)
    {
        JestSDK.Instance.player.TryGet("Coins", out int coins);
        coins += amount;
        JestSDK.Instance.player.Set("Coins", coins);



        JestSDK.Instance?.analytics?.CaptureEvent("AddCoins", new Dictionary<string, object> {{ "amount", amount }});
        UIManager.Instance.SetCoinsText(coins);
    }

    public void IncrementLevel()
    {
        JestSDK.Instance.player.TryGet("Level", out int level);
        level += 1;
        JestSDK.Instance.player.Set("Level", level);

        JestSDK.Instance?.analytics?.CaptureEvent("LevelUp", new Dictionary<string, object> { { "level", level } });
        UIManager.Instance.SetLevelText(level);
    }

    public void SetName(string newName)
    {
        JestSDK.Instance.player.Set("Name", newName);
        JestSDK.Instance?.analytics?.CaptureEvent(
            "UpdateName",
            new Dictionary<string, object> { { "newName", newName } }
        );
    }
}
