using UnityEngine;
using System.Collections;
using ZFrame.Asset;

public class AutoPooled : MonoBehaviour
{
    [SerializeField]
    private float m_Delay;

    private void Recycle(float delay)
    {
#if !FX_DESIGN
        var poolStatus = ObjectPoolManager.GetPoolStatus(gameObject);
        if (poolStatus != PoolStatus.None) {
            if (poolStatus == PoolStatus.Pooled) {
                ObjectPoolManager.DestroyPooled(gameObject, delay);
            }
            return;
        }

        poolStatus = ObjectPoolManager.GetPoolStatusScenely(gameObject);
        if (poolStatus != PoolStatus.None) {
            if (poolStatus == PoolStatus.Pooled) {
                ObjectPoolManager.DestroyPooledScenely(gameObject, delay);
            }
            return;
        }
#endif
        Destroy(gameObject, delay);
    }

    protected void Start()
    {
        if (m_Delay <= 0) return;

        Recycle(m_Delay);

    }

    protected void OnDisable()
    {
        if (AssetLoader.Instance && !AssetLoader.Instance.isQuiting) {
            Recycle(0);
        }
    }
}
