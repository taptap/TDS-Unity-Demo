using UnityEngine;
using UnityEngine.SceneManagement;
using TapSDK.Login;
using UnityEngine.UI;
using TapSDK.Compliance;

/// <summary>
/// 游戏商店及设置场景
/// </summary>
public class GameStoreAndSettingsScene : MonoBehaviour
{
    // 设置菜单 UI
    public GameObject SettingsMenu;

    // 商城 UI
    public GameObject StoreMenu;

    // 音频资源
    public AudioSource audioBgm;

    // 支付提示弹窗
    public GameObject ChargeTip;

    // 上报付费重试次数
    private int TryTimes = 0;

    // 最大重试次数
    private const int MaxTryTimes = 3;


    /// <summary>
    /// 设置菜单中显示用户昵称
    /// </summary>
    public async void Start()
    {
        Text username = SettingsMenu.transform.Find("username").GetComponent<Text>();
        var account = await TapTapLogin.Instance.GetCurrentTapAccount();
        username.text = account.name;
    }

    /// <summary>
    /// 显示设置菜单
    /// </summary>
    public void GoSettings()
    {
        // 暂停游戏
        Time.timeScale = 0f;
        audioBgm.Pause();

        SettingsMenu.SetActive(true);

    }

    /// <summary>
    /// 从设置页返回游戏
    /// </summary>
    public void ResumeGame()
    {
        // 返回游戏
        SettingsMenu.SetActive(false);
        Time.timeScale = 1f;
        audioBgm.Play();

    }

    /// <summary>
    /// 返回登录页
    /// </summary>
    public void GoBackToLogin()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    /// <summary>
    /// 支付取消
    /// </summary>
    public void OnChargeCancel()
    {
        // 关闭商店 UI 
        StoreMenu.SetActive(false);

        // 返回游戏
        audioBgm.Play();

    }

    /// <summary>
    /// 充值 10 元
    /// </summary>
    public void OnCharge10()
    {
        Charge(10 * 100);
    }

    /// <summary>
    /// 充值 50 元
    /// </summary>
    public void OnCharge50()
    {
        Charge(50 * 100);
    }

    /// <summary>
    /// 开始充值流程，先通过防沉迷模块检查充值是否受限，当无限制时，继续后续充值流程，否则提示充值限制
    /// </summary>
    /// <param name="amount"> 充值金额，单位：分 </param>
    private void Charge(int amount)
    {
        TapTapCompliance.CheckPaymentLimit(amount, (result) =>
        {
            int status = result.status;
            // 当前充值不受限
            if (status == 1)
            {
                // TODO: 完成后续充值流程
                // 示例中默认为充值成功,显示充值结果
                ShowPayResult(true, null);

                //上报充值金额
                SubmitPayResult(amount);
            }
            else // 充值受限
            {
                Debug.Log("当前充值受限");
            }
        }, (exception) =>
        {
            ShowPayResult(false, "当前网络异常，请稍后重试");
        });
    }

    /// <summary>
    /// 显示充值结果
    /// </summary>
    /// <param name="success">充值是否受限</param>
    /// <param name="error"> 错误描述</param>
    private void ShowPayResult(bool success, string error)
    {
        // 关闭商店
        StoreMenu.SetActive(false);

        Text chargeResult = ChargeTip.transform.Find("chargeResult").GetComponent<Text>();
        if (success)
        {
            chargeResult.text = "充值成功！";
        }
        else
        {
            chargeResult.text = error;
        }
        ChargeTip.SetActive(true);

    }

    /// <summary>
    /// 充值流程完成，返回游戏
    /// </summary>
    public void OnChargeFinished()
    {
        ChargeTip.SetActive(false);

        audioBgm.Play();
    }

    /// <summary>
    /// 进入商店，暂停游戏
    /// </summary>
    public void GoStore()
    {
        audioBgm.Pause();

        StoreMenu.SetActive(true);

    }

    /// <summary>
    /// 上报充值
    /// </summary>
    /// <param name="amount"></param>
    private void SubmitPayResult(int amount)
    {
        TapTapCompliance.SubmitPayment(amount, () =>
            {
                // 上报成功
                TryTimes = 0;
            }, (exception) =>
            {
                // 进行重试
                if (TryTimes < MaxTryTimes)
                {
                    TryTimes++;
                    SubmitPayResult(amount);
                }
            });
    }

}