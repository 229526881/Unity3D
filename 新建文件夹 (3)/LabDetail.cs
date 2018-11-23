using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LabDetail : MonoBehaviour {

    private int id;
    private Text name;//名字
    private Text message;//内容信息
    private Text dick;//
    private Text tip;//
    private Button btn_back;//返回
    private Button btn_unlock;//解锁
    private Button btn_confirm;//确认
    private Button btn_adSkip;//广告跳过
    private Button btn_rubySkip;//红宝石跳过
    private Text btn_rubySkip_text;//红宝石跳过需要消耗的数量
    private GameObject countDown;//倒计时
    private int ruby;//
    private bool inited = false;
    private Button btn_background;//点击背景

    private int potNum = 0;//记录 “科技研发中...”点的个数
    /// <summary>
    /// 初始化
    /// </summary>
    private void Init()
    {
        if (inited)
        {
            return;
        }
        inited = true;
        name = transform.Find("Ground/Name").GetComponent<Text>();
        message = transform.Find("Ground/Content/Message").GetComponent<Text>();
        dick = transform.Find("Ground/Content/Dick").GetComponent<Text>();
        tip = transform.Find("Ground/Tip").GetComponent<Text>();
        btn_back = transform.Find("Ground/Buttons/Back").GetComponent<Button>();
        btn_back.onClick.AddListener(()=> { SoundManager.instance.Play("btn-click"); HideDetail(); });
        btn_unlock = transform.Find("Ground/Buttons/Unlock").GetComponent<Button>();
        btn_unlock.onClick.AddListener(()=>{ LabPanel.Instance.ClickUnlock(id); });
        btn_confirm = transform.Find("Ground/Buttons/Confirm").GetComponent<Button>();
        btn_confirm.onClick.AddListener(() => { SoundManager.instance.Play("btn-click"); HideDetail(); });

        btn_adSkip = transform.Find("Ground/Buttons/AdSkip").GetComponent<Button>();
        btn_adSkip.onClick.AddListener(() => { SoundManager.instance.Play("btn-click");ClickAdSkip();  });
        btn_rubySkip = transform.Find("Ground/Buttons/RubySkip").GetComponent<Button>();
        btn_rubySkip.onClick.AddListener(() => { SoundManager.instance.Play("btn-click");ClickRubySkip(); });
        btn_rubySkip_text = btn_rubySkip.transform.Find("Image/Text").GetComponent<Text>();
        countDown = transform.Find("Ground/CountDown").gameObject;

        btn_background = transform.GetComponent<Button>();
        btn_background.onClick.AddListener(HideDetail);
        //广告

        AdvertisementManager.Instance.AddOnPlayEvent(AdvertisementManager.AdvertisementType.TechUnlcokSkip, () => DataServerSend.StartAd());
        AdvertisementManager.Instance.AddPlayEndEvent(AdvertisementManager.AdvertisementType.TechUnlcokSkip, OnRewardFinished);
    }

    /// <summary>
    /// 广告结束 给服务器发送消息
    /// </summary>
    private void OnRewardFinished()
    {
        DataServerSend.CheckTimerFinished(DataServerMessage.TimerRequestCode.Timer_Reduce,1,255, id);
        LabPanel.Instance.ShowCover();
    }

    /// <summary>
    /// 设置多语言
    /// </summary>
    public void SetText()
    {
        Init();
        btn_unlock.GetComponentInChildren<Text>().text = Multilingual.GetLanguage("Btn_UnLock");
        btn_back.GetComponentInChildren<Text>().text = Multilingual.GetLanguage("Btn_Cancel");
        btn_confirm.GetComponentInChildren<Text>().text = Multilingual.GetLanguage("Btn_Sure");
        btn_rubySkip.GetComponentInChildren<Text>().text =Multilingual.GetLanguage("Lab_Btn_FinishAtOnce");
            //Multilingual.GetLanguage("Btn_Sure");
    }

    /// <summary>
    /// 显示详细面板
    /// </summary>
    public void ShowDetail()
    {
        if (id<1000)
        {
            return;
        }
        gameObject.SetActive(true);
    }


    /// <summary>
    /// 隐藏详细面板
    /// </summary>
    public void HideDetail()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 更新红宝石消耗数量
    /// </summary>
    public void UpdateRuby()
    {
        LabDic msg = LabUtil.Instance.labDicDictionary[id];
         ruby =( msg.Time - (int)(XTime.Now - msg.UnlockStartTime).TotalSeconds-1)/120+1;
        btn_rubySkip_text.text =ruby>0? ruby.ToString():"0";
        btn_adSkip.GetComponentInChildren<Text>().text=ruby>15 ? Multilingual.GetLanguage("Lab_Btn_SkipHalfHour"): Multilingual.GetLanguage("Lab_Btn_FinishAtOnce");
        SetTipsString();
    }

    /// <summary>
    /// 动态设置提示文本
    /// </summary>
    /// <returns></returns>
    private void SetTipsString()
    {
        LabDic msg = LabUtil.Instance.labDicDictionary[id];
        potNum += 10;
        if (potNum >= 600)
        {
            //点的个数
            string pot = "";
            potNum = (potNum % 600 + 1) % 4;
            for (int i = 0; i < potNum; i++)
            {
                pot += ".";
            }
            //进度
            int progress = (int)(XTime.Now - msg.UnlockStartTime).TotalSeconds * 100 / msg.Time;
            string str = Multilingual.GetLanguage("Lab_Unlocking_Tips") + "..." + pot + progress + "%";
            for (int i = 0; i < 4 - potNum; i++)
            {
                str += " ";
            }
            tip.text = str;
        }
    }

    /// <summary>
    /// 点击红宝石跳过
    /// </summary>
    private void ClickRubySkip()
    {
        //如果红宝石不足
        if (Static.Player.Ruby<ruby)
        {
            TipPanel.TipItemShow(Multilingual.GetLanguage("Toust_Ruby_NotEnouth"));
            return;
        }

        DataServerSend.LogicTechnology(DataServerMessage.TechnologyCode.TechnologySkip, id, 255);
        LabPanel.Instance.ShowCover();
    }

    /// <summary>
    /// 点击广告跳过
    /// </summary>
    private void ClickAdSkip()
    {
        LabPanel.Instance.ShowCover();
        AdvertisementManager.Instance.Play(AdvertisementManager.AdvertisementType.TechUnlcokSkip);
    }


    /// <summary>
    /// 填充内容
    /// </summary>
    public void SetContent(int id)
    {
        Init();

        LabDic msg = LabUtil.Instance.labDicDictionary[id];
        
        this.id = id;

      
        //名字
        name.text = Multilingual.GetLanguage("Lab_Tech_" + msg.Name + "_info1");
        //内容
        message.text = Multilingual.GetLanguage("Lab_Tech_" + msg.Name + "_info2") .Replace("\\n", "\n"); ;
        dick.text = Multilingual.GetLanguage("Lab_Tech_" + msg.Name + "_info3");
        //提示
        string[] str = Multilingual.GetLanguage("Lab_Unlock_Tips").Split('|');
          tip.text = msg.Unlocked ? Multilingual.GetLanguage("Lab_Alreadyunlcok") :str[0]+ "<color=#FB6A11FF>" + msg.CostCoin+ "</color>" + str[1];
        //Lab_Alreadyunlcok
        //按钮
        btn_confirm.onClick.RemoveListener(UnlockConfirm);
        btn_background.onClick.RemoveListener(UnlockConfirm);

        //如果是解锁倒计时结束未点击确定
        if (msg.IsUnlockFinishedUnconfirm())
        {
            //按钮
            btn_adSkip.gameObject.SetActive(false);
            btn_rubySkip.gameObject.SetActive(false);
            btn_back.gameObject.SetActive(false);
            btn_unlock.gameObject.SetActive(false);
            btn_confirm.gameObject.SetActive(true);
            btn_confirm.onClick.AddListener(UnlockConfirm);
            btn_background.onClick.AddListener(UnlockConfirm);
            //倒计时
            countDown.SetActive(false);
            //提示
            tip.text = Multilingual.GetLanguage("Lab_UnlockSuccess_Tips");
        }
        //如果是正在解锁
        else if (!msg.Unlocked&& msg.IsUnlocking)
        {
            //按钮
            btn_back.gameObject.SetActive(true);
            btn_adSkip.gameObject.SetActive(true);
            btn_rubySkip.gameObject.SetActive(true);
            btn_unlock.gameObject.SetActive(false);
            btn_confirm.gameObject.SetActive(false);
            //提示
            tip.text =Multilingual.GetLanguage("Lab_Unlocking_Reading_Tips");
            potNum = 0;
               // Multilingual.GetLanguage("Lab_Unlocking_Tips");
            //倒计时
            countDown.SetActive(true);
            //剩余时间
            float restTime = msg.Time - (float)(XTime.Now - msg.UnlockStartTime).TotalSeconds;
            countDown.GetComponentInChildren<CountDown>().SetCountDown(restTime, true);
            countDown.GetComponentInChildren<CountDown>().SetCountDownUpdateEvent(UpdateRuby);
            countDown.GetComponentInChildren<CountDown>().SetCountDownOverEvent(()=> { StartCoroutine(RefreshDetail());  });
        }
        //如果是倒计时结束未确认
        
        else
        {
            //按钮
            btn_adSkip.gameObject.SetActive(false);
            btn_rubySkip.gameObject.SetActive(false);
            btn_back.gameObject.SetActive(!msg.Unlocked);
            btn_unlock.gameObject.SetActive(!msg.Unlocked);
            btn_confirm.gameObject.SetActive(msg.Unlocked);
            //倒计时
            countDown.SetActive(false);
        }


    }

    /// <summary>
    /// 刷新面板
    /// </summary>
    /// <returns></returns>
    private IEnumerator RefreshDetail()
    {
        LabPanel.Instance.ShowCover();
        yield return new WaitForSeconds(1);
        SetContent(id);
        LabPanel.Instance.HideCover();
    }


    /// <summary>
    /// 解锁成功确认
    /// </summary>
    private void UnlockConfirm()
    {
        LabPanel.Instance.ShowCover();
        //向服务器发送消息
        DataServerSend.CheckTimerFinished(DataServerMessage.TimerRequestCode.Timer_Start, 1, 255, id);
    }


}
