using System;

namespace NameList
{
    public enum Layer
    {//名字可以与unity中不同，但不建议不同；编号要相同
        Default = 0,
        TransparentFX = 1,
        IgnoreRaycast = 2,
        Water = 4,
        UI = 5,
        Ground = 6,
        Player = 7,
        Enemy = 8,
        DamageArea = 9,
        PlayerIgnoreEnemy = 10,
        EnemyIgnoreOthers = 11,
        SceneInteractive = 12,
        SceneItem = 13,
        DamageableArea = 14,
    }
    public enum UI
    {
        TitleUI = 0,
        MainUI = 1,
        CharacterUI = 2,
        PropUI = 3,
        BagUI = 4,
        ItemMenuUI = 5, 
        ItemBreifUI = 6,
        InteractiveTipUI = 7,
        TipUI = 8,
        EffectExplainUI = 12,
        ShopUI = 13,
        TaskBoardUI = 15,
        DialogUI = 16,
        DialogSampleUI = 17,
        DieUI = 18,
        MenuUI = 19,
        BossUI = 20,
        TrapUI = 21,
        CodeSetUI=22,    }
}
