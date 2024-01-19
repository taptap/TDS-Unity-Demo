using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TapTap.Connect;

public class PlayerController : MonoBehaviour
{
    // 声明一个刚体变量
    private Rigidbody2D rb;

    // 跑动起来的动画
    private Animator anim;

    // 碰撞体
    public Collider2D coll;

    //手机摇杆
    public Joystick joystick;

    // 声明一个速度变量
    public float speed;
    public AudioSource jumpAudio,
        cherryAudio;

    // 声明一个跳跃强度
    public float jumpforce;

    // 判断狐狸回到地面
    public LayerMask ground;

    public int Cherry;
    public Text CherryNum;
    private bool isHurt; //默认是 false

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();



        //隐藏悬浮窗
        TapConnect.SetEntryVisible(false);
    }

    // Update is called once per frame，
    // 每一帧都会调用 Update()，因为有的电脑达不到每一帧更新一次的运行效果，可用 FixedUpdate() 代替，使运行更平滑，使不同帧数电脑在操作手感上保持一致。
    void FixedUpdate()
    {
        if (!isHurt)
        {
            //电脑键盘控制移动
            movement();
            //手机摇杆控制移动
            movementInApp();
        }

        SwitchAnim();
    }

    // 设置一个移动函数，在每一帧产生画面效果。
    //电脑端移动效果
    void movement()
    {
        float horizontalmove = Input.GetAxis("Horizontal");
        //实现转身。返回三个值，0,1和-1。 -1左边移动，1右边移动，0不动。
        float facedirection = Input.GetAxisRaw("Horizontal");
        //角色移动，让狐狸跑起来
        if (horizontalmove != 0)
        {
            // rd.velocity = new Vector2(horizontalmove * speed, rd.velocity.y);
            // Time.fixedDeltaTime 这样相比上面比保证更平滑不跳帧
            rb.velocity = new Vector2(horizontalmove * speed * Time.fixedDeltaTime, rb.velocity.y);
            anim.SetFloat("running", Mathf.Abs(facedirection));
        }
        //左右方向跑的时候，狐狸切换朝向
        if (facedirection != 0)
        {
            // Vector3：三个维度
            transform.localScale = new Vector3(facedirection, 1, 1);
        }
        // 角色跳跃
        if (Input.GetButton("Jump"))
        {
            // Y轴跳跃力设置为 jumpforce * Time.fixedDeltaTime
            rb.velocity = new Vector2(rb.velocity.x, jumpforce * Time.deltaTime);
            jumpAudio.Play();
            //  设置跳跃动画
            anim.SetBool("jumping", true);
        }
    }

    //手机端用摇杆移动
    void movementInApp()
    {
        float horizontalmove = joystick.Horizontal;
        float facedirection = joystick.Horizontal;
        //角色移动
        if (horizontalmove != 0f)
        {
            rb.velocity = new Vector2(horizontalmove * speed * Time.fixedDeltaTime, rb.velocity.y);
            anim.SetFloat("running", Mathf.Abs(facedirection));
        }
        //左右方向跑的时候，狐狸切换朝向
        if (facedirection > 0f)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        if (facedirection < 0f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        // APP 摇杆跳跃
        if (joystick.Vertical > 0.5f)
        {
            // Y轴跳跃力设置为 jumpforce * Time.fixedDeltaTime
            rb.velocity = new Vector2(rb.velocity.x, jumpforce * Time.deltaTime);
            jumpAudio.Play();
            //  设置跳跃动画
            anim.SetBool("jumping", true);
        }
    }

    //  动画切换，跳跃，降落，停止
    void SwitchAnim()
    {
        // anim.SetBool("idle", false);
        if (rb.velocity.y < 0.1f && !coll.IsTouchingLayers(ground))
        {
            anim.SetBool("falling", true);
        }

        // 设置降落动画和停止跳跃动画
        if (anim.GetBool("jumping"))
        {
            // 如果 Y 轴跳跃力没有了，则停止跳跃动画，触发降落动画
            if (rb.velocity.y < 0)
            {
                anim.SetBool("jumping", false);
                anim.SetBool("falling", true);
            }
        }
        else if (isHurt)
        {
            anim.SetBool("hurt", true);
            anim.SetFloat("running", 0);
            //受伤结束
            if (Mathf.Abs(rb.velocity.x) < 0.1f)
            {
                anim.SetBool("hurt", false);
                // anim.SetBool("idle", true);
                isHurt = false;
            }
        }
        //下降过程是否碰撞到地面
        else if (coll.IsTouchingLayers(ground))
        {
            anim.SetBool("falling", false);
            // anim.SetBool("idle", true);
        }
    }

    //收集物品
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Collection")
        {
            cherryAudio.Play();
            //销毁收集的樱桃
            Destroy(other.gameObject);
            Cherry += 1;
            CherryNum.text = Cherry.ToString();
        }
        //本地记录樱桃数量，用于排行榜
        PlayerPrefs.SetInt("CherryNum",Cherry);
    }

    //消灭敌人
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            if (anim.GetBool("falling"))
            {
                Destroy(other.gameObject);
                //杀死私人后跳跃
                rb.velocity = new Vector2(rb.velocity.x, jumpforce * Time.fixedDeltaTime);
                anim.SetBool("jumping", true);
            }
            //左侧碰撞
            else if (transform.position.x < other.gameObject.transform.position.x)
            {
                rb.velocity = new Vector2(-10, rb.velocity.y);
                isHurt = true;
            }
            //右侧碰撞
            else if (transform.position.x > other.gameObject.transform.position.x)
            {
                rb.velocity = new Vector2(10, rb.velocity.y);
                isHurt = true;
            }
        }
    }
}


