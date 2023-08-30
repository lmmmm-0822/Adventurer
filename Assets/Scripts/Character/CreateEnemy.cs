using Chronos;
using QxFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class CreateEnemy : MonoBehaviour
{
    [System.Serializable]
    public struct EnemyWeight
    {
        public EnemyType enmeyType;//我知道enemy打错了，但是不要改，不然现在已有的携带CreateEnemy组件的enemies的enemyType都会丢失
        public float weight;
    }
    public List<EnemyWeight> enemies;

    public bool random;
    [ShowIf("random")] public int minTime;
    [ShowIf("random")] public int maxTime;
    [HideIf("random")] public int certainTime;

    public bool point;
    [HideIf("point")] public Vector2 size;

    public bool randomFacing;
    [HideIf("randomFacing")] public bool facingToRight;

    public int maxCount;
    public int initCount;

    public bool limitedByTime;
    [ShowIf("limitedByTime")] public int early;
    [ShowIf("limitedByTime")] public int late;

    //所有数据从CharacterManager中获取
    //private bool init = false;
    //private int lastTime;
    //private int disableTime;
    //private List<EnemyType> nextCreate = new List<EnemyType>(4);
    public void Enable(CharacterData.CreateEnemyData data, List<EnemyType> nextCreate, List<CharacterData.EnemyEntityData> entityDatas)
    {
        if (!InPeriod())
        {
            gameObject.SetActive(false, true);
            return;
        }

        gameObject.SetActive(true, true);
        SimulateCreate(data, nextCreate);
        CreateEnemys(nextCreate, entityDatas);
    }
    public void Disable(ref int disableTime)
    {
        if (!gameObject.activeSelf)
            return;

        disableTime = GameMgr.TimeMgr.GetNow().TotalMinutes;
        foreach (Transform enemy in transform)
        {
            GameMgr.CharacterMgr.RemoveCharacter(enemy.GetComponent<CharacterBase>(), false);
        }
    }
    private bool InPeriod()
    {
        return !limitedByTime || Utils.InPeriod(early, late, GameMgr.TimeMgr.GetNow().TodayMinutes);
    }
    private void CreateEnemys(List<EnemyType> nextCreate, List<CharacterData.EnemyEntityData> entityDatas)
    {
        List<EnemyType> current = new List<EnemyType>(transform.childCount);
        foreach (Transform tran in transform)
        {//获取当前已有的enemy
            var enemy = tran.GetComponent<Enemy>();
            current.Add(enemy.enemyType);
            GameMgr.CharacterMgr.AddCharacter(enemy, false);
        }

        foreach (var enemyType in nextCreate)
        {//以nextCreate为基准，补充当前没有的enemy
            if (current.Remove(enemyType))
                continue;//如果enemyType存在于当前的enemy中，则跳过此次生成，并将enemyType从current中移除

            CharacterData.EnemyEntityData data = null;
            if (entityDatas != null)
            {
                int dataId = entityDatas.FindIndex((data) => data.type == enemyType);
                if (dataId != -1)
                {//取出entityDatas的数据
                    data = entityDatas[dataId];
                    entityDatas.RemoveAt(dataId);//因为entityDatas只是存档的数据，之后没有使用，而且存档前也会重新赋值，所以这里可以修改
                }
            }
            var realPos = point ? Vector2.zero : new Vector2(Random.Range(-size.x, size.x), Random.Range(-size.y, size.y));
            if (GameMgr.CharacterMgr.InstantiateEnemy(enemyType, realPos, out var enemy, transform, data))
            {
                enemy.ChangeFacing(randomFacing ? Random.Range(0, 2) == 0 : facingToRight);
            }
        }
    }
    public static void SimulateCreate(CharacterData.CreateEnemyData data, List<EnemyType> nextCreate)
    {//模拟生成，但不会实际生成，而是下次可以生成时再生成，主要是获取其他场景的怪物数量时需要模拟生成一下
     //不用判断是否在时间范围内，因为不是真的出现在场景中

        if (data.disableTime == 0)
        {//从来都没有经过这个场景，使用initCount
            int count = nextCreate.Count;
            while (count < data.initCount)
            {
                ++count;
                nextCreate.Add(data.enemies[0].enmeyType);
            }
            data.lastTime = data.randomTime ? Random.Range(data.minTime, data.maxTime) : data.certainTime;
            return;
        }

        //设置生成判断时间（小于0代表可以生成
        var now = GameMgr.TimeMgr.GetNow().TotalMinutes;
        data.lastTime -= now - data.disableTime;//因为在场景中不计时，所以减去的是disableTime

        //模拟生成
        int cnt = nextCreate.Count;
        while (data.lastTime <= 0 && cnt < data.maxCount)
        {
            ++cnt;
            data.lastTime += data.randomTime ? Random.Range(data.minTime, data.maxTime) : data.certainTime;
            nextCreate.Add(data.enemies[0].enmeyType);
        }

        //设置时间
        if (data.lastTime <= 0)//达到限制数量，但是时间上还是可以生成
            data.lastTime = data.randomTime ? Random.Range(data.minTime, data.maxTime) : data.certainTime;//强制换为不可以生成的时间
        data.disableTime = now;
    }
}
