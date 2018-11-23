using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LabItem : MonoBehaviour {

    private GameObject show;
    private int id;
    private Text name;//名字
    private GameObject cost;
    private GameObject costCoin;
    private GameObject costRuby;
    private Text coin;//金币数量
    private Text ruby;//红宝石数量
    private bool inited=false;//是否初始化
    private LabDic labDic;//相关数据
    private RectTransform grayLine;
    private RectTransform orangeLine;
    private bool locked = true;//是否锁着
    private GameObject unlockedPattern;//已解锁的背景图片
    private GameObject lockedPattern;//未解锁的背景图片
    private GameObject lockImg;//锁的图片
    private Slider countDownSlider;//倒计时进度条
    private GameObject countDown;//倒计时
    private Text countDownText;//倒计时时间文本
    
    private void Init()
    {
        CommonEffect effect = Pool.GetGameObject("UI", "MainUIScene", "Lab_Building_eff", Vector3.one, Quaternion.identity).GetComponent<CommonEffect>(); 
        effect.transform.SetParent(transform.Find("Show"));
        effect.transform.localPosition = new Vector3(177, 97, 0);
        effect.transform.localScale = new Vector3(0.5f, 0.5f);

        show = transform.Find("Show").gameObject;
        name = transform.Find("Show/Name").GetComponent<Text>();
        cost = transform.Find("Show/Patterns/LockedPattern/Cost").gameObject;
        costCoin = cost.transform.Find("Coin").gameObject;
        coin = costCoin.transform.Find("Text").GetComponent<Text>();
        costRuby = cost.transform.Find("Ruby").gameObject;
        ruby = costRuby.transform.Find("Text").GetComponent<Text>();
        grayLine = transform.Find("Show/Line/Gray").GetComponent<RectTransform>();
        orangeLine = transform.Find("Show/Line/Orange").GetComponent<RectTransform>();
        unlockedPattern = transform.Find("Show/Patterns/UnlockedPattern").gameObject;
        lockedPattern = transform.Find("Show/Patterns/LockedPattern").gameObject;
        lockImg = lockedPattern.transform.Find("Lock").gameObject;
        countDown = lockedPattern.transform.Find("CountDown").gameObject;
        countDownSlider = countDown.GetComponentInChildren<Slider>();
        countDownText = countDown.transform.Find("TimeText").GetComponent<Text>();

        inited = true;

        transform.GetComponent<Button>().onClick.AddListener(() => {
            SoundManager.instance.Play("lab-item-click");
            LabPanel.Instance.ClickTech(id,this);
        });
    }

    /// <summary>
    /// 获取id
    /// </summary>
    public int GetID()
    {
        return id;
    }

    /// <summary>
    /// 控制内容是否显示
    /// </summary>
    /// <param name="i"></param>
    public void IsShow(bool i)
    {
        show.SetActive(i);
    }

    /// <summary>
    /// 暂时将item表现为未解锁
    /// </summary>
    public void SetLocked()
    {
        unlockedPattern.SetActive(false);
        lockedPattern.SetActive(true);
        countDown.SetActive(true);
        float restTime = labDic.Time - (float)(XTime.Now - labDic.UnlockStartTime).TotalSeconds;
        countDown.GetComponentInChildren<CountDown>().SetCountDown(restTime, false);
    }

    /// <summary>
    /// 播放解锁动画
    /// </summary>
    public void PlayUnlockAnimation()
    {
        unlockedPattern.SetActive(true);
        lockedPattern.SetActive(false);
        SoundManager.instance.Play("lab-unlock-confirm");
        float height =grayLine.sizeDelta.y;
        float second = height/60;
        orangeLine.sizeDelta = new Vector2(8, 20);
        orangeLine.DOSizeDelta(new Vector2(8, height), second);
        PlayUnlockEffect();
    }

    /// <summary>
    /// 播放特效
    /// </summary>
    private void PlayUnlockEffect()
    {
      
        CommonEffect effect = Pool.GetGameObject("UI", "MainUIScene", "Lab_Unlock_Item_eff", Vector3.one, Quaternion.identity).GetComponent<CommonEffect>();
        effect.transform.SetParent(transform);
        effect.transform.localScale = Vector3.one;
        effect.transform.localPosition = Vector3.zero;
        effect.Play(transform, 2f);
    }


    /// <summary>
    /// 播放特效
    /// </summary>
    private void PlayBuildingEffect()
    {
        if (transform.Find("Show/UIMainUISceneLab_Building_eff(Clone)") !=null)
        {
            transform.Find("Show/UIMainUISceneLab_Building_eff(Clone)").gameObject.SetActive(true);
            return;
        }
      
        //effect.GetComponent<ParticleSystem>().Play(true);
    }



    /// <summary>
    /// 根据id初始化内容
    /// </summary>
    /// <param name="id"></param>
    public void Setcontent(int id)
    {
        if (!inited)
        {
            Init();
        }
       this.id = id;

       labDic = LabUtil.Instance.labDicDictionary[id];
        //名字 多语言
        name.text =Multilingual.GetLanguage("Lab_Tech_"+ labDic.Name+"_Name") ;
        //名字颜色
        //花费
        cost.SetActive(!labDic.Unlocked);
        costCoin.SetActive(labDic.CostCoin != 0);
        coin.text = labDic.CostCoin.ToString();
            costRuby.SetActive(labDic.CostRuby != 0);
        ruby.text = labDic.CostRuby.ToString();
        //背景颜色
        unlockedPattern.SetActive(labDic.Unlocked);
        lockedPattern.SetActive(!labDic.Unlocked);
        name.color = labDic.Unlocked ? new Color(99,19,12) : Color.white;
        //线
        if (labDic.Unlocked)
        {
        grayLine.sizeDelta = new Vector2(8, 350 * labDic.LineLength);
            orangeLine.sizeDelta = new Vector2(8, 350 * labDic.LineLength);
        }
        else
        {
            grayLine.sizeDelta = new Vector2(8, 350 * labDic.LineLength);
            orangeLine.sizeDelta = new Vector2(8, 0 * labDic.LineLength);
        }
        //锁的图片
        lockImg.SetActive(!labDic.IsUnlocking||!labDic.Unlocked);
        countDown.SetActive(false);

        if (transform.Find("Show/UIMainUISceneLab_Building_eff(Clone)") != null)
        {
            transform.Find("Show/UIMainUISceneLab_Building_eff(Clone)").gameObject.SetActive(false);
        }


        //倒计时
        if (labDic.IsUnlocking)
        {
            //播放解锁中动画
            PlayBuildingEffect();

            cost.SetActive(false);
            countDown.SetActive(true);
            //剩余时间
            float restTime = labDic.Time - (float)(XTime.Now - labDic.UnlockStartTime).TotalSeconds;
            countDown.GetComponentInChildren<CountDown>().SetCountDown(restTime, true);
            countDown.GetComponentInChildren<CountDown>().SetCountDownUpdateEvent(UpdateSlider);
            countDown.GetComponentInChildren<CountDown>().SetCountDownOverEvent(AddTechToFinishedUnconfirmQueue);
        }

        orangeLine.DOKill();
    }


    /// <summary>
    /// 更新进度条
    /// </summary>
    private void UpdateSlider()
    {
        countDownSlider.value = (float)countDown.GetComponentInChildren<CountDown>().GetTheRestOfTime() / (float)labDic.Time;
    }

    /// <summary>
    /// 将科技传到主面板的解锁倒计时结束未确认队列
    /// </summary>
    private void AddTechToFinishedUnconfirmQueue()
    {
        LabPanel.Instance.AddTechToFinishedUnconfirmQueue(id);
    }

    /// <summary>
    /// 隐藏橙色的线
    /// </summary>
    public void HideOrangeLine()
    {
        orangeLine.sizeDelta = new Vector2(8, 0 );
    }

}
