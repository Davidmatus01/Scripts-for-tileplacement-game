using System.Collections;
using TMPro;
using UnityEngine;

public class SimpleMessageUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float duration = 2f;
    public static SimpleMessageUI instance;
    private void Awake()
    {
        instance = this;
        messageText.gameObject.SetActive(false);
    }

    public void Show(string message) // Display a message for a set duration
    {
        StopAllCoroutines();
        messageText.text = message;
        messageText.gameObject.SetActive(true);
        StartCoroutine(Hide());
    }

    private IEnumerator Hide()
    {
        yield return new WaitForSeconds(duration);
        messageText.gameObject.SetActive(false);
    }
}