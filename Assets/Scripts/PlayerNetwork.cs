using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [Header("Network States")]
    // Синхронизация Ника и HP (из 1 практики)
    public NetworkVariable<FixedString32Bytes> Nickname = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> HP = new(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    // Состояние жизни (для 2 практики)
    public NetworkVariable<bool> IsAlive = new(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("References")]
    [SerializeField] private GameObject _visualModel; // Ссылка на модель (тело) игрока
    private Transform[] _spawnPoints;

    public override void OnNetworkSpawn()
    {
        // 1. Отправляем ник на сервер при появлении
        if (IsOwner)
        {
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
        }

        // 2. Подписываемся на события изменения здоровья и жизни
        HP.OnValueChanged += OnHpChanged;
        IsAlive.OnValueChanged += OnIsAliveChanged;

        // 3. Автоматически находим точки спавна на сцене по тегу "Respawn"
        if (IsServer)
        {
            GameObject[] points = GameObject.FindGameObjectsWithTag("Respawn");
            if (points.Length > 0)
            {
                _spawnPoints = new Transform[points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    _spawnPoints[i] = points[i].transform;
                }
            }
        }

        // Устанавливаем начальное состояние модели
        if (_visualModel != null) _visualModel.SetActive(IsAlive.Value);
    }

    public override void OnNetworkDespawn()
    {
        // Отписка обязательна!
        HP.OnValueChanged -= OnHpChanged;
        IsAlive.OnValueChanged -= OnIsAliveChanged;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitNicknameServerRpc(string nickname)
    {
        string safeValue = string.IsNullOrWhiteSpace(nickname) ? $"Player_{OwnerClientId}" : nickname.Trim();
        Nickname.Value = safeValue;
    }

    private void OnHpChanged(int prev, int next)
    {
        if (!IsServer) return;

        // Если здоровье упало до 0 и игрок еще считался живым
        if (next <= 0 && IsAlive.Value)
        {
            IsAlive.Value = false;
            StartCoroutine(RespawnRoutine());
        }
    }

    private void OnIsAliveChanged(bool prev, bool next)
    {
        // Показываем или скрываем модельку для всех
        if (_visualModel != null) _visualModel.SetActive(next);
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(3f); // Ждем 3 секунды

        Vector3 spawnPos = Vector3.up; // Позиция по умолчанию
        if (_spawnPoints != null && _spawnPoints.Length > 0)
        {
            spawnPos = _spawnPoints[Random.Range(0, _spawnPoints.Length)].position;
        }

        // ВАЖНО: Телепортируем игрока через ClientRpc, чтобы CharacterController не сопротивлялся
        TeleportPlayerClientRpc(spawnPos);

        HP.Value = 100;
        IsAlive.Value = true;
    }

    [ClientRpc]
    private void TeleportPlayerClientRpc(Vector3 targetPos)
    {
        // Выполняется только у владельца персонажа
        if (!IsOwner) return;

        CharacterController cc = GetComponent<CharacterController>();
        
        if (cc != null) cc.enabled = false; // Выключаем физику на мгновение
        transform.position = targetPos;
        if (cc != null) cc.enabled = true; // Включаем обратно
        
    }
}