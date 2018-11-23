using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LabPanel : PanelBase
{
    private Button Btn_Close;//关闭按钮
    public static LabPanel Instance;
    private GameObject Cover;//遮罩
    private int selectedTreeRecord=1;//上次点击的记录

    private List<LabLevel> labLevels;
    private List<LabTypeItem> labTypeItems;

    private RectTransform ScrollContent;//
    private RectTransform TypeScrollContent;//

    private LabDetail labDetail;//详细面板

    private int selectedItemID;//
    private LabItem selectedItem;//

    private Text coin;
    private Text ruby;

    private Text progress;//项目研究进度

    private Queue<int> unlockFinishedQueue;//解锁完成待显示的科技队列

    private void Awake()
    {
        transform.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    /// <summary>
    /// 进入面板时
    /// </summary>
    public override void OnPanelEnter()
    {
        coin.text = Static.Player.GoldCoin.ToString();
        ruby.text = Static.Player.Ruby.ToString();

        // SetScrollMove();
        //改变scroll view
        ScrollContent.anchoredPosition = new Vector2(ScrollContent.anchoredPosition.x, 0);

        //缩放动画
        //Ground.transform.localScale = Vector3.zero;
        //Ground.transform.DOScale(Vector3.one, 0.3f).SetRelative().OnComplete(SetScrollstop);
        
        transform.gameObject.SetActive(true);

        if (LabUtil.Instance.pageIndex!=0)
        {
            selectedTreeRecord = LabUtil.Instance.pageIndex;
            LabUtil.Instance.pageIndex = 0;
        }
       // ClickType(selectedTreeRecord);
        StartCoroutine(DoubleRefresh());

        CancelInvoke("CheckTechFinished");
        InvokeRepeating("CheckTechFinished", 0, 3);
    }
    
    /// <summary>
    /// 特别处理
    /// </summary>
    /// <returns></returns>
    IEnumerator DoubleRefresh()
    {
        SetTree(selectedTreeRecord);
        yield return new WaitForSeconds(0.01f);
        SetTree(selectedTreeRecord);
    }


    public override void OnPanelExit()
    {
        CancelInvoke("CheckTechFinished");
        transform.gameObject.SetActive(false);
    }

    public override void OnPanelPause()
    {
        CancelInvoke("CheckTechFinished");
        transform.gameObject.SetActive(false);
    }

    /// <summary>
    /// 从其他面板回到研究所
    /// </summary>
    public override void OnPanelResume()
    {
        transform.gameObject.SetActive(true);
    }

    /// <summary>
    /// 第一次进入面板
    /// </summary>
    public override void OnStart()
    {
        InitLab();
        Instance = this;
        Settext();
        Multilingual.RegisterMultilingual(Settext);
    }

    /// <summary>
    /// 检测是否有科技解锁完成
    /// </summary>
    private void CheckTechFinished()
    {
        Debug.LogError("CheckTechFinished");
        foreach (int id in LabUtil.Instance.GetFinishedUnconfirmTechs())
        {
            if (!unlockFinishedQueue.Contains(id))
            {
                unlockFinishedQueue.Enqueue(id);
            }
        }

        //显示倒计时结束未确认的科技
        ShowFinishedTechInQueue();
    }

    /// <summary>
    /// 初始化研究所
    /// </summary>
    private void InitLab()
    {
        LuaManager.Instance.InitLuaTable(GetType().Name);

        TypeScrollContent = transform.Find("Head/Scroll View/Viewport/Content").GetComponent<RectTransform>();
        ScrollContent = transform.Find("ContentGround/Scroll View/Viewport/Content").GetComponent<RectTransform>();

        coin = transform.Find("Head/CoinShow/Text").GetComponent<Text>();
        ruby = transform.Find("Head/RubyShow/Text").GetComponent<Text>();

        Btn_Close = transform.Find("Head/Btn_Close").GetComponent<Button>();
        Btn_Close.onClick.AddListener(()=> { SoundManager.instance.Play("btn-click"); BoothManager.Instance.PanelPop(); });

        labDetail = transform.Find("Detail").GetComponent<LabDetail>();

        progress = transform.Find("Bottom/Text").GetComponent<Text>();

        Cover = transform.Find("Cover").gameObject;
        Cover.SetActive(false);


        labLevels = new List<LabLevel>();
        for (int i = 0; i < LabUtil.Instance.levelQuantity; i++)
        {
            LabLevel item = GenericityPool<LabLevel>.Instantiate("UI", "MainUIScene", "LabLevel", Vector3.one, Quaternion.identity);
            item.transform.SetParent(ScrollContent, false);
            item.transform.localScale = Vector3.one;
            //Canvas canvas= item.GetComponent<Canvas>();
            //canvas.overrideSorting = true;
            labLevels.Add(item);
        }

        unlockFinishedQueue = new Queue<int>();

        InitType();
    }

    /// <summary>
    /// 初始化科技树类型选项
    /// </summary>
    private void InitType()
    {
        //个数
        labTypeItems = new List<LabTypeItem>();

        int quantity = LabUtil.Instance.positionMsg.Count;
        for (int i = 0; i < quantity; i++)
        {
            LabTypeItem item = GenericityPool<LabTypeItem>.Instantiate("UI", "MainUIScene", "LabTypeItem", Vector3.one, Quaternion.identity);
            item.transform.SetParent(TypeScrollContent.transform);
            item.transform.localScale = Vector3.one;
            labTypeItems.Add(item);

            int temp = i + 1;
            item.SetContent(temp);
            item.GetComponent<Button>().onClick.AddListener(() => {
                ClickType(temp);
            });
        }
    }


    /// <summary>
    /// 点击科技类型 比如英雄科技
    /// </summary>
    public void ClickType(int key)
    {
        SoundManager.instance.Play("learnUI_pickLeft");
        SetTree(key);
        selectedTreeRecord = key;
    }


    /// <summary>
    /// 显示某一个科技树
    /// </summary>
    /// <param name="i"></param>
    private void SetTree(int treeID)
    {
        foreach (LabTypeItem item in labTypeItems)
        {
            item.SetChoosen(false);
        }
        labTypeItems[treeID - 1].SetChoosen(true);

        //更新数据
        LabUtil.Instance.UpdateUnlockMsg();
        Json.Log(LabUtil.Instance.positionMsg);
        Debug.LogError(treeID);
        int[,] position = LabUtil.Instance.positionMsg[treeID];
        int width = position.GetLength(0);

        //科技树进度
        progress.text = Multilingual.GetLanguage("Lab_Progress") + "   " +LabUtil.Instance.GetProgress();

        for (int i = 0; i < LabUtil.Instance.levelQuantity; i++)
        {
            //赋值
            int[] xulie = new int[4];
           
            //不显示的赋值-1
            for (int j = 0; j < 4; j++)
            {
                if (j >= width)
                {
                    xulie[j] = -1;
                }
            }
            for (int x = 0; x < 4; x++)
            {
                if(xulie[x]!=-1)
                xulie[x] = position[x, i];
            }
            labLevels[i].Setcontent(i+1,LabUtil.Instance.labDicDictionary[i+1].Unlocked,xulie);
           // labLevels[i].Setcontent(i+1,true,xulie);
        }

        //如果最低的未解锁阶位所需声望满足 则解锁按钮可以点击
        if (LabUtil.Instance.GetMinLockedLevel()<=RankFameManager.Instance.CurrentRank)
        labLevels[LabUtil.Instance.GetMinLockedLevel() - 1].SetUnlockAble(true);
    }



    /// <summary>
    /// /点击解锁阶位
    /// </summary>
    public void ClickUnlockLevel(int id)
    {
        //通知服务器
        if (Static.Player.GoldCoin < LabUtil.Instance.labDicDictionary[id].CostCoin)
        {
            TipPanel.TipItemShow(Multilingual.GetLanguage("Toust_Coin_NotEnouth"));
            return;
        }
        int heroType = LabUtil.Instance.labDicDictionary[id].Hero;
        if (!Static.Player.Technologys.ContainsKey(heroType))
        {
            Static.Player.Technologys[heroType] = new List<int>();
        }

        selectedItem = null;
        selectedItemID = id;
        ShowCover();
        DataServerSend.LogicTechnology(DataServerMessage.TechnologyCode.TechnologyUnLock, id, LabUtil.Instance.labDicDictionary[id].Hero);
    }

    /// <summary>
    /// 点击解锁科技项
    /// </summary>
    /// <param name="id"></param>
    public void ClickTech(int id,LabItem labItem)
    {
        selectedItem = labItem;
        labDetail.SetContent(id);
        labDetail.ShowDetail();
    }

    /// <summary>
    /// 弹出面板后点击确定解锁
    /// </summary>
    public void ClickUnlock(int id)
    {
        //前置科技未解锁
        if (!LabUtil.Instance.IsConditionSatisfying(id))
        {
            TipPanel.TipItemShow(Multilingual.GetLanguage("Lab_Precondition_Tips"));
            return;
        }
        //金币不足
        else if (Static.Player.GoldCoin< LabUtil.Instance.labDicDictionary[id].CostCoin)
        {
            TipPanel.TipItemShow(Multilingual.GetLanguage("Toust_Coin_NotEnouth"));
            return;
        }
        //回调
        selectedItemID = id;
        ShowCover();
        DataServerSend.LogicTechnology(DataServerMessage.TechnologyCode.TechnologyUnLock, id, LabUtil.Instance.labDicDictionary[id].Hero);
    }

    /// <summary>
    /// 解锁开始倒计时回调
    /// </summary>
    public void UnlockStart(int id)
    {
        SetTree(selectedTreeRecord);
        HideCover();
        // 如果是科技项 
        if (selectedItem!=null)
        {
            labDetail.SetContent(selectedItemID);
            SoundManager.instance.Play("btn-click");
            TipPanel.TipItemShow(Multilingual.GetLanguage("Lab_UnlockStart_Tips"));

            //热云统计
            ReYunGame.Instance.Game_SetEconomy("研究所解锁科技" + id + ":金币", 1, LabUtil.Instance.labDicDictionary[selectedItemID].CostCoin);
        }
        //如果是阶位解锁
        else
        {
            //TipPanel.TipItemShow("解锁成功");

            labLevels[id-1]. PlayerEffect();
            SoundManager.instance.Play("lab-level-up");
            labDetail.HideDetail();
            ShowCover();
            DataServerSend.CheckTimerFinished(DataServerMessage.TimerRequestCode.Timer_Start, 1, 255, id);
            //热云统计
            ReYunGame.Instance.Game_SetEconomy("研究所解锁阶位" + id, 1, 0);
        }
        
        UIRoot.Instance.OnChangeCoin(Static.Player.GoldCoin -= LabUtil.Instance.labDicDictionary[selectedItemID].CostCoin);
        coin.text = Static.Player.GoldCoin.ToString();

    }

    /// <summary>
    /// 科技已经解锁
    /// </summary>
    public void AlreadyUnlock()
    {
        Static.Player.Technologys[LabUtil.Instance.labDicDictionary[selectedItemID].Hero].Add(selectedItemID);
        SetTree(selectedTreeRecord);
        labDetail.HideDetail();
        HideCover();
    }
    

    /// <summary>
    /// 科技解锁确认后服务器回调
    /// </summary>
    /// <param name="id"></param>
    public void UnlockFinishedCallBack(int id,int costRuby)
    {
       // labDetail.HideDetail();
        //从队列中移除该科技
        if (unlockFinishedQueue.Contains(id))
        {
            List<int> list = new List<int>(unlockFinishedQueue.ToArray());
            list.Remove(id);
            unlockFinishedQueue = new Queue<int>(list);
        }

        //解锁成功
        LabUtil.Instance.SetTechUnlocked(id);


        ////播放动画
        if (id >= 1000)
        {
            SetTree(id / 1000);
            GetItemByID(id).HideOrangeLine();
            int y = 0;
            int rank = LabUtil.Instance.labDicDictionary[id].Rank;
            if (rank>1)
            {
                y = (rank-1)*330;
            }
            GetItemByID(id).SetLocked();
            ScrollContent.DOAnchorPos(new Vector2(0, y), 0.8f).OnComplete(()=> {
                GetItemByID(id).PlayUnlockAnimation();
            });

            if (costRuby>0)
            {
                //热云统计
                ReYunGame.Instance.Game_SetEconomy("研究所解锁科技" + id + ":红宝石", 1, costRuby);
            }
        }
        else
        {
            SetTree(selectedTreeRecord);
        }


        //扣除红宝石
        UIRoot.Instance.OnChangeRuby(Static.Player.Ruby -= costRuby);
        ruby.text = Static.Player.Ruby.ToString();
        //刷新面板
        labDetail.SetContent(id);
        if (costRuby>0)
        {
            labDetail.HideDetail();
        }

        //弹出提示
       // TipPanel.TipItemShow("解锁成功");

        //关闭遮罩
        HideCover();
    }

   /// <summary>
   /// 倒计时结束点击确认
   /// </summary>
   /// <param name="id"></param>
    private void ClickUnlockFinishedConfirm(int id)
    {
        DataServerSend.CheckTimerFinished(DataServerMessage.TimerRequestCode.Timer_Start, 1,255,id);
    }

    /// <summary>
    /// 根据id获取item 前提必须是当前页
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private LabItem GetItemByID(int id)
    {
        foreach (LabLevel lablevel in labLevels)
        {
            foreach (LabItem item in lablevel.GetItems())
            {
                if (item.GetID()== id)
                {
                    return item;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 广告加速或者解锁未完成 更新开始解锁时间
    /// </summary>
    public void UpdateTechStartTime(int id,long startTime)
    {
        
        LabUtil.Instance.labDicDictionary[id].SetUnlockStartTime(startTime);
        //更新面板
        labDetail.SetContent(id);
        SetTree(selectedTreeRecord);
        HideCover();
    }

    /// <summary>
    /// 显示倒计时已结束的科技
    /// </summary>
    private void ShowFinishedTechInQueue()
    {
        if (unlockFinishedQueue.Count<=0||labDetail.isActiveAndEnabled)
        {
            return;
        }
        labDetail.SetContent(unlockFinishedQueue.Dequeue());
        labDetail.ShowDetail();
    }

    /// <summary>
    /// 将科技传到主面板的解锁倒计时结束未确认队列
    /// </summary>
    public void AddTechToFinishedUnconfirmQueue(int id)
    {
        //防止重复进入队列
        if (unlockFinishedQueue.Contains(id))
        {
            return;
        }
        unlockFinishedQueue.Enqueue(id);
    }
    

    private void Settext()
    {

        labDetail.SetText();
    }
    
    /// <summary>
    /// 点击关闭面板
    /// </summary>
    private void ClickBack()
    {
        SoundManager.instance.Play("btn-click");
        BoothManager.Instance.PanelPop();
    }
    

    public void ShowCover()
    {
        WaitPanel.Show();
        Cover.SetActive(true);
        Invoke("HideCover", 3);

    }
    public void HideCover()
    {
        WaitPanel.Hide();
        Cover.SetActive(false);
        if (IsInvoking("HideCover"))
        {
            CancelInvoke("HideCover");
        }
    }

    //防止scrollview滑动方法
    private void SetScrollstop()
    {
        ScrollRect TypeScroll = transform.GetComponentInChildren<ScrollRect>();
        TypeScroll.movementType = ScrollRect.MovementType.Elastic;
        TypeScroll.inertia = true;
    }

    private void SetScrollMove()
    {
        ScrollRect TypeScroll = transform.GetComponentInChildren<ScrollRect>();
        TypeScroll.movementType = ScrollRect.MovementType.Unrestricted;
        TypeScroll.inertia = false;
    }
}
