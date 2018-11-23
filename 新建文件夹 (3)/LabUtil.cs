using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 成就方法模板类
/// </summary>
public class LabDic
{
    public int ID;//ID
    public string Name;//名字
    public int CostRuby;//红宝石
    public int CostCoin;//金币
    public string Preconditions;//前置条件
    public int Rank;//声望等级
    public int Hero;//英雄
    public bool Unlocked = false;//是否解锁
    public bool IsUnlocking = false;//正在解锁
    public DateTime UnlockStartTime;//解锁开始时间
    public int LineLength=0;//到下一阶线的长度
    public int Time;//建造需要的时间 （分钟）
    public int SkipCostRuby;//跳过所需红宝石
    public int SkipCostCoin;//跳过所需金币
    public int Value;//值

    //是否解锁倒计时完成 未确认
    public bool IsUnlockFinishedUnconfirm()
    {
        //Debug.LogError(UnlockStartTime);
        //如果已经是解锁过了 返回false 
        if (Unlocked| UnlockStartTime.Year<=1970)
        {
            return false;
        }
        //如果是倒计时刚刚完成 未确认过
       else if (IsUnlocking&&(XTime.Now-UnlockStartTime).TotalSeconds> Time)
        {
            Debug.LogError(UnlockStartTime);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 填写解锁开始时间
    /// </summary>
    public void SetUnlockStartTime(long time)
    {
        UnlockStartTime =  new System.DateTime(1970, 1, 1, 8, 0, 0) + new System.TimeSpan(time * 10000);
        Static.Player.AddInToTimer(1, 255, ID);
        Static.Player.UpDateTimerTarget(1, 255, ID, time);
    }

   
}




/// <summary>
/// 成就工具类
/// </summary>
public class LabUtil
{

    public List<LabDic> labDics;//科技树信息
    public Dictionary<int, int[,]> positionMsg;//位置信息
    public Dictionary<int, LabDic> labDicDictionary;//id对应的字典
    private Dictionary<int, List<LabDic>> labTreeDictionary;//tree对应的id
    public int levelQuantity;//阶位总数
    public int pageIndex=0;//指定科技树页面

    //单例
    private static LabUtil instance;
    private LabUtil()
    {
        labDics = GetLabDics();
        labDicDictionary = GetLabDicDictionary();
        levelQuantity = GetLevelQuantity();
        positionMsg = GetPositions();
        SetLineLength();
    }


    public static LabUtil Instance
    {
        get
        {
            if (null == instance)
            {
                instance = new LabUtil();
            }
            return instance;
        }
    }

    /// <summary>
    /// 解锁成功
    /// </summary>
    public void SetTechUnlocked(int id)
    {
        labDicDictionary[id].Unlocked = true;
        labDicDictionary[id].IsUnlocking = false;

        Static.Player.Technologys[255].Add(id);
        //从计时器中删除
        Static.Player.RemoveTimerTarget(1, 255, id);
    }


    /// <summary>
    /// 设置界面
    /// </summary>
    /// <param name="index"></param>
    public void SetPage(int index)
    {
        pageIndex = index;
    }

    /// <summary>
    /// 前提条件是否满足
    /// </summary>
    public bool IsConditionSatisfying(int key)
    {
        string[] str = labDicDictionary[key].Preconditions.Split('|');
        foreach (string item in str)
        {
            int temp = int.Parse(item);
            if (!labDicDictionary[temp].Unlocked)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 获取科技树的进度
    /// </summary>
    public string GetProgress()
    {
        int temp = 0;
        int all = 0;
        foreach (LabDic item in labDicDictionary.Values)
        {
            if (item.ID/1000>0)
            {
                all++;
                if ( item.Unlocked)
                {
                    temp++;
                }
               
            }
        }
      return  temp+"/" + all;
    }

    /// <summary>
    /// 更新解锁信息
    /// </summary>
    public void UpdateUnlockMsg()
    {
 
        foreach (LabDic item in labDicDictionary.Values)
        {
            item.IsUnlocking = false;
            item.Unlocked = false; 
        }

        //更新正在解锁的科技 
        if (Static.Player.timer.ContainsKey(1))
        {
            if (Static.Player.timer[1].ContainsKey(255))
            {
                foreach (KeyValuePair<int, long> kv in Static.Player.timer[1][255])
                {
                    labDicDictionary[kv.Key].SetUnlockStartTime(kv.Value);
                    labDicDictionary[kv.Key].IsUnlocking = true;
                    labDicDictionary[kv.Key].Unlocked = false;
                }
            }
        }

        //更新已解锁的科技
        foreach (List<int> msg in Static.Player.Technologys.Values)
        {
            foreach (int item in msg)
            {
                labDicDictionary[item].Unlocked = true;
                labDicDictionary[item].IsUnlocking = false;
            }
        }
      

    }

    /// <summary>
    /// 给每个item赋值 线的长度
    /// </summary>
    private void SetLineLength()
    {
        for (int i = 0; i < labDics.Count-1; i++)
        {
            if (labDics[i + 1].ID / 100 == labDics[i].ID / 100)
            {
                labDics[i].LineLength = labDics[i + 1].Rank - labDics[i].Rank;
            }
            else labDics[i].LineLength = 0;
        }
    }

    /// <summary>
    /// 获取最低未解锁的阶位
    /// </summary>
    /// <returns></returns>
    public int GetMinLockedLevel()
    {
        //更新解锁信息
        UpdateUnlockMsg();
        int min = 100;
        for (int i = 1; i <= levelQuantity; i++)
        {
            if (!labDicDictionary[i].Unlocked)
            {
                min = labDicDictionary[i].ID;
                break;
            }
               
        }
        return min;
    }
    

    /// <summary>
    /// 获取科技树的数量 即type
    /// </summary>
    /// <returns></returns>
    private Dictionary<int,int[,]> GetPositions()
    {
        //根据不同的页面放入字典
        Dictionary<int, List<LabDic> > trees= new Dictionary<int, List<LabDic>>();
        foreach (LabDic item in labDics)
        {
            int i = item.ID / 1000;
            if (i<=0)
            {
                continue;
            }
            if ( !trees.ContainsKey(i))
            {
                List<LabDic> dics = new List<LabDic>();
                trees[i] = dics;
            }
            trees[i].Add(item);
        }
        labTreeDictionary = trees;

        //获取每个页面获得位置信息
        Dictionary<int, int[,]> positions =new Dictionary<int, int[,]>();
        foreach (KeyValuePair<int, List<LabDic>> tree in trees)
        {
            //计算出树的宽度
            int width = 0;
            List<int> branchs = new List<int>();
            foreach (LabDic item in tree.Value)
            {
                if (branchs.Contains( item.ID / 100))
                {
                    continue;
                }
                else
                {
                    branchs.Add(item.ID / 100);
                }
            }
            width = branchs.Count;
            //创建位置矩阵
            int[,] pos = new int[width, levelQuantity];
            //遍历填充
            foreach (LabDic item in tree.Value)
            {
                pos[branchs.IndexOf(item.ID / 100),item.Rank-1] = item.ID;
            }
            //赋值
            positions[tree.Key] = pos;
        }
        
        return positions;
    }


    /// <summary>
    /// 获取科技树
    /// </summary>
    private List<LabDic> GetLabDics()
    {
        List<LabDic> dicList = Data.GetJData<LabDic>("Lab", "&Dic").contents;
        //foreach (LabDic func in funcList)
        //{
        //    func.Init();
        //}
        return dicList;
    }

    /// <summary>
    /// 获取科技树字典
    /// </summary>
    /// <returns></returns>
    private Dictionary<int, LabDic> GetLabDicDictionary()
    {
        Dictionary<int, LabDic> dic = new Dictionary<int, LabDic>();
        foreach (LabDic item in labDics)
        {
            dic.Add(item.ID, item);
        }
        return dic;
    }

    /// <summary>
    /// 获取阶位总数
    /// </summary>
    /// <returns></returns>
    private int GetLevelQuantity()
    {
        int max = 0;
        foreach (LabDic item in labDics)
        {
            max = item.Rank > max ? item.Rank : max;
        }
        return max;
    }

    /// <summary>
    /// 获取倒计时结束的科技
    /// </summary>
    public List<int> GetFinishedUnconfirmTechs()
    {

        Json.Log(Static.Player.timer);
        List<int> list = new List<int>();
        if (Static.Player.timer.ContainsKey(1))
        {
            if (Static.Player.timer[1].ContainsKey(255))
            {
                foreach (KeyValuePair<int, long> item in Static.Player.timer[1][255])
                {
                    LabDic tech = labDicDictionary[item.Key];
                    tech.SetUnlockStartTime(item.Value);

                    //
                    if (tech.IsUnlockFinishedUnconfirm() && !list.Contains(item.Key))
                    {
                        list.Add(item.Key);
                    }
                }
            }
        }
        return list;
    }


    /// <summary>
    /// 是否可操作 显示小红点
    /// </summary>
    /// <returns></returns>
    public bool ShowRedTips()
    {
        int rank = RankFameManager.Instance.GetRankLevelByFame(Static.Player.Honor);
        //如果有阶位可以解锁
        if (GetMinLockedLevel()<=rank)
        {
            return true;
        }
        //如果有科技倒计时结束
        else if (GetFinishedUnconfirmTechs().Count>0)
        {
            return true;
        }
        return false;
    }
    

    /// <summary>
    /// 获取英雄数量上限
    /// </summary>
    /// <returns></returns>
    public int GetHeroQuantityLimit()
    {
        //更新数据
        UpdateUnlockMsg();

        int num = 3;
        for (int i = 1; i <= levelQuantity; i++)
        {
            if (labDicDictionary.ContainsKey(2100 + i)&& labDicDictionary[2100 + i].Unlocked)
            {
                num = labDicDictionary[2100 + i].Value;
            }
        }
        return num;
    }

    
}
