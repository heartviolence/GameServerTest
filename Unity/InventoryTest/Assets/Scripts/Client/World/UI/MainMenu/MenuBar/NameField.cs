
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NameField : MonoBehaviour
{
    public TMP_InputField inputField;

    [SerializeField]
    EnableAnimation enableAnimation;

    [SerializeField]
    DisableAnimation disableAnimation;
        
    public async UniTask EnableAnimation()
    {
        if (gameObject.activeSelf)
        {
            return;
        }
        gameObject.SetActive(true);
        inputField.interactable = true;
        await enableAnimation.StartAsync();
        inputField.Select();
    }

    public async UniTask DisableAnimation()
    {
        inputField.interactable = false;
        await disableAnimation.StartAsync();
        gameObject.SetActive(false);
    }
}