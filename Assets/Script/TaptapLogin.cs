using System.Collections.Generic;
using UnityEngine;
using System;

using TapTap.Bootstrap;
using TapTap.Common;
using UnityEngine.SceneManagement;
using TapTap.AntiAddiction;
using TapTap.AntiAddiction.Model;
using TapTap.Billboard;
using TapTap.Connect;

public class TaptapLogin : MonoBehaviour
{

    void Start()
    {
        //初始化公告
        //设置地区、渠道、版本号三个维度
        var dimensionSet = new HashSet<KeyValuePair<string, string>>();
        KeyValuePair<string, string> platformPair = new KeyValuePair<string, string>("platform", "TapTap");
        KeyValuePair<string, string> locationPair = new KeyValuePair<string, string>("location", "CN");
        KeyValuePair<string, string> versionPair = new KeyValuePair<string, string>("version", "v1");

        dimensionSet.Add(platformPair);
        dimensionSet.Add(locationPair);
        dimensionSet.Add(versionPair);

        //公告模版类型，目前仅支持导航模版。可填写：导航模版-navigate、图片模版-image。
        var templateType = "navigate"; 
        var billboardServerUrl = "https://tdsgonggao.goodluckin.top";


        var config = new TapConfig.Builder()
            .ClientID("mlbfoduqiglbdugddp") // 必须，开发者中心对应 Client ID
            .ClientToken("3wROiubU8Dkv5c3h5K6bsawFYMjoSqBxXN0A55Hm") // 必须，开发者中心对应 Client Token
            .ServerURL("https://mlbfoduq.cloud.tds1.tapapis.cn") // 必须，开发者中心 > 你的游戏 > 游戏服务 > 基本信息 > 域名配置 > API
            .RegionType(RegionType.CN) // 非必须，CN 表示中国大陆，IO 表示其他国家或地区
            .TapBillboardConfig(dimensionSet, templateType, billboardServerUrl)
            .ConfigBuilder();
        TapBootstrap.Init(config);


        //初始化防沉迷
        AntiAddictionInit();

        //隐藏悬浮窗
        TapConnect.SetEntryVisible(false);

    }

    public void AntiAddictionInit()
    {
        AntiAddictionConfig config = new AntiAddictionConfig()
        {
            gameId = "mlbfoduqiglbdugddp",      // TapTap 开发者中心对应 Client ID
            showSwitchAccount = false,      // 是否显示切换账号按钮
        };
        Action<int, string> callback = (code, errorMsg) =>
        {
            if (code == 500)
            {
                // 登录成功
                Debug.Log("防沉迷成功");
                // 防沉迷验证成功
                // 进入菜单页面
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            else if (code == 1000)
            {
                // 用户登出
            }
            else if (code == 1001)
            {
                // 切换账号
                Debug.Log("切换账号");
            }
            else if (code == 1030)
            {
                // 用户当前无法进行游戏
                Debug.Log("用户当前无法进行游戏");

            }
            else if (code == 1050)
            {
                // 时长限制
                Debug.Log("当前用户玩游戏时长受限");

            }
            else if (code == 9002)
            {
                // 实名过程中点击了关闭实名窗
                //重新开始防沉迷实名步骤
                antiAddiction();
            }
            UnityEngine.Debug.LogFormat($"code: {code} error Message: {errorMsg}");
        };

        AntiAddictionUIKit.Init(config, callback);

        // 如果是 PC 平台还需要额外设置一下 gameId
        TapTap.AntiAddiction.TapTapAntiAddictionManager.AntiAddictionConfig.gameId = "mlbfoduqiglbdugddp";
    }

    public async void taptapLogin()
    {
        var currentUser = await TDSUser.GetCurrent();

        if (null == currentUser)
        {
            Debug.Log("当前未登录");
            // 开始登录
            try
            {
                // 在 iOS、Android 系统下会唤起 TapTap 客户端或以 WebView 方式进行登录
                // 在 Windows、macOS 系统下显示二维码（默认）和跳转链接（需配置）
                var tdsUser = await TDSUser.LoginWithTapTap();
                Debug.Log($"login success:{tdsUser}");
                // 获取 TDSUser 属性
                var objectId = tdsUser.ObjectId; // 用户唯一标识
                var nickname = tdsUser["nickname"]; // 昵称
                var avatar = tdsUser["avatar"]; // 头像


                Debug.Log("当前登录成功的用户是：");
                Debug.Log(nickname);

                // 登录成功以后开始防沉迷流程
                antiAddiction();
            }
            catch (Exception e)
            {
                if (e is TapException tapError) // using TapTap.Common
                {
                    Debug.Log($"encounter exception:{tapError.code} message:{tapError.message}");
                    // TapErrorCode.ERROR_CODE_BIND_CANCEL = 80002
                    if (tapError.code == 80002) // 取消登录
                    {
                        Debug.Log("登录取消");
                    }
                    Debug.Log("登录失败");
                }
            }
        }
        else
        {
            Debug.Log("已登录");
            // 开启防沉迷流程
            antiAddiction();
        }
    }
    public async void antiAddiction()
    {
        var currentUser = await TDSUser.GetCurrent();
        if (null != currentUser)
        {
            string userIdentifier = currentUser.ObjectId;
            AntiAddictionUIKit.Startup(userIdentifier, true);
            Debug.Log("触发防沉迷");
        }
        else
        {
            //未登录
            Debug.Log("未登录");
        }
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void OpenBillboard()
    {
        TapBillboard.OpenPanel((any, error) =>
        {
            if (error != null)
            {
                // 打开公告失败，可以根据 error.code 和 error.errorDescription 来判断错误原因
                Debug.Log($"打开开屏公告失败: {error.code}, {error.errorDescription}");
            }
            else
            {
                Debug.Log("打开公告成功");
            }
        }, () =>
        {
            Debug.Log("公告已关闭");
        });


        //监听公告中的跳转
        TapBillboard.RegisterCustomLinkListener(url =>
        {
            // 这里返回的 url 地址和游戏在公告系统内配置的地址是一致的

        });
        //刷新小红点
        TapBillboard.QueryBadgeDetails((badgeDetails, error) =>
        {
            if (error != null)
            {
                // 获取小红点信息失败，可以根据 error.code 和 error.errorDescription 来判断错误原因
                Debug.Log($"打开开屏公告失败: {error.code}, {error.errorDescription}");
            }
            else
            {
                // 获取小红点信息成功
                if (badgeDetails.showRedDot == 1)
                {
                    // 有新的公告信息
                    Debug.Log("有新的公告信息");
                }
                else
                {
                    // 没有新的公告信息
                    Debug.Log("刷新小红点，没有新的公告信息");
                }
            }
        });

    }

}
