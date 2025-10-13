using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using JestSDK;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private JestSDK.JestSDK m_jestSDK;

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

        m_jestSDK = new JestSDK.JestSDK();

        try
        {
            await m_jestSDK.Init();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"InitJestSDK failed: {e}");
            UIManager.Instance?.HideLoadingSpinner();
            return;
        }
        Debug.Log("InitJestSDK Success");
        Dictionary<string, object> payload = m_jestSDK.GetEntryPayload();
        if (payload.TryGetValue("Name", out object nameObject))
        {
            m_jestSDK.player.Set("Name", nameObject.ToString());
        }

        if (payload.TryGetValue("Level", out object level))
        {
            m_jestSDK.player.Set("Level",   int.Parse(level.ToString()));
        }

        if (payload.TryGetValue("Coins", out object coins))
        {
            m_jestSDK.player.Set("Coins", int.Parse(coins.ToString()));
        }

        UIManager.Instance.EnableLoginButton(!m_jestSDK.player.isRegistered);

        SetupPlayerUI();

        UIManager.Instance?.HideLoadingSpinner();
        UIManager.Instance?.ShowPanel();

    }

    private void SetupPlayerUI()
    {
        m_jestSDK.player.TryGet("Coins", out int coins);
        m_jestSDK.player.TryGet("Level", out int level);
        m_jestSDK.player.TryGet("Name", out string name);
        name ??= "<Set Name>";

        UIManager.Instance.SetCoinsText(coins);
        UIManager.Instance.SetLevelText(level);
        UIManager.Instance.SetNameText(name);
    }

    public void ClaimFreeCoinsReward(int amount)
    {
        AddCoin(amount);
        m_jestSDK?.analytics?.CaptureEvent(
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
        m_jestSDK.player.TryGet("Coins", out int coins);
        m_jestSDK.player.TryGet("Level", out int level);
        m_jestSDK.player.TryGet("Name", out string name);
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
        m_jestSDK.Login(payload);
    }

    public void ScheduleNotification(string message, float delaySeconds, bool sendPush)
    {
        m_jestSDK?.notifications?.ScheduleNotification(new Notifications.Options
        {
            message = message,
            date = System.DateTime.Now.AddSeconds(delaySeconds),
            attemptPushNotification = sendPush
        });
    }


    public void AddCoin(int amount = 1)
    {
        m_jestSDK.player.TryGet("Coins", out int coins);
        coins += amount;
        m_jestSDK.player.Set("Coins", coins);



        m_jestSDK?.analytics?.CaptureEvent("AddCoins", new Dictionary<string, object> {{ "amount", amount }});
        UIManager.Instance.SetCoinsText(coins);
    }

    public void IncrementLevel()
    {
        m_jestSDK.player.TryGet("Level", out int level);
        level += 1;
        m_jestSDK.player.Set("Level", level);

        m_jestSDK?.analytics?.CaptureEvent("LevelUp", new Dictionary<string, object> { { "level", level } });
        UIManager.Instance.SetLevelText(level);
    }

    public void SetName(string newName)
    {
        m_jestSDK.player.Set("Name", newName);
        m_jestSDK?.analytics?.CaptureEvent(
            "UpdateName",
            new Dictionary<string, object> { { "newName", newName } }
        );
    }
}
