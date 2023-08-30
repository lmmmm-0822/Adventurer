using System.Collections.Generic;
using QxFramework.Utilities;
using UnityEngine;
using System.Net;
using System;

namespace QxFramework.Core
{
    public enum UIMsg
    {
        UpDateWareHouse
    }
    public enum CourseUI
    {
        CloseUI,
    }
    public enum HintType
    {
        CommonHint,
        ClickWindow,
        ChooseWindow,
    }

    /// <summary>
    /// 界面管理，提供管理界面和界面组的功能，如显示隐藏界面、激活界面。
    /// </summary>
    public class UIManager : MonoSingleton<UIManager>
    {
        /// <summary>
        /// UI预设目录，位于Assets/Resource/Prefabs/UI/
        /// </summary>
        public readonly string FoldPath = "Prefabs/UI/";

        /// <summary>
        /// 子面板对象列表
        /// </summary>
        private readonly List<Transform> _panels = new List<Transform>();

        //TODO 要改成可以存在同种的多个于场上
        /// <summary>
        /// 打开的UI列表
        /// </summary>
        private readonly List<KeyValuePair<string, UIBase>> _openUI = new List<KeyValuePair<string, UIBase>>();


        /// <summary>
        /// 检测ui是否开启
        /// </summary>
        /// <returns></returns>
        public bool IsOpen(NameList.UI uiName)
        {
            for (int i = _openUI.Count - 1; i >= 0; i--)
            {
                if (_openUI[i].Key == uiName.ToString())
                {
                    return true;
                }
            }
            return false;
        }

        public bool GetOpenUI(NameList.UI uiName, out UIBase ui)
        {
            return GetOpenUI(uiName.ToString(), out ui);
        }

        /// <summary>
        ///  打开一个UI。
        /// </summary>
        /// <param name="uiName">UI预设的名称。</param>
        /// <param name="name">名称</param>
        /// <param name="args">附带的参数。</param>
        /// <returns></returns>
        public UIBase Open(string uiName, string name = "", object args = null,bool onlyOne = true)
        {
            if (onlyOne && GetOpenUI(uiName, out UIBase tmp))
            {
                tmp.DoReOpen(args);
                return tmp;
            }
            //实例化UI

            GameObject ui = ResourceManager.Instance.Instantiate(FoldPath + uiName, transform);

            if (name != "")
            {
                ui.name = name;
            }
            UIBase uiBase = ui.GetComponent<UIBase>();
            //添加到UI键值对列表
            int i = 0,cnt = _openUI.Count;
            for (; i < cnt; i++)
            {
                if (_openUI[i].Key == null)
                {
                    _openUI[i] = new KeyValuePair<string, UIBase>(uiName, uiBase);
                    break;
                }
            }
            if (i == cnt)
                _openUI.Add(new KeyValuePair<string, UIBase>(uiName, uiBase));

            //启动参数
            if (uiBase == null)
            {
                Debug.LogError("[UIManager]" + ui.name + "未挂载继承UIBase的脚本");
            }
            else
            {
                //通过脚本覆盖掉执行的层级

                ui.transform.SetParent(_panels[uiBase.UILayer]);
                if (!uiBase.Init)
                    uiBase.DoAwake();
                uiBase.DoDisplay(args);
            }

            return uiBase;
        }

        /// <summary>
        ///  关闭指定名称的UI，是从后往前查找。
        /// </summary>
        /// <param name="uiName">UI名称。</param>
        public void Close(string uiName, string objName = "")
        {
            for (int i = _openUI.Count - 1; i >= 0; i--)
            {
                if (_openUI[i].Key == uiName)
                {
                    if (objName != "" && _openUI[i].Value.name != objName)
                    {
                        continue;
                    }
                    if (_openUI[i].Value != null)
                    {
                        var pair = _openUI[i];
                        _openUI[i] = new KeyValuePair<string, UIBase>(null, null);
                        //_openUI.RemoveAt(i);
                        CloseUI(pair.Value);
                        break;
                    }
                    else
                    {
                        Debug.Log("关闭UI时，这个UI并不存在");
                    }
                }
            }
        }

