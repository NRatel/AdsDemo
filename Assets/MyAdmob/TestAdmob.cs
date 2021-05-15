using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;

public class TestAdmob : MonoBehaviour
{

#if UNITY_ANDROID
    private const string kAdUnitId = "ca-app-pub-3940256099942544/5224354917";  //官方专用测试广告单元ID
    private const string kTestBtnName = "安卓广告";
#endif

#if UNITY_IOS
    private const string kAdUnitId = "ca-app-pub-3940256099942544/1712485313";  //官方专用测试广告单元ID
    private const string kTestBtnName = "IOS广告";
#endif

    private MyAdmob myAdmob;

    private void Start()
    {
        MyAdmob.Initialize(() =>
        {
            myAdmob = MyAdmob.CreateInstance(kAdUnitId);
            myAdmob.CreateAndLoadRewardedAd(() =>
            {
                Debug.Log(string.Format("加载成功:{0}", myAdmob.adUnitId));
            }, (errorMsg) =>
            {
                Debug.Log(string.Format("加载失败:{0},{1}", myAdmob.adUnitId, errorMsg));
            });
        });
    }

    public void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 50), "中介测试套件"))
        {
            MyAdmob.ShowMediationTestSuite();
        }

        if (GUI.Button(new Rect(130, 10, 100, 50), kTestBtnName))
        {
            myAdmob.ShowAd(() =>
            {
                Debug.Log(string.Format("未准备好：{0}", myAdmob.adUnitId));
            }, () =>
            {
                Debug.Log(string.Format("展示成功：{0}", myAdmob.adUnitId));
            }, (errorMsg) =>
            {
                Debug.Log(string.Format("展示失败：{0},{1}", myAdmob.adUnitId, errorMsg));
            }, () =>
            {
                Debug.Log(string.Format("视频关闭：{0}", myAdmob.adUnitId));
            }, (type, amount) =>
            {
                Debug.Log(string.Format("获得奖励：{0},{1},{2}", myAdmob.adUnitId, type, amount));
            }, (precision, value, currencyCode) =>
            {
                Debug.Log(string.Format("获得收益：{0},{1},{2},{3}", myAdmob.adUnitId, precision, value, currencyCode));
            });
        }
    }
}
