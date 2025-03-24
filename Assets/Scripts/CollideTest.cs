using UnityEngine;

public class CollideTest : MonoBehaviour
{
    // Ссылка на CapsuleCollider, который должен быть настроен как триггер
    [SerializeField] 
    private CapsuleCollider triggerCapsuleCollider;

    private void Start()
    {
        // Проверяем, что Collider задан и установлен как триггер
        if (triggerCapsuleCollider == null)
        {
            Debug.LogError("CapsuleCollider не задан в инспекторе.");
        }
        else if (!triggerCapsuleCollider.isTrigger)
        {
            Debug.LogWarning("Указанный CapsuleCollider не настроен как триггер. Установите флаг isTrigger.");
        }
    }

    // Вызывается, когда другой Collider входит в этот триггер
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Объект вошел в триггер: " + other.gameObject.name);
    }

    // Вызывается, когда происходит столкновение (если коллайдер не является триггером)
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Объект соприкоснулся: " + collision.gameObject.name);
    }
}