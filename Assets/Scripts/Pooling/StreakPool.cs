using System.Collections.Generic;
using UnityEngine;

namespace Pooling
{
    /// <summary>
    /// Lightweight pool for the GameObject+LineRenderer combo used by HardDropStreakEffect
    /// and SoftDropTrailEffect. Both effects spawn many short-lived streaks per second on
    /// soft-drop and at every hard-drop — pooling eliminates the per-event allocation +
    /// GC spike the audit flagged (50-100 ms / session on ARMv7).
    ///
    /// Usage:
    ///   var lr = StreakPool.Acquire();   // ready to configure (positions, color, width)
    ///   ...configure + use...
    ///   StreakPool.Release(lr);          // returns to pool (SetActive(false))
    ///
    /// Pool is module-scoped (static) because there is only one board and one pool of streaks
    /// at any moment — matching the singleton style already in this codebase.
    /// </summary>
    public static class StreakPool
    {
        private static readonly Queue<LineRenderer> _free = new Queue<LineRenderer>(16);
        private static Transform _root;

        /// <summary>Pre-create N instances at scene init so the first few effects don't
        /// pay the allocation tax. Safe to call repeatedly; no-op once warmed.</summary>
        public static void Prewarm(int count)
        {
            EnsureRoot();
            while (_free.Count < count)
            {
                LineRenderer lr = CreateNew();
                lr.gameObject.SetActive(false);
                _free.Enqueue(lr);
            }
        }

        /// <summary>Get an active LineRenderer ready to be configured by the caller.</summary>
        public static LineRenderer Acquire()
        {
            EnsureRoot();
            LineRenderer lr;
            while (_free.Count > 0)
            {
                lr = _free.Dequeue();
                // Pool can outlive scene reloads; skip any nulls left behind by a domain reload.
                if (lr != null)
                {
                    lr.gameObject.SetActive(true);
                    return lr;
                }
            }
            lr = CreateNew();
            return lr;
        }

        /// <summary>Return a streak to the pool. Caller must not touch it after release.</summary>
        public static void Release(LineRenderer lr)
        {
            if (lr == null) return;
            // Detach from any temporary parent the caller may have set.
            EnsureRoot();
            lr.transform.SetParent(_root, false);
            lr.gameObject.SetActive(false);
            _free.Enqueue(lr);
        }

        private static LineRenderer CreateNew()
        {
            GameObject go = new GameObject("StreakPool_LR");
            go.transform.SetParent(_root, false);
            LineRenderer lr = go.AddComponent<LineRenderer>();
            return lr;
        }

        private static void EnsureRoot()
        {
            if (_root != null) return;
            GameObject rootGo = new GameObject("[StreakPool]");
            Object.DontDestroyOnLoad(rootGo);
            _root = rootGo.transform;
        }
    }
}
