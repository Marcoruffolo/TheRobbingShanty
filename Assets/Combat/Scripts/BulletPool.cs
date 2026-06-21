using UnityEngine;

public class BulletPool : MonoBehaviour
{
    private ObjectPool<Bullet> _pool;

    public static BulletPool Create(Bullet prefab, int size, Transform parent)
    {
        var go = new GameObject("BulletPool");
        go.transform.SetParent(parent);

        var pool = go.AddComponent<BulletPool>();
        pool._pool = new ObjectPool<Bullet>(() => Instantiate(prefab, pool.transform), size);
        return pool;
    }

    public bool TryGet(out Bullet bullet) => _pool.TryGet(out bullet);

    public void Return(Bullet bullet)
    {
        bullet.transform.SetParent(transform, false);
        _pool.Return(bullet);
    }
}
