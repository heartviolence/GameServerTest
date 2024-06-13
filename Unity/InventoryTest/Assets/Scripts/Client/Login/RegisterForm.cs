using Cysharp.Threading.Tasks;
using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;
using Shared.Data;
using Shared.Server.GameServer.Interface.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RegisterForm : MonoBehaviour
{
    public string ID => idField.text;
    public string Password => passwordField.text;
    TextField idField;
    TextField passwordField;
    public Button registerButton;
    public Button closeButton;

    public Action RegisterButtonClicked { get; set; }
    public Action OnBack { get; set; }    

    void OnEnable()
    {
        var rootElement = GetComponent<UIDocument>().rootVisualElement;
        idField = rootElement.Q<TextField>("FieldID");
        passwordField = rootElement.Q<TextField>("FieldPassword");
        registerButton = rootElement.Q<Button>("BtnRegister");
        registerButton.clicked += () =>
        {
            RegisterButtonClicked?.Invoke();
        };
        closeButton = rootElement.Q<Button>("BtnClose");
        closeButton.clicked += () =>
        {
            Back();
        };
    }

    public void Open()
    {
        this.gameObject.SetActive(true);
    }

    public void Back()
    {
        this.gameObject.SetActive(false);
        OnBack?.Invoke();
    }
}
