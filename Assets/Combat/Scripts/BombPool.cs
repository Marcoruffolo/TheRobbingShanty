using UnityEngine;

public class BombPool : MonoBehaviour
{
    private ObjectPool<Bomb> _pool;

    public static BombPool Create(Bomb prefab, int size, Transform parent)
    {
        var go = new GameObject("BombPool");
        go.transform.SetParent(parent);

        var pool = go.AddComponent<BombPool>();
        pool._pool = new ObjectPool<Bomb>(() => Instantiate(prefab, pool.transform), size);
        return pool;
    }

    public bool TryGet(out Bomb bomb) => _pool.TryGet(out bomb);

    public void Return(Bomb bomb)
    {
        bomb.transform.SetParent(transform, false);
        _pool.Return(bomb);
    }
}
