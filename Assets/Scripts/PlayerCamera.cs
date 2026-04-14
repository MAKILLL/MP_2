using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem; // Если используешь новую систему ввода

public class PlayerCamera : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector3 _offset = new Vector3(0f, 1.6f, 0f); // Уровень глаз
    [SerializeField] private float _sensitivity = 2f;
    [SerializeField] private float _minPitch = -60f;
    [SerializeField] private float _maxPitch = 60f;

    private Transform _camTransform;
    private float _pitch = 0f;

    public override void OnNetworkSpawn()
    {
        // Если это не наш персонаж — выключаем скрипт, чтобы не управлять чужой камерой
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        // Берем главную камеру со сцены
        if (Camera.main != null)
        {
            _camTransform = Camera.main.transform;
            // Блокируем курсор, чтобы он не вылетал за пределы окна
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void LateUpdate()
    {
        if (_camTransform == null) return;

        // 1. Позиция камеры следует за игроком
        _camTransform.position = transform.position + _offset;

        // 2. Вращение мышкой (Вертикальное - Pitch)
        float mouseY = 0;
        
        // Для старой системы ввода:
        mouseY = Input.GetAxis("Mouse Y"); 
        // Если новая система ввода (измени на Mouse.current.delta.y.ReadValue() если нужно)

        _pitch -= mouseY * _sensitivity;
        _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

        // Применяем вращение: Камера крутится вверх-вниз, а тело игрока — влево-вправо
        _camTransform.localRotation = Quaternion.Euler(_pitch, transform.eulerAngles.y, 0f);
    }
}