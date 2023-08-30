using QxFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieBase_Enemy : State_Enemy
{
    public float existTime = 3f;
    protected float lastExistTime;
    private List<(int id, int count)> drop = new List<(int id, int count)>(4);

    public override void OnEnterState(StateBase lastState, float value = 0, object args = null)
    {
        MessageManager.Instance.Get<Enemy.EnemyMsg>().DispatchMessage(Enemy.EnemyMsg.Die, enemy);
        enemy.gameObject.layer = Utils.NameToLayer(NameList.Layer.EnemyIgnoreOthers);
        SetAnimation();
        lastExistTime = existTime;
        //enemy.rb2D.gravityScale = 1;
    }
    public override void OnUpdate(float deltaTime)
    {
        lastExistTime -= deltaTime;
        if (lastExistTime <= 0)
        {
            Drop();
            GameMgr.CharacterMgr.RemoveCharacter(enemy);
        }
    }
    public override void OnFixedUpdate(float deltaTime)
    {
        enemy.Stopping(0.1f);
    }
    protected virtual void SetAnimation()
    {
        PlayAnimation("Die");
    }
    protected void Drop()
    {
        if (enemy.drop == null)
            return;
        int i;
        foreach (var (itemId, probability) in enemy.drop)
        {
            if (Random.Range(0, 1f) <= probability)
            {
                i = drop.FindIndex((t) => { return t.id == itemId; });
                if (i != -1)
                    drop[i] = (itemId, drop[i].count + 1);
                else
                    drop.Add((itemId, 1));
            }
        }
        foreach (var (id, count) in drop)
        {
            var go = ResourceManager.Instance.Instantiate("Prefabs/Scene/Effect",GameMgr.SceneMgr.CurrentSceneAnchorPoint);
            go.transform.position = enemy.transform.position + new Vector3(Random.Range(-0.3f, 0.3f), 0, 0);
            go.GetComponentInChildren<AddEffectInteractive>().SetEffect(id);
        }
        drop.Clear();
        GameMgr.CharacterMgr.Character.gold += Data.Instance.TableAgent.GetInt("Enemy", enemy.enemyType.ToString(), "Gold");
    }
}

