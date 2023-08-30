using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraMoveArea : MonoBehaviour
{
    public static CameraMoveArea Instance { get; private set; }
    //private Vector2 startPos;
    //private Vector2 target;
    //private float totalTime;
    //private float lastTime;
    //private CinemachineConfiner[] confiners;
    //private float[] originDams;
    private void Awake()
    {
        Instance = this;
        //confiners = transform.parent.GetComponentsInChildren<CinemachineConfiner>();
        //originDams = new float[confiners.Length];
    }
    public void ChangePosition(Vector2 targetPos)//,float time = 0.3f)
    {
        transform.position = targetPos;
        //TimeEventManager.Instance.RegisterTimeAction(0.8f,
        //    () => { for (int i = 0; i < confiners.Length; i++) confiners[i].m_Damping = originDams[i]; },
        //    () => { for (int i = 0; i < confiners.Length; i++) { originDams[i] = confiners[i].m_Damping; confiners[i].m_Damping = 1; } },
        //    TimeEventManager.EventKey.CameraMoveAreaChange);
        //startPos = transform.position;
        //target = targetPos;
        //totalTime = time;
        //lastTime = time;
    }
    //private void Update()
    //{
    //    if (lastTime > 0)
    //    {
    //        lastTime -= Time.deltaTime;
    //        transform.position = Vector2.Lerp(target, startPos, lastTime / totalTime);
    //    }
    //}
}
