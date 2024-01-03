using System.Collections.Generic;
using LeanCloud.Storage;
using TapTap.UI.AillieoTech;
using UnityEngine;
using UnityEngine.UI;

public class ListView_score : MonoBehaviour
{
    public Image avatar;
    public struct RankItemData
    {
        // 名次
        public int rank;
        // 名字
        public string name;
        // 分数
        public string score;
    }

    List<RankItemData> testData = new List<RankItemData>();

    public ScrollView scrollView;

    private void Start()
    {
        // 构造测试数据
        InitData();


        scrollView.SetUpdateFunc((index, rectTransform) =>
        {
            // 更新item的UI元素
            RankItemData data = testData[index];
            rectTransform.gameObject.SetActive(true);
            rectTransform.Find("rankText").GetComponent<Text>().text = data.rank.ToString();
            rectTransform.Find("nameText").GetComponent<Text>().text = data.name;
            rectTransform.Find("scoreText").GetComponent<Text>().text = data.score;

        });
        scrollView.SetItemSizeFunc((index) =>
        {
            // 返回item的尺寸
            RankItemData data = testData[index];
            if (data.rank <= 3)
            {
                return new Vector2(1350, 140);
            }
            else
            {
                return new Vector2(1350, 120);
            }
        });
        scrollView.SetItemCountFunc(() =>
        {
            // 返回数据列表item的总数
            return testData.Count;
        });

        scrollView.UpdateData(false);

    }

    private async void InitData()
    {
        var leaderboard = LCLeaderboard.CreateWithoutData("CherryNum");
        //注意，排行榜显示昵称就用 nickname，排行榜显示用户名就用 username。
        var rankings = await leaderboard.GetResults(limit: 200, selectKeys: new List<string> { "username", "nickname" });
        foreach (var statistic in rankings)
        {
            //Debug.Log("排行榜的名称是：" + statistic.StatisticName);
            //Debug.Log("排行榜的分数是：" + statistic.Value);
            //Debug.Log("排名是：" + statistic.Rank);
            //Debug.Log("用户 ID 是：" + statistic.User.ObjectId);
            //Debug.Log("用户昵称：" + statistic.User["nickname"]);

            RankItemData data = new RankItemData();
            data.rank = statistic.Rank + 1;
            data.name = statistic.User.Username;
            data.score = statistic.Value.ToString();
            //可选
            // data.name = statistic.User["nickname"].ToString();
            testData.Add(data);
        }
        scrollView.UpdateData(true);
    }
}
