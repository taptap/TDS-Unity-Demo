using System.Collections.Generic;
using TapTap.UI.AillieoTech;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TabGroup : MonoBehaviour
{
    private Toggle[] m_Toggle;
    private Image[] m_Image;
    // private Image[] leaderboardicon;
    public Text scoreText;
    string[] score = new string[2];
    private ScrollView[] m_scrollView;

    void Start()
    {
        //找到组件
        m_Toggle = new Toggle[2];
        m_Toggle[0] = GameObject.Find("Panel_score").GetComponent<Toggle>();
        m_Toggle[1] = GameObject.Find("Panel_level").GetComponent<Toggle>();

        m_Image = new Image[2];
        m_Image[0] = GameObject.Find("Image_score").GetComponent<Image>();
        m_Image[1] = GameObject.Find("Image_level").GetComponent<Image>();

        m_scrollView = new ScrollView[2];
        m_scrollView[0] = GameObject.Find("ScrollView_score").GetComponent<ScrollView>();
        m_scrollView[1] = GameObject.Find("ScrollView_level").GetComponent<ScrollView>();

        m_Image[0].gameObject.SetActive(true);
        m_Image[1].gameObject.SetActive(false);
        m_scrollView[0].gameObject.SetActive(true);
        m_scrollView[1].gameObject.SetActive(false);


        //动态添加监听
        m_Toggle[0].onValueChanged.AddListener((isOn) => ToggleOnValueChanged(isOn, 0));
        m_Toggle[1].onValueChanged.AddListener((isOn) => ToggleOnValueChanged(isOn, 1));

        scoreText.text = "分数";
        score[0] = "分数";
        score[1] = "关卡数";
    }

    private void ToggleOnValueChanged(bool isOn, int index)
    {
        //其他页隐藏
        for (int i = 0; i < m_Image.Length; i++)
        {
            m_Image[i].gameObject.SetActive(false);
            m_scrollView[i].gameObject.SetActive(false);
            scoreText.text = score[i];
        }
        //显示特定页
        if (isOn)
        {
            m_Image[index].gameObject.SetActive(true);
            m_scrollView[index].gameObject.SetActive(true);
            scoreText.text = score[index];
        }
    }
    public void GoBackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}