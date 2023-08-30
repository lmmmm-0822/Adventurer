using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chronos;

public class AttackEffectCharacter : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    
    private string skillNum;
    public void Init()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<TimelineChild>().animator.component;
    }
    public void PlayAttackEffect(Skill skill)
    {
        _spriteRenderer.color = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, 1);
        skillNum = string.Intern(skill.num.ToString());
        _animator.Play(skillNum, -1, 0);
    }
    public void ReplayAttackEffect()
    {
        _animator.Play(skillNum, -1, 0);
    }
}

#region 旧版本_基于URPshaderGraph
//{
//    public Mesh circle_m;
//    public Shader circle_s;
//    public Mesh rectangle_m;
//    public Shader rectangle_s;
//    private ParticleSystem particle;
//    private EffectSetting[] effectSettings;
//    private ParticleSystem.MainModule main;
//    private ParticleSystemRenderer parRen;
//    private struct EffectSetting
//    {
//        public Vector3 offset;
//        public Vector3 rotation;
//        public Vector3 size;
//    }
//    private void Awake()
//    {
//        particle = GetComponent<ParticleSystem>();
//        parRen = GetComponent<ParticleSystemRenderer>();
//        main = particle.main;
//    }
//    private void Start()
//    {
//        effectSettings = new EffectSetting[15];
//        effectSettings[0] = new EffectSetting
//        {
//            offset = Vector3.zero,
//            rotation = new Vector3(-56, 90, -90),
//            size = new Vector3(1.2f, 1.2f, 1.2f),
//        };
//        effectSettings[1] = new EffectSetting
//        {
//            offset = Vector3.zero,
//            rotation = new Vector3(101, 90, -90),
//            size = new Vector3(-1.2f, 1.2f, 1.2f),
//        };
//        effectSettings[2] = new EffectSetting
//        {
//            offset = new Vector3(0.1f,0.24f,0),
//            rotation = new Vector3(95, 90, -90),
//            size = new Vector3(-1.2f, 1.2f, 1.2f),
//        };
//        effectSettings[3] = new EffectSetting
//        {
//            offset = new Vector3(0.1f, 0.1f, 0),
//            rotation = new Vector3(-75, 90, -90),
//            size = new Vector3(1.2f, 1.2f, 1.2f),
//        };
//        effectSettings[4] = new EffectSetting
//        {
//            offset = Vector3.zero,
//            rotation = new Vector3(50, 90, -90),
//            size = new Vector3(-1.2f, 1.2f, 1.2f),
//        };
//        effectSettings[5] = new EffectSetting
//        {
//            offset = new Vector3(0.33f, 0.483f, 0),
//            rotation = new Vector3(-15f, 182.4f, -171.33f),
//            size = new Vector3(1.2f, 1.2f, 1.2f),
//        };
//        effectSettings[6] = new EffectSetting
//        {
//            offset = new Vector3(0.3f, -0.12f, 0),
//            rotation = new Vector3(0, 84.5f, -0),
//            size = new Vector3(0.5f, 5f, 1.5f),
//        };
//        effectSettings[7] = new EffectSetting
//        {
//            offset = new Vector3(0, 0.385f, 0),
//            rotation = new Vector3(-3.76f, 89.5f, -17.383f),
//            size = new Vector3(1.55832f, 1.2f, 1.58196f),
//        };
//        effectSettings[8] = new EffectSetting
//        {
//            offset = new Vector3(0.06f, 0.2f, 0),
//            rotation = new Vector3(-43.28f, 90, -90),
//            size = new Vector3(1.2f, 1.2f, 1.2f),
//        };
//        effectSettings[9] = new EffectSetting
//        {
//            offset = new Vector3(0.863f, -0.149f, 0),
//            rotation = new Vector3(180, 90, -90),
//            size = new Vector3(1.2f, 1.2f, 1.2f),
//        };
//        effectSettings[10] = new EffectSetting
//        {
//            offset = new Vector3(0, -0.034f, 0),
//            rotation = new Vector3(2.154f, 168.525f, -3.895f),
//            size = new Vector3(-1.3f, 3.17f, 0.26f),
//        };
//        effectSettings[11] = new EffectSetting
//        {
//            offset = new Vector3(-0.085f, -0.045f, -0.017f),
//            rotation = new Vector3(1.397f, 168.41f, -7.607f),
//            size = new Vector3(-1.84184f, 5.16f, 0.26f),
//        };
//        effectSettings[12] = new EffectSetting
//        {
//            offset = new Vector3(0.1f, 0.21f, 0),
//            rotation = new Vector3(86.431f, 90f, -90f),
//            size = new Vector3(-1.2f, 1.2f, 1.2f),
//        }; 
//        effectSettings[13] = new EffectSetting
//        {
//            offset = new Vector3(0.3f, 0, 0),
//            rotation = new Vector3(0, -115.4f, 180),
//            size = new Vector3(0.8f, 4f, 1.5f),
//        };
//    }
//    public void PlayAttackEffect(Skill skill,bool isWeak)
//    {
//        main.startColor = new Color(1, 1, 1, isWeak ? 85f / 255f : 1);
//        main.simulationSpeed = 1;
//        parRen.mesh = circle_m;
//        parRen.material.shader = circle_s;
//        switch (skill.num)
//        {
//            case 11: SetEffectSetting(0);
//                main.simulationSpeed = 0.7f;
//                break;
//            case 12: SetEffectSetting(1);
//                main.simulationSpeed = 0.7f; 
//                break;
//            case 13: SetEffectSetting(10);
//                main.simulationSpeed = 1.8f;
//                break;
//            case 14: SetEffectSetting(2); break;
//            case 15: SetEffectSetting(3);
//                main.simulationSpeed = 1.8f;
//                break;
//            case 21: SetEffectSetting(4);
//                main.simulationSpeed = 1.5f;
//                break;
//            case 22:
//                SetEffectSetting(11);
//                main.simulationSpeed = 1.8f;
//                break;
//            case 23:
//                SetEffectSetting(11);
//                main.simulationSpeed = 1.8f;
//                break;
//            case 24: SetEffectSetting(13); break;
//            case 31: SetEffectSetting(5); break;
//            case 32: SetEffectSetting(7);
//                main.simulationSpeed = 1.2f;
//                break;
//            case 33: SetEffectSetting(8);
//                main.simulationSpeed = 1.2f; 
//                break;
//            case 34:
//                SetEffectSetting(9);
//                main.simulationSpeed = 1.5f; 
//                parRen.mesh = rectangle_m;
//                parRen.material.shader = rectangle_s;
//                break;
//            case 51: SetEffectSetting(6); break;
//            case 52:
//                SetEffectSetting(6);
//                main.simulationSpeed = 1.2f;
//                break;
//            case 61: SetEffectSetting(12); break;
//            default:
//                Debug.LogWarning(skill.num + "号技能没有设置攻击特效");
//                return;
//        }

//        particle.Play();
//    }
//    public void ReplayAttackEffect()
//    {
//        particle.Play();
//    }
//    public void PauseAttackEffect()
//    {
//        particle.Pause();
//    }
//    public void RestartAttackEffect()
//    {
//        particle.Play();
//    }
//    private void SetEffectSetting(int effectSettingNum)
//    {
//        transform.localPosition = effectSettings[effectSettingNum].offset;
//        transform.localRotation = Quaternion.Euler(effectSettings[effectSettingNum].rotation);
//        transform.localScale = effectSettings[effectSettingNum].size;
//    }
//}
#endregion