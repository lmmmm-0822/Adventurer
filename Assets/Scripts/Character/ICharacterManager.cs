using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterManager
{
    Character Character { get; }
    void PreCreateEnemy(string scene, int creatorIndex, EnemyType type, int count = 1);
    bool InstantiateEnemy(EnemyType type, Vector3 localPosition, out Enemy enemy, Transform parent = null, CharacterData.EnemyEntityData entityData = null);
    void AddCharacter(CharacterBase c, bool create, CharacterData.EntityData entityData = null);
    void RemoveCharacter(CharacterBase c,bool destory = true);
    void PlayerDieRepeat();
    List<List<EnemyType>> GetSceneEnemyInfo(string scene);
}
