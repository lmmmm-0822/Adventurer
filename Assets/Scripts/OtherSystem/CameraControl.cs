using Cinemachine;
using QxFramework.Core;
using QxFramework.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoSingleton<CameraControl>
{
    [SerializeField]
    private CinemachineConfiner cinemachineConfiner;
    private Character character;
    //private Transform distantView;

    //private bool changeFollowMode;
    private int currentCamNum;
    private int shakeCamNum;
    private int changeCamNum;
    private List<CinemachineVirtualCamera> vcams = new List<CinemachineVirtualCamera>();
    private List<CinemachineFramingTransposer> bodys = new List<CinemachineFramingTransposer>();
    private List<CinemachineBasicMultiChannelPerlin> noises = new List<CinemachineBasicMultiChannelPerlin>();
    private void Awake()
    {
        GetComponentsInChildren<CinemachineVirtualCamera>(vcams);
        foreach(var t in vcams)
        {
            bodys.Add(t.GetCinemachineComponent<CinemachineFramingTransposer>());
            noises.Add(t.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>());
        }

        //distantView = vcams[0].transform.Find("DistantView");
        //character = GameMgr.CharacterMgr.Character; 

        MessageManager.Instance.Get<Character.CharacterMsg>().RegisterHandler(Character.CharacterMsg.FightStart, (sender, args) => { ChangeLiveCamera("Fight"); });
        MessageManager.Instance.Get<Character.CharacterMsg>().RegisterHandler(Character.CharacterMsg.FightEnd, (sender, args) => { ChangeLiveCamera("Normal"); });
    }
    private void Start()
    {
        character = GameMgr.CharacterMgr.Character;
        foreach (var t in vcams)
        {
            t.m_Follow = character.transform;
        }

        currentCamNum = 0;
        vcams[0].Priority = 100;
    }
    private void OnDestroy()
    {
        MessageManager.Instance.RemoveAbout(this);
    }
    public void ChangeTarget(string camName,Transform newTarget,Vector3 offset = default)
    {        
        changeCamNum = GetCamera(camName);

        vcams[changeCamNum].Follow = newTarget;
        bodys[changeCamNum].m_TrackedObjectOffset = offset;
    }
    public void RestoreTarget()
    {
        vcams[changeCamNum].Follow = character.transform;
        //if (character.DistanceToGround <= 0.5f)
        //{
        //    bodys[changeCamNum].m_TrackedObjectOffset = new Vector3(0, 1, 0);
        //    bodys[changeCamNum].m_YDamping = 1f;
        //}
        //else
        //{
        //    bodys[changeCamNum].m_TrackedObjectOffset = new Vector3(0, 0, 0);
        //    bodys[changeCamNum].m_YDamping = 5f;
        //}
    }
    public void ChangeLiveCamera(string name)
    {
        vcams[currentCamNum].Priority = 10;
        currentCamNum = GetCamera(name);
        //distantView.parent = vcams[currentCamNum].transform;
        //if (name == "Fight")
        //    distantView.transform.localPosition = Vector3.zero;
        vcams[currentCamNum].Priority = 11;
    }
    public void Shake(float amplitude, float time)
    {
        if (time <= 0 || noises[currentCamNum] == null) return;

        noises[currentCamNum].m_AmplitudeGain = amplitude;
        TimeEventManager.Instance.RegisterTimeAction(time,
            () => { noises[shakeCamNum].m_FrequencyGain = 0; },
            () => { noises[currentCamNum].m_FrequencyGain = 1; shakeCamNum = currentCamNum; },
            TimeEventManager.EventKey.ScreenShake);
    }
    private int GetCamera(string name)
    {
        for (int i = 0; i < vcams.Count; i++)
        {
            if (vcams[i].name == name)
            {
                return i;
            }
        }
        Debug.LogWarning("不包含名称为" + name + "的子摄像机");
        return 0;
    }
    public void ChangeMoveArea(Collider2D co)
    {
        cinemachineConfiner.m_BoundingShape2D = co;
    }
}
