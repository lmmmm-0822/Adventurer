using System.Collections.Generic;
using QxFramework.Utilities;
using UnityEngine;
using Sirenix.OdinInspector;

namespace QxFramework.Core
{
    /// <summary>
    /// 程序入口，对各项管理类进行初始化操作。
    /// </summary>
    public class Launcher : MonoSingleton<Launcher>
    {
        [ValueDropdown("procedures")]
        /// <summary>
        /// 入口流程。
        /// </summary>
        public string StartProcedure;

        private static string[] procedures = Utilities.TypeUtilities.GetTypeNames(typeof(ProcedureBase));
        /// <summary>
        /// 系统组件列表
        /// </summary>
        private List<ISystemModule> _modules;
        private int _m_cnt;

        /// <summary>
        ///  初始化各项全局管理。
        /// </summary>
        private void Awake()
        {
            _modules = new List<ISystemModule>();

            _modules.Add(ProcedureManager.Instance);
            _modules.Add(MessageManager.Instance);
            _modules.Add(ResourceManager.Instance);
            _modules.Add(PlayerInput.Instance);
            _modules.Add(TimeEventManager.Instance);


            _m_cnt = _modules.Count;
            for (int i = 0; i < _m_cnt; i++)
            {
                _modules[i].Initialize();
            }

            Data.Instance.SetTableAgent();
        }

        /// <summary>
        ///  开始流程。
        /// </summary>
        private void Start()
        {
            ProcedureManager.Instance.ChangeTo(StartProcedure);
        }

        /// <summary>
        /// Unity每帧更新。
        /// </summary>
        private void Update()
        {
            for (int i = 0; i < _m_cnt; i++)
            {
                _modules[i].Update(Time.deltaTime);
            }
        }

        /// <summary>
        /// Unity每帧更新。
        /// </summary>
        private void FixedUpdate()
        {
            for (int i = 0; i < _m_cnt; i++)
            {
                _modules[i].FixedUpdate(Time.fixedDeltaTime);
            }
        }

        /// <summary>
        /// 当退出时调用。
        /// </summary>
        private void OnDestroy()
        {
            for (int i = 0; i < _m_cnt; i++)
            {
                _modules[i].Dispose();
            }
        }
    }
}