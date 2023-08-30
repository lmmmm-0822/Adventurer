using QxFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneItem : MonoBehaviour, ISceneSave
{
    private SceneItemData data;
    public (int, int) PickUp()
    {
        GameMgr.SceneMgr.RemoveObject(GameObjectPath);
        return (data.itemId, data.count);
    }

    public string GameObjectPath
    {
        get
        {
            var tran = transform;
            string res = tran.name;
            var parent = tran.parent;
            while (!parent.CompareTag("SceneAnchorPoint"))
            {
                res = parent.name + "/" + res;
                parent = parent.parent;
            }
            return res;
        }
    }
    public void InitData(SceneSaveData data)
    {
        this.data = (SceneItemData)data;
        if (!this.data.init)
        {
            this.data.init = true;
        }
        //transform.GetComponent<SpriteRenderer>().sprite = ResourceManager.Instance.Load<Sprite>("Textures/Property/" + GameMgr.ItemMgr.GetItemStatus(this.data.itemId).ItemRealImg);
    }
    public void UpdateData() { data.Position = transform.position; }
    SceneSaveData ISceneSave.GetInitSaveData()
    {
        throw new System.NotImplementedException();
    }

    public class SceneItemData : CreatedObjectData
    {
        public bool init;
        public int itemId;
        public int count;
    }
}
