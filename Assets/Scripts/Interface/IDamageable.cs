using UnityEngine;
public interface IDamageable
{
    bool IsDead { get; }
    bool IsDeadState { get; }
    int JustDamage { get; }
    Transform Damage(DamageAreaBase.DamageArgs args);
    //Transform Damage(int damage, int poiseDamage, Transform beHitArea, Skill skill = null, CharacterBase perpetrator = null);
}
