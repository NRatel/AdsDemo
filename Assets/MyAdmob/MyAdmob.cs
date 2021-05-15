using System;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAdsMediationTestSuite.Api;

//初始化&加载&播放策略：
//方案一：所有广告场景用一个adUnitId，游戏登录时初始化，初始化成功后立刻加载一个，然后每次播放关闭后重新加载。   缺点：有可能每次登录加载的用户永远不播放。
//方案二：所有广告场景用一个/多个adUnitId，游戏登录时初始化，用户点击播放时主动加载并等待，加载完成后播放。   缺点：有等待时间，用户可能中途放弃。

public class MyAdmob : MonoBehaviour
{
    public string adUnitId;
    private RewardedAd m_RewardedAd;

    private Action onAdLoaded;
    private Action<string> onAdFailedToLoad;
    private Action onNotReady;
    private Action onAdOpening;
    private Action<string> onAdFailedToShow;
    private Action<string, double> onUserEarnedReward;
    private Action onAdClosed;
    private Action<int, long, string> onPaidEvent;

    static public void ShowMediationTestSuite()
    {
        MediationTestSuite.Show();
    }

    static public void Initialize(Action onInited)
    {
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            Dictionary<string, AdapterStatus> map = initStatus.getAdapterStatusMap();
            foreach (KeyValuePair<string, AdapterStatus> keyValuePair in map)
            {
                string className = keyValuePair.Key;
                AdapterStatus status = keyValuePair.Value;
                switch (status.InitializationState)
                {
                    case AdapterState.NotReady:
                        Debug.LogWarning("Adapter: " + className + " not ready.");
                        break;
                    case AdapterState.Ready:
                        Debug.Log("Adapter: " + className + " is initialized.");
                        break;
                }
            }
            onInited?.Invoke();
        });
    }
    
    //支持多个实例 adUnitId
    static public MyAdmob CreateInstance(string adUnitId)
    {
        MyAdmob myAdmob = new GameObject("#MyAdmob#").AddComponent<MyAdmob>();
        myAdmob.adUnitId = adUnitId;
        DontDestroyOnLoad(myAdmob);
        return myAdmob;
    }

    public void CreateAndLoadRewardedAd(Action onAdLoaded = null, Action<string> onAdFailedToLoad = null)
    {
        //Create
        m_RewardedAd = new RewardedAd(adUnitId);
        m_RewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        m_RewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        m_RewardedAd.OnAdOpening += HandleRewardedAdOpening;
        m_RewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
        m_RewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        m_RewardedAd.OnAdClosed += HandleRewardedAdClosed;

        //Load
        this.onAdLoaded = onAdLoaded;
        this.onAdFailedToLoad = onAdFailedToLoad;
        AdRequest request = new AdRequest.Builder().Build();
        m_RewardedAd.LoadAd(request);
    }

    public void ShowAd(Action onNotReady = null, Action onAdOpening = null, Action<string> onAdFailedToShow = null, Action onAdClosed = null, Action<string, double> onUserEarnedReward = null, Action<int, long, string> onPaidEvent = null)
    {
        if (!m_RewardedAd.IsLoaded())
        {
            onNotReady?.Invoke();
            return;
        }
        
        this.onNotReady = onNotReady;
        this.onAdOpening = onAdOpening;
        this.onAdFailedToShow = onAdFailedToShow;
        this.onAdClosed = onAdClosed;
        this.onUserEarnedReward = onUserEarnedReward;
        this.onPaidEvent = onPaidEvent;

        m_RewardedAd.Show();
    }

    public void HandleRewardedAdLoaded(object sender, EventArgs args)
    {
        onAdLoaded?.Invoke();
    }

    public void HandleRewardedAdFailedToLoad(object sender, AdErrorEventArgs args)
    {
        onAdFailedToLoad?.Invoke(args.Message);
    }

    public void HandleRewardedAdOpening(object sender, EventArgs args)
    {
        onAdOpening?.Invoke();
    }

    public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        onAdFailedToShow?.Invoke(args.Message);
    }

    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        onAdClosed?.Invoke();
        //重新加载
        CreateAndLoadRewardedAd(this.onAdLoaded, this.onAdFailedToLoad);
    }

    public void HandleUserEarnedReward(object sender, Reward args)
    {
        onUserEarnedReward?.Invoke(args.Type, args.Amount);
    }

    public void HandlePaidEvent(object sender, AdValueEventArgs args)
    {
        onPaidEvent?.Invoke((int)args.AdValue.Precision, args.AdValue.Value, args.AdValue.CurrencyCode);
    }
}
