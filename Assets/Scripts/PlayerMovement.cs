using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _gravity = -9.81f;

    private CharacterController _cc;
    private PlayerNetwork _playerNetwork;
    private float _verticalVelocity;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _playerNetwork = GetComponent<PlayerNetwork>();
    }

    private void Update()
    {
        if (!IsOwner || !_playerNetwork.IsAlive.Value) return;
        if (!IsOwner) return;
        if (!_playerNetwork.IsAlive.Value) return; // Мертвый не двигается

        float mouseX = Input.GetAxis("Mouse X"); // Для старой системы
        transform.Rotate(Vector3.up * mouseX * 2f); // 2f - это чувствительность


        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 moveInput = (transform.forward * v + transform.right * h).normalized;
        Vector3 move = moveInput * _speed;  

        _verticalVelocity += _gravity * Time.deltaTime;
        move.y = _verticalVelocity;

        _cc.Move(move * Time.deltaTime);

        if (_cc.isGrounded) _verticalVelocity = 0f;
    }
}