
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PopupMessage : MonoBehaviour
{
    [SerializeField]
    TMP_Text tmp;

    public async UniTask ShowAsync(string message, float duration)
    {
        tmp.text = message;
        this.gameObject.SetActive(true);
        await UniTask.WaitForSeconds(duration);
        this.gameObject.SetActive(false);
    }
}