using Chronos;
using QxFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EffectController
{
    [HideInInspector]
    private CharacterBase cB;
    private bool isCharacter;
    public List<Effect> effects;//包括buff和debuff也包括装备带来的属性提升
    public void Init(CharacterBase cB)
    {
        this.cB = cB;
        isCharacter = cB is Character;
        effects = new List<Effect>();
    }
    public void RebuildData(List<Effect.SaveData> effectDatas)
    {
        foreach (var data in effectDatas)
            effects.Add(new Effect(data));
    }
    public void RefreshData()
    {
        var exitTime = GameMgr.SceneMgr.ExitTime(GameMgr.SceneMgr.CurrentScene, true);
        if (exitTime < 0) //如果之前没来过这个场景（实际上是没有离开过这个场景）
            return;//则不刷新buff
        int intervalTime = GameMgr.TimeMgr.GetNow().TotalMinutes - exitTime;
        foreach (var effect in effects)
        {
            if (effect.EffectType == Effect.Type.TimeLimitted && effect.Active)
            {
                effect.LastTime -= intervalTime;//减去离开场景的这段时间
                if (effect.LastTime <= 0)
                {
                    DisableEffect(effect);
                }
            }
            //之后需要加效果执行，比如每隔1秒回复100生命值之类的Todo
        }
    }
    public void AddEffect(string name, float[] args = null, string key = null)
    {
        AddEffect(Effect.GetTemplate(name), args, key);
    }
    public void AddEffect(int num, float[] args = null, string key = null)
    {
        AddEffect(Effect.GetTemplate(num), args, key);
    }
    private void AddEffect(Effect template, float[] args, string key)
    {
        bool isOK = false;
        if (template.EffectType != Effect.Type.Instant)
        {
            if (template.GetFeatureArgs(Effect.Feature.OnlyOne, out _))
            {
                foreach (Effect effect in effects)
                {
                    if (effect.Num == template.Num)
                    {
                        effect.LastTime = template.LastTime;//刷新持续时间
                        isOK = true;
                        return;
                    }
                }
            }
            foreach (var effect in effects)
            {
                if (!effect.Active)//失效的效果
                {
                    effect.SetEffect(template, args, key);
                    EffectExecute(effect, true);
                    isOK = true;
                    break;
                }
            }
            if (!isOK)
            {
                var newEffect = new Effect(template, args, key);
                effects.Add(newEffect);
                EffectExecute(newEffect, true);
            }

        }
        else
        {//立刻执行的效果，不需要添加到effects里面
            Effect.tmpEffect.SetEffect(template, args);
            EffectExecute(Effect.tmpEffect, true);
        }

        CheckRefreshAtr(template);
        if (isCharacter && template.IconPath != null)
            MessageManager.Instance.Get<Character.CharacterMsg>().DispatchMessage(Character.CharacterMsg.ChangeEffect, this);
    }
    public bool HaveEffect(int num,out int index)
    {
        index = effects.FindIndex((e) => e.Num == num);
        return index != -1;
    }
    public void OnUpdate()
    {//Todo，没处理场景切换场景的时间管理
        float deltaTime = GameMgr.TimeMgr.TimeClock.deltaTime;
        foreach (var effect in effects)
        {
            if (effect.EffectType == Effect.Type.TimeLimitted && effect.Active)
            {
                effect.LastTime -= deltaTime;
                if (effect.LastTime <= 0)
                {
                    DisableEffect(effect);
                }
            }
            //之后需要加效果执行，比如每隔1秒回复100生命值之类的Todo
        }
    }
    private void DisableEffect(Effect effect)
    {
        effect.Active = false;
        EffectExecute(effect, false);
        CheckRefreshAtr(effect);
        if (isCharacter && effect.IconPath != null)
            MessageManager.Instance.Get<Character.CharacterMsg>().DispatchMessage(Character.CharacterMsg.ChangeEffect, this);
    }
    public void RemoveEffect(string key)
    {
        if (key == null)
        {
            Debug.LogWarning("不要移除key为null的Effect");
            return;
        }
        foreach(var eff in effects)
        {
            if (eff.Key == key)
                DisableEffect(eff);
        }
    }

    private void CheckRefreshAtr(Effect effect)
    {
        return;//如果改变了战斗属性，则需要刷新
        //foreach (var (result, _) in effect.Results)
        //{
        //    switch (result)
        //    {
        //        case Effect.Result.AllAttribute:
        //            cB.atrCtr.RefreshCAtr();
        //            break;
        //    }
        //}
    }

    private void EffectExecute(Effect effect, bool start)
    {
        int tmp = start ? 1 : -1;
        foreach (var (result, degree) in effect.Results)
        {//对所有的results执行操作
            ExecuteResult(result, degree, tmp);
        }
    }

    public void RefreshCAtrEffect()
    {//此为战斗属性的刷新
        cB.atrCtr.otherCAtr.Reset();
        //foreach (var effect in effects)
        //{
        //    if (effect.Active)
        //    {
        //        foreach (var (result, degree) in effect.Results)
        //        {
        //            switch (result)
        //            {
        //                case Effect.Result.AllAttribute:
        //                    ExecuteResult(result, degree, 1);
        //                    break;
        //            }
        //        }
        //    }
        //}
    }
    private void ExecuteResult(Effect.Result result, float[] degree, float fix)
    {
        switch (result)
        {
            case Effect.Result.Red:
                if (fix > 0)
                {//添加
                    ((Character)cB).bulletRangeRate = degree[0];
                    bool blue = HaveEffect(2, out var blueId);
                    bool green = HaveEffect(3, out var greenId);
                    if (blue && green)
                    {
                        RemoveEffect("cray");
                        effects[blueId].LastTime /= 2;
                        effects[greenId].LastTime /= 2;
                        HaveEffect(1, out var redId);
                        effects[redId].LastTime /= 2;
                    }
                    else if (blue)
                        AddEffect(4, key: "purple");
                    else if (green)
                        AddEffect(6, key: "yellow");
                }
                else
                {//移除
                    if (HaveEffect(1, out _))
                        break;
                    ((Character)cB).bulletRangeRate = 1;
                    bool blue = HaveEffect(2, out _);
                    bool green = HaveEffect(3, out _);
                    if (blue && green)
                        AddEffect(5, key: "cray");
                    else if (blue)
                        RemoveEffect("purple");
                    else if (green)
                        RemoveEffect("yellow");
                }
                break;
            case Effect.Result.Blue:
                if (fix > 0)
                {//添加
                    ((Character)cB).moveSpeedRate *= degree[0];
                    bool red = HaveEffect(1, out var redId);
                    bool green = HaveEffect(3, out var greenId);
                    if (red && green)
                    {
                        RemoveEffect("yellow");
                        effects[redId].LastTime /= 2;
                        effects[greenId].LastTime /= 2;
                        HaveEffect(2, out var blueId);
                        effects[blueId].LastTime /= 2;
                    }
                    else if (red)
                        AddEffect(4, key: "purple");
                    else if (green)
                        AddEffect(5, key: "cray");
                }
                else
                {//移除
                    if (HaveEffect(2, out _))
                        break;
                    ((Character)cB).moveSpeedRate /= degree[0];
                    bool red = HaveEffect(1, out _);
                    bool green = HaveEffect(3, out _);
                    if (red && green)
                        AddEffect(6, key: "yellow");
                    else if (red)
                        RemoveEffect("purple");
                    else if (green)
                        RemoveEffect("cray");
                }
                break;
            case Effect.Result.Green:
                if (fix > 0)
                {//添加
                    ((Character)cB).addJumpHeight = true;
                    bool red = HaveEffect(1, out var redId);
                    bool blue = HaveEffect(2, out var blueId);
                    if (red && blue)
                    {
                        RemoveEffect("purple");
                        effects[redId].LastTime /= 2;
                        effects[blueId].LastTime /= 2;
                        HaveEffect(3, out var greenId);
                        effects[greenId].LastTime /= 2;
                    }
                    else if (red)
                        AddEffect(6, key: "yellow");
                    else if (blue)
                        AddEffect(5, key: "cray");
                }
                else
                {//移除
                    if (HaveEffect(3, out _))
                        break;
                    ((Character)cB).addJumpHeight = false;
                    bool red = HaveEffect(1, out _);
                    bool blue = HaveEffect(2, out _);
                    if (red && blue)
                        AddEffect(4, key: "purple");
                    else if (red)
                        RemoveEffect("yellow");
                    else if (blue)
                        RemoveEffect("cray");
                }
                break;
            case Effect.Result.Purple:
                ((Character)cB).bulletIntervalRate = fix > 0 ? degree[0] : 1;
                break;
            case Effect.Result.Cyan:
                ((Character)cB).stateController.GetState<Jump>(AllStates.Jump).SetDoubleJump(fix > 0);
                break;
            case Effect.Result.Yellow:
                ((Character)cB).addBulletCnt = fix > 0;
                break;
            case Effect.Result.Shield:
                ((Character)cB).haveShield = fix > 0;
                break;
            case Effect.Result.Reset:
                if (HaveEffect(1, out var r))
                    effects[r].LastTime += 60;
                if (HaveEffect(2, out var b))
                    effects[b].LastTime += 60;
                if (HaveEffect(3, out var g))
                    effects[g].LastTime += 60;
                break;
            case Effect.Result.Slow:
                ((Character)cB).moveSpeedRate *= fix > 0 ? degree[0] : (1 / degree[0]);
                break;
        }
    }
}
[System.Serializable]
public class Effect
{
    public enum Type
    {
        Instant = 0,//瞬间生效
        TimeLimitted = 1,//有持续时间
        Continue = 2,//一直生效直到去掉装备/脱离某种环境/...
    }
    public enum Feature
    {                           //              只与effect自身相关
        None = 0,               //     特性描述                  附带参数（逗号间隔
        OnlyOne = 1,            // 同时只存在一个（重复触发刷新时间
    }
    public enum Result
    {                           //              （*代表只对玩家生效）
        None = 0,               //     效果描述                  附带参数（逗号间隔
        Red = 1,                // 增加子弹距离。。。。。。。。。倍率
        Blue = 2,               // 增加移动速度。。。。。。。。。倍率
        Green = 3,              // 增加跳跃高度
        Purple = 4,             // 减小子弹攻击间隔。。。。。。。倍率
        Cyan = 5,               // 获得二段跳
        Yellow = 6,             // 增加发射子弹的数量
        Shield = 7,             // 增加护盾
        Reset = 8,              // 刷新buff持续时间
        Slow = 9,               // 设置移动速度
    }
    public static Effect tmpEffect = new Effect();
    private static Dictionary<int, Effect> allEffects;
    static Effect()
    {
        var _tableAgent = Data.Instance.TableAgent;
        var allNums = Data.Instance.TableAgent.CollectKey1("Effect");
        allEffects = new Dictionary<int, Effect>(allNums.Count);
        foreach (var num in allNums)
        {
            Effect effect = new Effect
            {
                Num = int.Parse(num),
                Name = _tableAgent.GetString("Effect", num, "Name"),
                //EffectType = (Type)_tableAgent.GetInt("Effect", num, "Type"),
                Describe = _tableAgent.GetString("Effect", num, "Describe"),
                IconPath = _tableAgent.GetString("Effect", num, "Icon"),
                LastTime = _tableAgent.GetFloat("Effect", num, "Time"),
            };
            if (effect.IconPath == "")
                effect.IconPath = null;
            string[] temp = _tableAgent.GetStrings("Effect", num, "Features");
            if (temp[0] != "")
            {
                int t_len = temp.Length;
                effect.Features = new List<(Feature feature, float[] degree)>(t_len);
                for (int j = 0; j < t_len; j++)
                {
                    try
                    {
                        string[] feature = temp[j].Split(new char[] { ':', '+' });
                        int f_len = feature.Length;
                        if (f_len == 1)
                        {//该特性无参数
                            effect.Features.Add(((Feature)int.Parse(feature[0]), null));
                        }
                        else
                        {
                            float[] args = new float[f_len - 1];
                            for (int k = 1; k < f_len; k++)
                            {//将string数组转成float数组，获取特性参数
                                args[k - 1] = float.Parse(feature[k]);
                            }
                            effect.Features.Add(((Feature)int.Parse(feature[0]), args));
                        }
                    }
                    catch
                    {
                        Debug.LogError("从Effect读取特性出错");
                    }
                }
            }
            temp = _tableAgent.GetStrings("Effect", num, "Results");
            if (temp[0] != "")
            {
                int t_len = temp.Length;
                effect.Results = new List<(Result result, float[] args)>(t_len);
                for (int j = 0; j < t_len; j++)
                {
                    try
                    {
                        string[] result = temp[j].Split(':', '+');
                        int r_len = result.Length;
                        if (r_len == 1)
                        {//该特性无参数
                            effect.Results.Add(((Result)int.Parse(result[0]), null));
                        }
                        else
                        {
                            float[] args = new float[r_len - 1];
                            for (int k = 1; k < r_len; k++)
                            {//将string数组转成float数组，获取特性参数
                                args[k - 1] = float.Parse(result[k]);
                            }
                            effect.Results.Add(((Result)int.Parse(result[0]), args));
                        }
                    }
                    catch
                    {
                        Debug.LogError("从Effect读取效果出错");
                    }
                }
            }
            allEffects.Add(int.Parse(num), effect);
        }
    }
    public static Effect GetTemplate(int num)
    {
        try { return allEffects[num]; }
        catch
        {
            Debug.LogError("没有编号为" + num.ToString() + "的Effect");
            return null;
        }
    }
    public static Effect GetTemplate(string name)
    {
        foreach (var effect in allEffects.Values)
        {
            if (effect.Name == name)
                return effect;
        }
        Debug.LogError("没有名称为" + name + "的Effect");
        return null;
    }
    private Effect() { }

