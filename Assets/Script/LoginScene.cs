using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using TapTap.AntiAddiction;
using TapTap.Login;
using UnityEngine.UI;

/// <summary>
/// 登录场景
/// </summary>
public class LoginScene : MonoBehaviour
{

    // 登录按钮
    public GameObject LoginButton;

    // 年龄限制提示弹窗
    public GameObject AgeRangeRestrictTip;

    // 网络异常或应用信息错误提示弹窗
    public GameObject NetworkOrClientError;

    // 用户昵称 Text 控件
    public Text tv_userTip;

    // 切换账号按钮
    public GameObject SwitchAccountButton;

    // 用户昵称游戏对象
    public GameObject UserTip;

    // 开始游戏按钮
    public GameObject EnterGameButton;


    /// <summary>
    /// 初始化 SDK 并判断本地是否已登录，已登录时开始合规认证检查，否则显示登录按钮
    /// </summary>
    async void Start()
    {
        // 初始化 SDK
        GameSDKManager.Instance.InitSDK();

        // 注册合规认证限制事件回调，显示对应提示弹窗
        GameSDKManager.Instance.RegisterListener(HandleAntiAddictionError);

        AccessToken currentToken = null;
        try
        {
            // 检查本地是否已存在 TapToken
            currentToken = await TapLogin.GetAccessToken();
        }
        catch (Exception e)
        {
            Debug.Log("本地无有效 token");
        }

        // 根据本地是否存在用户信息显示不同 UI 
        SwitchLoginState(currentToken != null);
        
        // 本地存在用户信息且未通过合规认证时进行合规认证检查
        if (currentToken != null && !GameSDKManager.Instance.HasCheckedAntiAddiction)
        {
            StartAntiAddiction();
        }

    }

    /// <summary>
    /// 登录按钮点击后执行 Tap 登录
    /// </summary>
    public async void OnTapLoginButtonClick()
    {
        try
        {
            // 发起 Tap 登录并获取用户信息
            var accessToken = await TapLogin.Login();

            // 切换 UI 显示状态
            SwitchLoginState(true);

            // 开始合规认证检查
            StartAntiAddiction();
        }
        catch (Exception e)
        {
            Debug.Log("用户登录取消或错误");
        }
    }

    /// <summary>
    /// 根据登录状态切换 UI 
    /// </summary>
    /// <param name="logged">是否已登录</param>
    private async void SwitchLoginState(bool logged = true)
    {
        if (logged)
        {
            UserTip.SetActive(true);
            SwitchAccountButton.SetActive(true);
            EnterGameButton.SetActive(true);
            LoginButton.SetActive(false);

            var profile = await TapLogin.GetProfile();
            tv_userTip.text = profile.name + " 欢迎回来 ！";
        }
        else
        {
            UserTip.SetActive(false);
            SwitchAccountButton.SetActive(false);
            EnterGameButton.SetActive(false);
            LoginButton.SetActive(true);
        }
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    public void OnEnterGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    /// <summary>
    /// 切换账号
    /// </summary>
    public void OnSwitchAccount()
    {
        TapLogin.Logout();
        AntiAddictionUIKit.Exit();
        SwitchLoginState(false);
    }

    /// <summary>
    /// 因触发限制切换账号
    /// </summary>
    public void OnSwitchAccountForRestrict()
    {
        OnSwitchAccount();
        AgeRangeRestrictTip.SetActive(false);
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void OnExit()
    {
        Application.Quit();
    }


    /// <summary>
    /// 重新出发防沉迷
    /// </summary>
    public void OnRetryAntiaddiction()
    {
        StartAntiAddiction();
    }

    /// <summary>
    /// 开始合规认证检查
    /// </summary>
    public async void StartAntiAddiction()
    {
        var profile = await TapLogin.GetProfile();

        // 使用当前 Tap 用户的 unionid 作为用户标识进行合规认证检查
        string userIdentifier = profile.unionid;
        GameSDKManager.Instance.StartAntiAddiction(userIdentifier);
    }

    /// <summary>
    /// 处理合规认证限制异常回调，显示对应异常 UI
    /// </summary>
    /// <param name="type">异常类型</param>
    public void HandleAntiAddictionError(int type)
    {
        switch (type)
        {
            case GameSDKManager.EVENT_TYPE_AGE_RESTRICT:
                AgeRangeRestrictTip.SetActive(true);
                break;
            case GameSDKManager.EVENT_TYPE_NETWORK_ERROR:
                NetworkOrClientError.SetActive(true);
                break;
            default:
                break;
        }
    }

    void OnDestroy(){
        // 移除对应监听
        GameSDKManager.Instance.UnRegisterListener(HandleAntiAddictionError);
    }

}
