using Unity.Netcode;
using UnityEngine;
using TMPro;

public class PlayerShooting : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _cooldown = 0.5f;

    [Header("UI")]
    [SerializeField] private TMP_Text _ammoText; // Ссылка на текст патронов

    // Используем NetworkVariable для надежной синхронизации
    public NetworkVariable<int> Ammo = new(10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    private float _nextShotTime;
    private PlayerNetwork _pn;

    public override void OnNetworkSpawn()
    {
        _pn = GetComponent<PlayerNetwork>();

        // Подписываемся на обновление патронов
        Ammo.OnValueChanged += OnAmmoChanged;

        // Сразу обновляем UI при спавне
        UpdateAmmoUI(0, Ammo.Value);
    }

    public override void OnNetworkDespawn()
    {
        Ammo.OnValueChanged -= OnAmmoChanged;
    }

    private void Update()
    {
        // Стрелять может только владелец и только если он жив
        if (!IsOwner || !_pn.IsAlive.Value) return;

        if (Input.GetMouseButtonDown(0) && Time.time > _nextShotTime)
        {
            if (Ammo.Value > 0)
            {
                _nextShotTime = Time.time + _cooldown;
                ShootServerRpc(_firePoint.position, _firePoint.forward);
            }
        }
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 pos, Vector3 dir)
    {
        // Валидация на сервере
        if (!_pn.IsAlive.Value || Ammo.Value <= 0) return;

        // Уменьшаем патроны на сервере, NetworkVariable сама разошлет это всем
        Ammo.Value--;

        // Спавним пулю
        var bullet = Instantiate(_bulletPrefab, pos + dir * 1.5f, Quaternion.LookRotation(dir));
        bullet.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
    }

    private void OnAmmoChanged(int oldVal, int newVal)
    {
        UpdateAmmoUI(oldVal, newVal);
    }

    private void UpdateAmmoUI(int oldVal, int newVal)
    {
        // Текст видит только владелец (или все, если хочешь видеть чужие патроны)
        if (IsOwner && _ammoText != null)
        {
            _ammoText.text = $"Ammo: {newVal}";
        }
    }
}