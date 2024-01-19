using System.Collections;using System.Collections.Generic;using UnityEngine;using UnityEngine.SceneManagement;using TapTap.Bootstrap;using TapTap.Common;using UnityEngine.UI;using UnityEngine.Networking;using TapTap.AntiAddiction;using TapTap.AntiAddiction.Model;using TapTap.Moment;using TapTap.Achievement;using TapTap.Connect;
using TapTap.Payment;

public class Menu : MonoBehaviour, IAchievementCallback{
    public Text nickname;
    public Text age;
    public Image avatar;
    //获取玩家年龄段
    public void Start()

    {
        SetName();
        SetAge();

        //设置内嵌动态
        SetEmbeddedMoments();

        //注册成就
        InitTapAchievement();

        //显示悬浮窗
        TapConnect.SetEntryVisible(true);
    }

    //开始游戏
    public void PlayGame()
    {
        //上报游戏时长
        AntiAddictionUIKit.EnterGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    //退出登录
    public async void Logout()
    {
        var currentUser = await TDSUser.GetCurrent();
        if (null == currentUser)
        {
            Debug.Log("未登录");
        }
        else
        {
            await TDSUser.Logout();

            //退出登录时，退出防沉迷
            AntiAddictionUIKit.Exit();

            //退出登录后返回登录页面：
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public async void SetName()
    {
        var currentUser = await TDSUser.GetCurrent();

        if (null != currentUser)
        {
            string nicknameStr = currentUser["nickname"].ToString();
            nickname.text = nicknameStr;// 昵称
            string avatarPath = currentUser["avatar"].ToString(); // 头像
            Debug.Log("头像的 URL 是：" + avatarPath);
            //加载头像
            StartCoroutine(GetTexFromUnityWebRequest(avatarPath));

            //修改内建账户的用户名为 TapTap 的昵称，不然 username 会显示为 ObjectId，这里要做修改是用于在排行榜中显示名字（username），如果这里不做修改，排行榜可以直接用 nickname 这个字段作为姓名。
            currentUser["username"] = nicknameStr;
            await currentUser.Save();
        }
    }
    public void SetAge()
    {
        //展示三种年龄：未认证、未成年、18+
        int ageRange = AntiAddictionUIKit.AgeRange;
        if (ageRange == -1)
        {
            //未知
            age.text = "未知年龄";
        }
        else if (ageRange == 18)
        {
            //成年
            age.text = "18+";
        }
        else
        {
            // ageRange = 0,8,16 未成年
            age.text = "未成年";
        }
    }
    public void SetEmbeddedMoments()
    {
        TapMoment.SetCallback((code, msg) =>
              {
                  Debug.Log(code + "---" + msg);
                  if (code == 10000)
                  {
                      Debug.Log("动态发布成功");

                  }
                  else if (code == 10100)
                  {
                      Debug.Log("动态发布失败");
                  }
                  else if (code == 10200)
                  {
                      Debug.Log("关闭动态发布页面");
                  }
                  else if (code == 20000)
                  {
                      Debug.Log("获取新消息成功");
                  }
                  else if (code == 20100)
                  {
                      Debug.Log("获取新消息失败");
                  }
                  else if (code == 30000)
                  {
                      Debug.Log("动态页面打开");
                  }
                  else if (code == 30100)
                  {
                      Debug.Log("动态页面关闭");
                  }
                  else if (code == 50000)
                  {
                      Debug.Log("取消关闭所有动态界面（弹框点击取消按钮）");
                  }
                  else if (code == 50100)
                  {
                      Debug.Log("确认关闭所有动态界面（弹框点击确认按钮）");
                  }
                  else if (code == 60000)
                  {
                      Debug.Log("动态页面内登录成功");
                  }
                  else if (code == 70000)
                  {
                      Debug.Log("场景化入口回调");
                  }
              });

        //定时调用获取消息通知的接口，有新信息时可以在 TapTap 动态入口显示小红点，提醒玩家查看新动态。
        TapMoment.FetchNotification();

    }


    public void InitTapAchievement()
    {
        //注册监听回调
        TapAchievement.RegisterCallback(this);
        //初始化数据
        TapAchievement.InitData();

        // 获取本地数据
        TapAchievement.GetLocalAllAchievementList((list, code) =>
        {
            if (code != null)
            {
                // 获取成就数据失败
            }
            else
            {
                // 获取成就数据成功
            }
        });
        // 获取服务器数据
        TapAchievement.FetchAllAchievementList((list, code) =>
        {
            if (code != null)
            {
                // 获取成就数据失败
            }
            else
            {
                // 获取成就数据成功
            }
        });

        //获取当前用户成就数据

        // 获取本地数据
        TapAchievement.GetLocalUserAchievementList((list, code) =>
        {
            if (code != null)
            {
                // 获取成就数据失败
            }
            else
            {
                // 获取成就数据成功
            }
        });
        // 获取服务器数据
        TapAchievement.FetchUserAchievementList((list, code) =>
        {
            if (code != null)
            {
                // 获取成就数据失败
            }
            else
            {
                // 获取成就数据成功
            }
        });

    }
    //设置「已点击全部 TDS SDK 功能」的成就
    public void SetTDSAchievements()

    {
        // name = openForum/openAchievement/openLeaderboard
        string openForum = PlayerPrefs.GetString("openForum");
        string openAchievement = PlayerPrefs.GetString("openAchievement");
        string openLeaderboard = PlayerPrefs.GetString("openLeaderboard");

        if (openForum == "opened" && openAchievement == "opened" && openLeaderboard == "opened")
        {
            // displayID 是在开发者中心中添加成就时自行设定的成就 ID
            TapAchievement.Reach("TDS_ALL");
        }

    }

    public void OnAchievementSDKInitSuccess()
    {
        Debug.Log("成就初始化成功");
    }

    public void OnAchievementSDKInitFail(TapError errorCode)
    {
        if (errorCode != null)
        {
            Debug.Log("成就初始化失败");
        }
    }

    public void OnAchievementStatusUpdate(TapAchievementBean bean, TapError errorCode)
    {
        if (errorCode != null)
        {
            // 成就状态更新失败
            return;
        }

        if (bean != null)
        {
            // 成就状态更新成功
        }
    }

    //打开内嵌动态
    public void openForum()
    {
        Debug.Log("打开动态");
         TapMoment.Open(Orientation.ORIENTATION_LANDSCAPE);

        //记录已经打开过内嵌动态，用于实现「已点击全部 TDS SDK 功能」成就
        PlayerPrefs.SetString("openForum", "opened");
        SetTDSAchievements();

    }

    //跳转成就
    public void openAchievement()
    {
        Debug.Log("打开成就");
        TapAchievement.ShowAchievementPage();


        //记录已经打开过成就，用于实现「已点击全部 TDS SDK 功能」成就
        PlayerPrefs.SetString("openAchievement", "opened");
        SetTDSAchievements();

    }

    //跳转排行榜
    public void openLeaderboard()
    {
        Debug.Log("打开排行榜");
        SceneManager.LoadScene("Leaderboard");

        //记录已经打开过排行榜，用于实现「已点击全部 TDS SDK 功能」成就
        PlayerPrefs.SetString("openLeaderboard", "opened");
        SetTDSAchievements();

    }

    //打开客服
    public void OpenTapSupport()
    {
        Debug.Log("打开客服");
    }
    //加载头像
    IEnumerator GetTexFromUnityWebRequest(string imageUrl)
    {
        using var request = UnityWebRequest.Get(imageUrl);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.result);
            Debug.Log(request.error);
        }
        else
        {
            var texture = new Texture2D(150, 150);
            texture.LoadImage(request.downloadHandler.data);
            var sprite = Sprite.Create(
                texture,
                new Rect(80, 80, 160, 160),
                new Vector2(0.5f, 0.5f)
            );
            avatar.sprite = sprite;
            avatar.SetNativeSize();
            Resources.UnloadUnusedAssets();
        }
    }
    public void TestPayment() {        TapPayment.QueryProduct("Apple", (skuDetail, error) =>
        {
            if (error != null)

            {
                Debug.Log("查询商品成功");

                TapPayment.LaunchBillingFlow(skuDetail, "huli", "serverid", "{\"test\":\"test\"}", (responseCode, error) =>
                {
                    if (error != null)
                    {
                        // native bridge exception
                    }
                    else
                    {
                        if (responseCode == 0)
                        {
                            Debug.Log("购买完成");

                            // complete
                        }
                        else if (responseCode == 1)
                        {
                            // error
                        }
                        else if (responseCode == 2)
                        {
                            // user cancel
                        }
                    }
                });
            }
            else
            {
                if (skuDetail == null)
                {
                    // not found any product with given skuId
                }
                else
                {
                    // do something
                }
            }
        });    }}