    public string Key { get; private set; }
    public int Num { get; private set; }
    public string Name { get; private set; }
    public Type EffectType
    {
        get => LastTime switch
        {
            0 => Type.Instant,
            -1 => Type.Continue,
            _ => Type.TimeLimitted
        };
    }
    public string Describe { get; private set; }
    public string IconPath { get; private set; }
    public List<(Feature feature, float[] degree)> Features { get; private set; }
    public List<(Result result, float[] args)> Results { get; private set; }
    public bool GetFeatureArgs(Feature feature, out float[] degree)
    {
        degree = null;
        if (Features == null) return false;
        for (int i = 0, cnt = Features.Count; i < cnt; i++)
        {
            if (Features[i].feature == feature)
            {
                degree = Features[i].degree;
                return true;
            }
        }
        return false;
    }
    public bool GetResultArgs(Result result, out float[] args)
    {
        args = null;
        if (Results == null) return false;
        for (int i = 0, cnt = Results.Count; i < cnt; i++)
        {
            if (Results[i].result == result)
            {
                args = Results[i].args;
                return true;
            }
        }
        return false;
    }

    public float LastTime { get; set; }
    public bool Active
    {
        get => Num != -1;
        set
        {
            if (!value)
                Num = -1;
        }
    }

    public Effect(SaveData data)
    {
        Key = data.key;
        Num = data.num;
        Features = data.features;
        Results = data.results;
        LastTime = data.lastTime;
    }
    public Effect(Effect template, float[] resultArgs, string key)
    {
        SetEffect(template, resultArgs, key);
    }
    public void SetEffect(Effect template, float[] resultArgs, string key = null)
    {
        if (template.EffectType == Type.Continue && key == null)
            Debug.LogWarning("添加条件结束效果 " + template.Name + " 时没有设置ID");
        Key = key;
        Name = template.Name;
        Num = template.Num;
        Describe = template.Describe;
        IconPath = template.IconPath;
        //特性可以为null，但是效果不行
        Features = template.Features == null ? null : new List<(Feature feature, float[] degree)>(template.Features);
        Results = new List<(Result result, float[] args)>(template.Results);//Todo之后可能需要减少GC
        if (resultArgs != null)
        {//目前只是单效果
            for (int i = 0; i < resultArgs.Length; i++)
                Results[0].args[i] = resultArgs[i];
        }
        LastTime = template.LastTime;
    }
    public class SaveData
    {
        public SaveData() { }
        public SaveData(Effect effect)
        {
            key = effect.Key;
            num = effect.Num;
            features = effect.Features;
            results = effect.Results;
            lastTime = effect.LastTime;
        }
        public string key;
        public int num;
        public List<(Feature feature, float[] degree)> features;
        public List<(Result result, float[] args)> results;
        public float lastTime;
    }
}