using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float _speed = 20f;
    [SerializeField] private int _damage = 25;
    [SerializeField] private float _lifeTime = 3f;

    public override void OnNetworkSpawn()
    {
        // Только сервер планирует удаление пули через время
        if (IsServer)
        {
            StartCoroutine(DestroyAfterDelay());
        }
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Только сервер обрабатывает попадания
        if (!IsServer) return;
        
        // КРИТИЧНО: Если пуля еще не успела "заспавниться" в сети, ничего не делаем
        if (!NetworkObject.IsSpawned) return;

        var target = other.GetComponent<PlayerNetwork>();
        
        if (target != null)
        {
            // Не наносим урон самому стрелку
            if (target.OwnerClientId == OwnerClientId) return;

            target.HP.Value = Mathf.Max(0, target.HP.Value - _damage);
            
            // Удаляем пулю при попадании в игрока
            NetworkObject.Despawn(true);
        }
        // Если попали в стену/пол (объекты с коллайдером, но без PlayerNetwork)
        else if (!other.isTrigger)
        {
            NetworkObject.Despawn(true);
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(_lifeTime);
        // Проверяем, не удалили ли пулю раньше (при попадании)
        if (NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }
}