using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame
{
    public interface ITickBase
    {
        string name { get; }
        bool ignoreTimeScale { get; }
    }

    public interface ITickable : ITickBase
    {
        void Tick(float deltaTime);
    }

    public interface ILateTick : ITickBase
    {
        void LateTick(float deltaTime);
    }

    public interface IFixedTick : ITickBase
    {
        void FixedTick(float deltaTime);
    }

    public class TickManager : MonoSingleton<TickManager>
    {
        #region Update

        [Description("UpdateTicks")]
        private readonly List<ITickBase> m_UpdateTicks = new List<ITickBase>();

        [Description("LateUpdateTicks")]
        private readonly List<ITickBase> m_LateUpdateTicks = new List<ITickBase>();

        [Description("FixedUpdateTicks")]
        private readonly List<ITickBase> m_FixedUpdateTicks = new List<ITickBase>();

        private readonly HashSet<ITickBase> m_TempAdd = new HashSet<ITickBase>();   

        private bool ExistTick(ITickBase tick)
        {
            return m_UpdateTicks.Contains(tick) || m_LateUpdateTicks.Contains(tick) || m_FixedUpdateTicks.Contains(tick);
        }

        private void AddTicks()
        {
            foreach (var tick in m_TempAdd) {
                if (tick is ITickable) m_UpdateTicks.Add(tick);
                if (tick is ILateTick) m_LateUpdateTicks.Add(tick);
                if (tick is IFixedTick) m_FixedUpdateTicks.Add(tick);
            }
            m_TempAdd.Clear();
        }

        private void RemoveTick(ITickBase tick)
        {
            if (tick is ITickable) {
                var index = m_UpdateTicks.IndexOf(tick);
                if (index >= 0) m_UpdateTicks[index] = null;
            }

            if (tick is ILateTick) {
                var index = m_LateUpdateTicks.IndexOf(tick);
                if (index >= 0) m_LateUpdateTicks[index] = null;
            }

            if (tick is IFixedTick) {
                var index = m_FixedUpdateTicks.IndexOf(tick);
                if (index >= 0) m_FixedUpdateTicks[index] = null;
            }
        }

        private void Update()
        {
            AddTicks();
            m_UpdateTicks.RemoveNull();

            var deltaTime = Time.deltaTime;
            var unscaledDeltaTime = Time.unscaledDeltaTime;

            for (int i = 0; i < m_UpdateTicks.Count; ++i) {
                var tick = m_UpdateTicks[i] as ITickable;
                if (tick == null || tick.Equals(null)) continue;

                tick.Tick(tick.ignoreTimeScale ? unscaledDeltaTime : deltaTime);                
            }
        }

        private void LateUpdate()
        {
            m_LateUpdateTicks.RemoveNull();
            
            var deltaTime = Time.deltaTime;
            var unscaledDeltaTime = Time.unscaledDeltaTime;

            for (int i = 0; i < m_LateUpdateTicks.Count; ++i) {
                var tick = m_LateUpdateTicks[i] as ILateTick;
                if (tick == null || tick.Equals(null)) continue;

                tick.LateTick(tick.ignoreTimeScale ? unscaledDeltaTime : deltaTime);
            }
        }

        private void FixedUpdate()
        {
            m_FixedUpdateTicks.RemoveNull();

            var deltaTime = Time.deltaTime;
            var unscaledDeltaTime = Time.unscaledDeltaTime;

            for (int i = 0; i < m_FixedUpdateTicks.Count; ++i) {
                var tick = m_FixedUpdateTicks[i] as IFixedTick;
                if (tick == null || tick.Equals(null)) continue;

                tick.FixedTick(tick.ignoreTimeScale ? unscaledDeltaTime : deltaTime);
            }
        }

        public static void Add(ITickBase tick)
        {
            if (Instance == null || tick == null || tick.Equals(null)) return;
            if (Instance.ExistTick(tick)) return;

            Instance.m_TempAdd.Add(tick);
        }

        public static void Remove(ITickBase tick)
        {
            if (Instance == null || tick == null || tick.Equals(null)) return;
            
            Instance.RemoveTick(tick);
        }

        #endregion
    }
}
