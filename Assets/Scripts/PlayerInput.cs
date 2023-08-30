using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QxFramework.Utilities;
using QxFramework.Core;
//using XInputDotNetPure;

public enum CharacterInput
{
    moveLeft = 0,
    moveRight = 1,
    up = 2,
    down = 3,//前4个不要动，输入序列会记录前4个
    dodge,
    jump,
    run,
    defend,
    attack1,
    attack2,
    interactive1 = 11,//交互按键
    interactive2 = 12,//交互按键
    interactive3 = 13,//交互按键
    drawSword,//拔刀、收刀
    useProp,
    prop1,
    prop2,
    prop3,
    prop4,
    exit,
    End,//只用来表示枚举结束，不设置键位
}
public class PlayerInput : Singleton<PlayerInput>, ISystemModule
{
    public bool IsAuto { get; set; }//是否不接收玩家的输入（系统控制主角行动，而不是玩家
    private List<KeyCode> control;
    private List<float> preInputs;
    public float PreInputs(CharacterInput input) { return !IsAuto ? preInputs[(int)input] : 233f; }
    //public bool LastDownMoveRight { get => preInputs[(int)CharacterInput.moveLeft] > preInputs[(int)CharacterInput.moveRight]; }
    
    /// <summary>
    /// 0是没有按←→键，1是只按→键或者后按的→键，-1是←键
    /// </summary>
    public int GetMoveKey 
    {
        get
        {
            if (GetKey(CharacterInput.moveLeft) && GetKey(CharacterInput.moveRight))
                return preInputs[(int)CharacterInput.moveLeft] < preInputs[(int)CharacterInput.moveRight] ? -1 : 1;
            else if (GetKey(CharacterInput.moveRight))
                return 1;
            else if (GetKey(CharacterInput.moveLeft))
                return -1;
            else
                return 0;
        }
    }
    public int GetPropKeyDown
    {
        get
        {
            for(int i = (int)CharacterInput.prop1; i <= (int)CharacterInput.prop4; i++)
            {
                if (preInputs[i] == 0)
                    return i - (int)CharacterInput.prop1 + 1;
            }
            return -1;
        }
    }
    public int GetInteractiveKey(int interactiveButton)
    {
        int tmp = interactiveButton switch
        {
            0 => 0,
            1 => GetKey(CharacterInput.interactive1) ? 1 : 0,
            2 => GetKey(CharacterInput.interactive2) ? 2 : 0,
            4 => GetKey(CharacterInput.interactive2) ? 4 : 0,
            3 => (GetKey(CharacterInput.interactive1) ? 1 : 0) | (GetKey(CharacterInput.interactive2) ? 2 : 0),
            5 => (GetKey(CharacterInput.interactive1) ? 1 : 0) | (GetKey(CharacterInput.interactive3) ? 4 : 0),
            6 => (GetKey(CharacterInput.interactive2) ? 2 : 0) | (GetKey(CharacterInput.interactive3) ? 4 : 0),
            7 => (GetKey(CharacterInput.interactive1) ? 1 : 0) | (GetKey(CharacterInput.interactive2) ? 2 : 0) | (GetKey(CharacterInput.interactive3) ? 4 : 0),
            _ => throw new System.NotImplementedException(),
        };
        switch (tmp)
        {
            case 0: return 0;
            case 1: return 1;
            case 2: return 2;
            case 4: return 3;
            case 3: return preInputs[(int)CharacterInput.interactive2] < preInputs[(int)CharacterInput.interactive1] ? 2 : 1;
            case 5: return preInputs[(int)CharacterInput.interactive3] < preInputs[(int)CharacterInput.interactive1] ? 3 : 1;
            case 6: return preInputs[(int)CharacterInput.interactive3] < preInputs[(int)CharacterInput.interactive2] ? 3 : 2;
            case 7:
                return preInputs[(int)CharacterInput.interactive1] <= preInputs[(int)CharacterInput.interactive2]
                       ? (preInputs[(int)CharacterInput.interactive1] <= preInputs[(int)CharacterInput.interactive3] ? 1 : 3)
                       : (preInputs[(int)CharacterInput.interactive2] <= preInputs[(int)CharacterInput.interactive3] ? 2 : 3);
            default: return 0;

        }
    }
    public override void Initialize()
    {
        base.Initialize();
        control = new List<KeyCode>((int)CharacterInput.End);
        preInputs = new List<float>((int)CharacterInput.End);
        for (int i = 0; i < (int)CharacterInput.End; i++)
        {
            control.Add(KeyCode.None);
            preInputs.Add(10f);
        }
        SetDefaultKey();
    }
    public void SetDefaultKey()
    {
        control[(int)CharacterInput.moveLeft] = KeyCode.A;
        control[(int)CharacterInput.moveRight] = KeyCode.D;
        control[(int)CharacterInput.up] = KeyCode.W;
        control[(int)CharacterInput.down] = KeyCode.S;
#if UNITY_EDITOR
        control[(int)CharacterInput.dodge] = KeyCode.X;//LeftAlt;
#else
        control[(int)CharacterInput.dodge] = KeyCode.LeftAlt;
#endif
        control[(int)CharacterInput.jump] = KeyCode.Space;
        control[(int)CharacterInput.run] = KeyCode.LeftShift;
        control[(int)CharacterInput.defend] = KeyCode.I;
        control[(int)CharacterInput.attack1] = KeyCode.J;
        control[(int)CharacterInput.attack2] = KeyCode.K;
        control[(int)CharacterInput.interactive1] = KeyCode.F;
        control[(int)CharacterInput.interactive2] = KeyCode.E;
        control[(int)CharacterInput.interactive3] = KeyCode.V;
        control[(int)CharacterInput.drawSword] = KeyCode.Z;
        control[(int)CharacterInput.useProp] = KeyCode.R;
        //control[(int)CharacterInput.changeProp] = KeyCode.Q;
        control[(int)CharacterInput.prop1] = KeyCode.Alpha1;
        control[(int)CharacterInput.prop2] = KeyCode.Alpha2;
        control[(int)CharacterInput.prop3] = KeyCode.Alpha3;
        control[(int)CharacterInput.prop4] = KeyCode.Alpha4;
        control[(int)CharacterInput.exit] = KeyCode.Escape;
    }
    public KeyCode GetCharacterInputKey(CharacterInput input)
    {
        return control[(int)input];
    }
    public void ChangeKeyCode(CharacterInput input, KeyCode key)
    {
        control[(int)input] = key;
    }
    public bool CheckRepetition()
    {
        for (int i = 0; i < (int)CharacterInput.End; i++)
        {
            for (int j = i + 1; j < (int)CharacterInput.End; j++)
            {
                if (control[i] == control[j])
                {
                    return true;
                }
            }
        }
        return false;
    }
    public bool GetMouseDown(int button)
    {
        return Input.GetMouseButtonDown(button);
    }
    public bool GetKeyDown(CharacterInput input)
    {
        return !IsAuto && preInputs[(int)input] == 0;
    }
    public bool GetKey(CharacterInput input)
    {
        return !IsAuto && Input.GetKey(control[(int)input]);
    }
    public bool GetKeyUp(CharacterInput input)
    {
        return !IsAuto && Input.GetKeyUp(control[(int)input]);
    }
    public void Update(float deltaTime)
    {
//#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if(control[(int)CharacterInput.drawSword] == KeyCode.Z)
            {
                UIManager.Instance.Open(NameList.UI.TipUI, args: "键位切换至type2");
                control[(int)CharacterInput.dodge]     = KeyCode.L;
                control[(int)CharacterInput.drawSword] = KeyCode.N;
            }
            else
            {
                UIManager.Instance.Open(NameList.UI.TipUI, args: "键位切换至type1");
#if UNITY_EDITOR
                control[(int)CharacterInput.dodge] = KeyCode.X;//LeftAlt;
#else
        control[(int)CharacterInput.dodge] = KeyCode.LeftAlt;
#endif
                control[(int)CharacterInput.drawSword] = KeyCode.Z;
            }
        }
//#endif
        for (int i = 0; i < (int)CharacterInput.End; i++)
        {
            preInputs[i] += deltaTime;
            if (Input.GetKeyDown(control[i]))
                preInputs[i] = 0;
        }
    }
    public void FixedUpdate(float deltaTime) { }
    public void Dispose() { }

}

