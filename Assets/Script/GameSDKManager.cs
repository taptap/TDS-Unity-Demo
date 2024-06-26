using System;
using UnityEngine.SceneManagement;
using TapTap.Login;
using TapTap.AntiAddiction;
using TapTap.AntiAddiction.Model;
using System.Collections.Generic;

/// <summary>
/// SDK 初始化及合规认证回调处理管理类
/// </summary>
public sealed class GameSDKManager
{
    // 游戏在 TapTap 开发者中心对应的 Client ID
    private readonly string clientId = "游戏的 Client ID";

    // 异常事件类型-适龄限制
    public const int EVENT_TYPE_AGE_RESTRICT = 1;

    // 异常事件类型-网络异常或应用信息配置错误
    public const int EVENT_TYPE_NETWORK_ERROR = 2;

    // 是否已初始化
    private readonly bool hasInit = false;

    // 是否已通过合规认证检查
    public bool hasCheckedAntiAddiction { get; private set; }

    // 合规认证部分限制事件监听，用于显示对应提示 UI
    private readonly List<Action<int>> restrictActionList;

    private static readonly Lazy<GameSDKManager> lazy
        = new Lazy<GameSDKManager>(() => new GameSDKManager());
    public static GameSDKManager Instance { get { return lazy.Value; } }

    private GameSDKManager()
    {
        restrictActionList = new List<Action<int>>();
    }

    // 声明合规认证回调
    private readonly Action<int, string> AntiAddictionCallback = (code, errorMsg) =>
    {
        // 根据回调返回的参数 code 添加不同情况的处理
        switch (code)
        {
            case 500: // 玩家未受限制，可正常开始
                Instance.hasCheckedAntiAddiction = true;
                UnityEngine.Debug.Log("开始游戏");
                break;

            case 1000: // 防沉迷认证凭证无效时触发
            case 1001: // 当玩家触发时长限制时，点击了拦截窗口中「切换账号」按钮
            case 9002: // 实名认证过程中玩家关闭了实名窗口
                TapLogin.Logout();
                SceneManager.LoadScene("Login");
                break;

            case 1100: // 当前用户因触发应用设置的年龄限制无法进入游戏
                foreach (Action<int> action in Instance.restrictActionList)
                {
                    UnityEngine.Debug.Log("show anit ui");
                    action.Invoke(EVENT_TYPE_AGE_RESTRICT);
                }
                break;

            case 1200: // 数据请求失败，应用信息错误或网络连接异常  
                foreach (Action<int> action in Instance.restrictActionList)
                {
                    action.Invoke(EVENT_TYPE_NETWORK_ERROR);
                }
                break;

            default:
                UnityEngine.Debug.Log("其他可选回调");
                break;
        }

    };

    /// <summary>
    /// 初始化登录与合规认证 SDK 
    /// </summary>
    public void InitSDK()
    {
        if (!hasInit)
        {
            // 初始化 TapTap 登录
            TapLogin.Init(clientId);
            //初始化防沉迷
            AntiAddictionConfig config = new AntiAddictionConfig()
            {
                gameId = clientId,      // TapTap 开发者中心对应 Client ID
                showSwitchAccount = true,           // 是否显示切换账号按钮
                useAgeRange = false                  // 是否使用年龄段信息
            };

            // 初始化合规认证及设置回调
            AntiAddictionUIKit.Init(config);
            AntiAddictionUIKit.SetAntiAddictionCallback(AntiAddictionCallback);
        }
    }

    /// <summary>
    /// 开始合规认证检查
    /// </summary>
    /// <param name="userIdentifier">用户唯一标识</param>
    public void StartAntiAddiction(string userIdentifier)
    {
        hasCheckedAntiAddiction = false;
        AntiAddictionUIKit.StartupWithTapTap(userIdentifier);
    }

    /// <summary>
    /// 注册合规认证异常回调监听
    /// </summary>
    /// <param name="action"> 监听实例</param>
    public void RegisterListener(Action<int> action)
    {
        UnityEngine.Debug.Log("register anit ui");
        restrictActionList.Add(action);
    }

    /// <summary>
    /// 移除合规认证异常回调监听
    /// </summary>
    /// <param name="action"> 监听实例</param>
    public void UnRegisterListener(Action<int> action)
    {
        UnityEngine.Debug.Log("移除监听");
        restrictActionList.Remove(action);
    }
}
