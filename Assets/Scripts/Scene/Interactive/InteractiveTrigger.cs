using QxFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class InteractiveTrigger : MonoBehaviour,ITimesLimit
{
    public enum InteractiveType
    {
        ChangeScene = 1,
        AddItem = 2,
        Communicate = 3,
        Break = 4,
        //RecycleBuilding = 5,//直接使用AddItem
        Shop = 6,//Todo
        CollectItem = 7,
        TaskBoard = 8,//todo
        Trap = 9,
        AddEffect = 10,
        OnlyTip = 99,
    }
    public InteractiveType type;
    public bool instance;
    [HideIf("instance")] public string displayInfo;
    [HideIf("instance")] public float holdDown = 0;
    [MinValue(0)] public int threshold = 0;
    [HideIf("threshold", Value = 0), Tooltip("为空时隐藏自身")] public GameObject hideTarget;
    public string Name => gameObject.name;
    public int Threshold => threshold;
    public GameObject Target => hideTarget != null ? hideTarget : gameObject;

    public bool Interactive
    {
        get => _canInteractive;
        set
        {
            if (_canInteractive != value)
            {
                transform.localScale = value ? Vector3.one : new Vector3(0.5f, 0.5f, 1);
                _canInteractive = value;
            }
        }
    }
    private bool _canInteractive = true;
    private void Start()
    {
        if (threshold != 0 && !GameMgr.SceneMgr.CheckTimesLimit(this))
            Target.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if(instance)
        {
            if (_canInteractive)
            {
                GameMgr.SceneMgr.ExecuteTriggerAction(this);
            }
            return;
        }

        if (!_canInteractive)//不能交互则延迟半秒再注册
            StartCoroutine(DelayRegister());
        else
            GameMgr.SceneMgr.RegistInteractiveTrigger(transform);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") || instance) return;

        StopAllCoroutines();//退出则取消延迟注册
        GameMgr.SceneMgr.UnRegistInteractiveTrigger(transform);
    }
    private IEnumerator DelayRegister()
    {
        if (!TryGetComponent<CircleCollider2D>(out _)) yield break;

        yield return Utils.waitHalfSecond;
        GameMgr.SceneMgr.RegistInteractiveTrigger(transform);
        yield break;
    }

    public void Trigger()
    {
        if (threshold != 0)
            GameMgr.SceneMgr.TimesLimitTrigger(this);
    }
}
