using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LabTypeItem : MonoBehaviour {

    private bool inited=false;//初始化
    private GameObject unselected;//未选中
    private GameObject selected;//选中
    private Text name;//名字
    private int id;
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

        unselected = transform.Find("Unselected").gameObject;
        selected = transform.Find("Selected").gameObject;
        name = transform.Find("Text").GetComponent<Text>();

    }

    /// <summary>
    /// 设置是否选中
    /// </summary>
    /// <param name="isChoosen"></param>
    public void SetChoosen(bool isChoosen)
    {
        selected.SetActive(isChoosen);
        unselected.SetActive(!isChoosen);
    }

    /// <summary>
    /// 设置内容
    /// </summary>
    public void SetContent(int id)
    {
        Init();
        this.id = id;
        name.text =Multilingual.GetLanguage("Lab_TechTree_"+id+"_Name");


    }

}