        /// <summary>
        /// 关闭所有UI
        /// </summary>
        public void CloseAll()
        {
            for (int i = _openUI.Count - 1; i >= 0; i--)
            {
                var pair = _openUI[i];
                _openUI[i] = new KeyValuePair<string, UIBase>(null, null);
                //_openUI.RemoveAt(i);
                CloseUI(pair.Value);
                //break;
            }
        }

        /// <summary>
        ///  关闭指定UI对象。
        /// </summary>
        /// <param name="ui">ui对象</param>
        public void Close(UIBase ui/*, string objName = ""*/)
        {
            for (int i = _openUI.Count - 1; i >= 0; i--)
            {
                if (_openUI[i].Value == ui)
                {
                    //if (objName != "" && _openUI[i].Value.name != objName)
                    //{
                    //    continue;
                    //}
                    var pair = _openUI[i];
                    _openUI[i] = new KeyValuePair<string, UIBase>(null, null);
                    //_openUI.RemoveAt(i);
                    CloseUI(pair.Value);
                    break;
                }
            }
        }

        /// <summary>
        /// 进行关闭和销毁处理。
        /// </summary>
        /// <param name="ui">UI对象</param>
        private void CloseUI(UIBase ui)
        {
            if (ui != null)
            {
                ui.DoClose();
                //UIBase本身会Remove
                //MessageManager.Instance.RemoveAbout(ui);
            }
        }
        private bool GetOpenUI(string uiName,out UIBase ui)
        {
            foreach (KeyValuePair<string, UIBase> kvp in _openUI)
            {
                if (kvp.Key == uiName)
                {
                    if (kvp.Value != null)
                    {
                        ui = kvp.Value;
                        return true;
                    }
                }
            }
            ui = null;
            return false;
        }

        [Obsolete("使用IsOpen(NameList.UI uiName)代替")]
        public bool OpenUICheck(string uiName)
        {
            //for (int i = _openUI.Count - 1; i >= 0; i--)
            //{
            //    if (_openUI[i].Key == uiName)
            //    {
            //        //if (uiName != "" && _openUI[i].Value.name.Contains(uiName))
            //        //{
            //            return true;
            //        //}
            //    }
            //}
            return false;
        }
        [Obsolete("使用IsOpen(NameList.UI uiName)替代")]
        public bool FindUI(string uiName, string objName = "")
        {
            //bool IsFind = false;
            //for (int i = _openUI.Count - 1; i >= 0; i--)
            //{
            //    if (_openUI[i].Key == uiName)
            //    {
            //        if (_openUI[i].Value != null)
            //        {
            //            IsFind = true;
            //        }
            //        else
            //        {
            //            // Debug.Log("关闭UI时，这个UI并不存在");
            //        }
            //    }
            //}
            //return IsFind;
            return false;
        }
        [Obsolete("使用GetOpenUI替代")]
        public UIBase GetUI(string uiName)
        {
            //foreach (KeyValuePair<string, UIBase> kvp in _openUI)
            //{
            //    if (kvp.Key == uiName)
            //    {
            //        if (kvp.Value != null)
            //        {
            //            return kvp.Value;
            //        }
            //    }
            //}
            // Debug.Log("没找到这个UI");
            return null;
        }

        // Use this for initialization
        private void Awake()
        {
            //收集所有子对象
            for (int i = 0; i < transform.childCount; i++)
            {
                _panels.Add(transform.GetChild(i));
                //销毁现有的UI预设
                for (int j = _panels[i].childCount - 1; j >= 0; j--)
                {
                    if (!_panels[i].GetChild(j).name.Contains("Debug"))
                    {
                        Destroy(_panels[i].GetChild(j).gameObject);
                    }
                }
            }
        }
        private void Update()
        {
            for(int i = 0, cnt = _openUI.Count; i < cnt; i++)
            {
                if (_openUI[i].Value != null)
                    _openUI[i].Value.DoUpdate();
            }
        }
    }
} 