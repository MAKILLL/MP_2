using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private GameObject _mainMenuPanel; // Ссылка на панель с кнопками и инпутом

    public static string PlayerNickname { get; private set; } = "Player";

    public void StartAsHost()
    {
        SaveNickname();
        if (NetworkManager.Singleton.StartHost()) // Если запуск удался
        {
            HideMenu();
        }
    }

    public void StartAsClient()
    {
        SaveNickname();
        if (NetworkManager.Singleton.StartClient()) // Если запуск удался
        {
            HideMenu();
        }
    }

    private void SaveNickname()
    {
        string rawValue = _nicknameInput != null ? _nicknameInput.text : string.Empty;
        PlayerNickname = string.IsNullOrWhiteSpace(rawValue) ? "Player" : rawValue.Trim();
    }

    private void HideMenu()
    {
        if (_mainMenuPanel != null)
        {
            _mainMenuPanel.SetActive(false); // Выключаем всё меню
        }
    }
}