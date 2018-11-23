using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LabLevel : MonoBehaviour {

    private bool inited=false;
    private List<LabItem> items;
    private GameObject inside;
    private GameObject outside;
    private GameObject show;//需要显示的内容
    private Image flagLevelImage;//旗帜上等级图片
    private Image flagImage;//旗帜上等级图片
    private Text name;
    private Text coin;
    private Button btn_unlock;//解锁按钮
    private int level;//阶位
    private Text unlockCondition;//解锁条件

    private void Init()
    {
        inside = transform.Find("Inside").gameObject;
        outside = transform.Find("Outside").gameObject;
        name = transform.Find("Name").GetComponent<Text>();
        coin = outside.transform.Find("RightGround/CoinShow/Text").GetComponent<Text>();
        btn_unlock = outside.transform.Find("RightGround/Button").GetComponent<Button>();
        items =new List<LabItem>( transform.Find("Inside/Content").GetComponentsInChildren<LabItem>());
        flagLevelImage = transform.Find("Flag/Level").GetComponent<Image>();
        flagImage = transform.Find("Flag").GetComponent<Image>();
        unlockCondition = outside.transform.Find("UnlockCondition").GetComponent<Text>();
        inited = true;
    }

    /// <summary>
    /// 获得所有item
    /// </summary>
    /// <returns></returns>
    public List<LabItem> GetItems()
    {
        return items;
    }

    /// <summary>
    /// 设置解锁按钮是否可点击
    /// </summary>
    /// <param name="canUnlock"></param>
    public void SetUnlockAble(bool canUnlock)
    {
       
        //解锁按钮
        btn_unlock.onClick.RemoveAllListeners();
        btn_unlock.onClick.AddListener(ClickUnlockLevel);
        btn_unlock.transform.Find("Mask").gameObject.SetActive(!canUnlock);
        btn_unlock.interactable = canUnlock;
    }

    /// <summary>
    /// 点击解锁阶位
    /// </summary>
    private void ClickUnlockLevel()
    {
        //flagLevelImage.color =  new Color(1, 1, 1, 1) ;
        //flagImage.color = new Color(1, 1, 1, 1);
        //outside.gameObject.SetActive(false);
        LabPanel.Instance.ClickUnlockLevel(level);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="level">阶位</param>
    /// <param name="locked">是否锁着</param>
    /// <param name="xulie">四个item的ID</param>
    public void Setcontent(int level,bool unlocked,int[] xulie)
    {
        //初始化
        if (!inited)
        {
            Init();
        }

        this.level = level;
        

        //如果还没解锁
            outside.SetActive(!unlocked);
        

        //设置解锁金币
        coin.text = LabUtil.Instance.labDicDictionary[level].CostCoin.ToString();

        //设置旗帜
        flagLevelImage.sprite = AssetBundleManager.Instance.LoadAsset("CommonAssets", "Common", "renown"+level) as Sprite;
        flagLevelImage.color =unlocked? new Color(1, 1, 1, 1):new Color(0.5f, 0.5f, 0.5f);
        flagImage.color =unlocked? new Color(1, 1, 1, 1):new Color(0.5f, 0.5f, 0.5f);

        //设置右上角文字
        name.text =Multilingual.GetLanguage("Lab_Tech_LabLevel_"+level+"_Name");
        string[] str = Multilingual.GetLanguage("Lab_Unlockcondition").Split('|');
        //解锁条件
        unlockCondition.text = str[0] +level+ str[1];
        unlockCondition.color=RankFameManager.Instance.CurrentRank>=level ? unlockCondition.color = new Color(0.8f, 1, 0):  new Color(1, 0.305f, 0);

        //设置item
        for (int i = 0; i < 4; i++)
        {
            //如果等于0 隐藏内容
            if (xulie[i] == 0)
            {
                items[i].gameObject.SetActive(true);
                items[i].transform.Find("Show").gameObject.SetActive(false);
                items[i].GetComponent<Button>().interactable = false;
            }
            //如果等于-1 隐藏gameobject
            else if (xulie[i] == -1)
            {
                items[i].gameObject.SetActive(false);
            }
            //赋值 并初始化
            else if (xulie[i] > 0)
            {
                items[i].gameObject.SetActive(true);
                items[i].GetComponent<Button>().interactable = true;
                items[i].transform.Find("Show").gameObject.SetActive(true);
                items[i].Setcontent(xulie[i]);
            }
        }

       
    }



    /// <summary>
    /// 播放特效
    /// </summary>
    public void PlayerEffect()
    {
        CommonEffect effect = Pool.GetGameObject("UI", "MainUIScene", "Lab_Unlock_Research_eff", Vector3.one, Quaternion.identity).GetComponent<CommonEffect>();
        effect.transform.SetParent(transform);
        effect.transform.localScale = Vector3.one;
        effect.transform.localPosition = Vector3.zero;
        effect.Play(transform, 2f);
    }


}
