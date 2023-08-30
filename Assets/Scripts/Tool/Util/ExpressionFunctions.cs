using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ExpressionFunctions
{
    //public static DefaultContext defaultContext = new DefaultContext();
    //[Collect]
    //public bool ResumeHealth(int[] param, DefaultContext context)
    //{
    //    GameMgr.CharacterMgr.Character.cAtr.CurrentHealth += param[0];
    //    return true;
    //}
    //[Collect]
    //public bool ResumeFocus(int[] param, DefaultContext context)
    //{
    //    GameMgr.CharacterMgr.Character.cAtr.CurrentFocus += param[0];
    //    return true;
    //}

    //[Collect]
    //public bool SetBuff(int[] param,DefaultContext context)
    //{
    //    GameMgr.CharacterMgr.Character.AddBuff(param[0]);
    //    return true;
    //}

    public abstract class Context { }
    public class DefaultContext { }

    //public struct EquipmentType
    //{
    //    public int type;
    //}
}
