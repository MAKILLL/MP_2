using Unity.Netcode;
using UnityEngine;
using System.Collections;

// HealthPickup вешаем на префаб аптечки
public class HealthPickup : NetworkBehaviour
{
    private PickupManager _manager;
    private Vector3 _spawnPos;

    public void Init(PickupManager manager)
    {
        _manager = manager;
        _spawnPos = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        var player = other.GetComponent<PlayerNetwork>();
        if (player == null || !player.IsAlive.Value || player.HP.Value >= 100) return;

        player.HP.Value = Mathf.Min(100, player.HP.Value + 40);
        _manager.OnPickedUp(_spawnPos);
        GetComponent<NetworkObject>().Despawn(true);
    }
}