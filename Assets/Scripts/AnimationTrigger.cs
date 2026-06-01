using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    [Tooltip("Ссылка на компонент Animator (если не назначить, возьмётся с этого же GameObject)")]
    public Animator animator;

    [Tooltip("Имя триггера, прописанного в Animator Controller")]
    public string triggerName;

    private void Awake()
    {
        // Автоматически ищем Animator на этом объекте, если поле пустое
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Вызывает анимацию по триггеру. Удобно биндить в Animation Event, UI Button и т.д.
    /// </summary>
    public void PlayTrigger()
    {
        if (animator != null && !string.IsNullOrEmpty(triggerName))
        {
            animator.SetTrigger(triggerName);
        }
        else
        {
            Debug.LogWarning($"[{name}] AnimationTrigger: Не назначен Animator или пустое имя триггера!", this);
        }
    }
}

