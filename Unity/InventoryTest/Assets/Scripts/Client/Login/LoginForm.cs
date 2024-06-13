using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LoginForm : MonoBehaviour
{
    [SerializeField]
    private RegisterForm RegisterForm;

    public string ID => idField.text;
    public string Password => passwordField.text;

    TextField idField;
    TextField passwordField;
    public Button registerButton;
    public Button loginButton;
    VisualElement rootElement;

    public Action RegisterButtonClicked { get; set; }
    public Action LoginButtonClicked { get; set; }

    bool loginWaiting = false;

    void OnEnable()
    {
        rootElement = GetComponent<UIDocument>().rootVisualElement;
        idField = rootElement.Q<TextField>("FieldID");
        passwordField = rootElement.Q<TextField>("FieldPassword");
        registerButton = rootElement.Q<Button>("BtnRegister");
        loginButton = rootElement.Q<Button>("BtnLogin");

        registerButton.clicked += () =>
        {
            RegisterButtonClicked?.Invoke();
        };

        loginButton.clicked += () =>
        {
            LoginButtonClicked?.Invoke();
        };
    }

    public void Enable(bool value)
    {
        rootElement.SetEnabled(value);
    }

    public async UniTask LoginWaiting()
    {
        Enable(false);
        loginWaiting = true;
        while (loginWaiting)
        {
            await UniTask.WaitForSeconds(1);
        }
        Enable(true);
        loginWaiting = false;
    }

    public void LoginFailed()
    {
        loginWaiting = false;
    }
}
