using UnityEngine;
using UnityEngine.Events; // Для событий

public class SafeMechanism : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Animator сейфа или интерфейса")]
    public Animator safeAnimator;

    [Header("Animator Triggers")]
    [Tooltip("Имя триггера при НЕПРАВИЛЬНОМ коде")]
    public string wrongTriggerName = "WrongCode";
    
    [Tooltip("Имя триггера при ПРАВИЛЬНОМ коде")]
    public string correctTriggerName = "CorrectCode";

    [Header("Security Settings")]
    [Tooltip("Правильный пароль (цифры)")]
    public string password = "1234";

    [Tooltip("Автоматически сбрасывать ввод после попытки?")]
    public bool autoResetAfterAttempt = true;

    [Tooltip("Задержка перед сбросом после попытки (сек)")]
    public float resetDelay = 1f;

    [Tooltip("Блокировать ввод навсегда после успешного взлома?")]
    public bool lockAfterUnlock = true;

    [Header("Events")]
    [Tooltip("Событие, вызываемое при успешном вводе кода")]
    public UnityEvent OnSafeUnlocked;

    // Приватное поле для накопления ввода
    private string currentInput = string.Empty;
    
    // Флаг: был ли сейф уже открыт
    private bool isUnlocked = false;

    private void Start()
    {
        // Убеждаемся, что триггеры сброшены при старте
        if (safeAnimator)
        {
            safeAnimator.ResetTrigger(wrongTriggerName);
            safeAnimator.ResetTrigger(correctTriggerName);
        }
    }

    #region ⬇️ 10 PUBLIC VOID METHODS (для кнопок 0-9) ⬇️
    /// Назначь эти методы на UI кнопки через Inspector: Button.OnClick() → SafeMechanism → AddDigitX()

    public void AddDigit0() => AddDigit("0");
    public void AddDigit1() => AddDigit("1");
    public void AddDigit2() => AddDigit("2");
    public void AddDigit3() => AddDigit("3");
    public void AddDigit4() => AddDigit("4");
    public void AddDigit5() => AddDigit("5");
    public void AddDigit6() => AddDigit("6");
    public void AddDigit7() => AddDigit("7");
    public void AddDigit8() => AddDigit("8");
    public void AddDigit9() => AddDigit("9");
    #endregion

    #region ⬇️ PUBLIC UTILS ⬇️

    /// <summary>
    /// Добавляет цифру к текущему вводу и проверяет, если длина совпала с паролем.
    /// Игнорирует ввод, если сейф уже открыт и lockAfterUnlock = true.
    /// </summary>
    private void AddDigit(string digit)
    {
        // Если сейф уже открыт и блокировка включена — игнорируем ввод
        if (isUnlocked && lockAfterUnlock) return;
        
        currentInput += digit;
        
        // Проверяем, достигли ли нужной длины
        if (currentInput.Length == password.Length && !string.IsNullOrEmpty(password))
        {
            EvaluateInput();
        }
    }

    /// <summary>
    /// Сравнивает ввод с паролем и запускает соответствующий триггер.
    /// </summary>
    private void EvaluateInput()
    {
        if (currentInput == password)
        {
            // ✅ ПРАВИЛЬНЫЙ КОД
            isUnlocked = true;
            TriggerAnimator(correctTriggerName);
            Debug.Log("✅ Safe opened!");
            
            // Вызываем события для других скриптов
            OnSafeUnlocked?.Invoke();
        }
        else
        {
            // ❌ НЕПРАВИЛЬНЫЙ КОД
            TriggerAnimator(wrongTriggerName);
            Debug.Log("❌ Wrong code!");
        }

        if (autoResetAfterAttempt)
        {
            Invoke(nameof(ResetInput), resetDelay);
        }
    }

    /// <summary>
    /// Запускает триггер в Animator с проверкой на null.
    /// </summary>
    private void TriggerAnimator(string triggerName)
    {
        if (safeAnimator && !string.IsNullOrEmpty(triggerName))
        {
            safeAnimator.SetTrigger(triggerName);
        }
    }

    /// <summary>
    /// Сбрасывает текущий ввод.
    /// Назначь на кнопку "Clear" или "Backspace".
    /// </summary>
    public void ResetInput()
    {
        CancelInvoke(nameof(ResetInput));
        currentInput = string.Empty;
        
        // Сбрасываем триггеры, чтобы анимация могла проиграться снова
        if (safeAnimator)
        {
            safeAnimator.ResetTrigger(wrongTriggerName);
            safeAnimator.ResetTrigger(correctTriggerName);
        }
    }

    /// <summary>
    /// Удаляет последний введённый символ (Backspace).
    /// Назначь на кнопку "⌫".
    /// </summary>
    public void RemoveLastDigit()
    {
        if (isUnlocked && lockAfterUnlock) return;
        if (string.IsNullOrEmpty(currentInput)) return;
        
        currentInput = currentInput.Substring(0, currentInput.Length - 1);
    }

    /// <summary>
    /// Возвращает текущий введённый код (для отображения в UI).
    /// </summary>
    public string GetCurrentInput() => currentInput;

    /// <summary>
    /// Возвращает длину пароля (для инициализации UI).
    /// </summary>
    public int GetPasswordLength() => string.IsNullOrEmpty(password) ? 0 : password.Length;

    /// <summary>
    /// Возвращает состояние: открыт ли сейф.
    /// </summary>
    public bool IsUnlocked() => isUnlocked;

    /// <summary>
    /// Принудительно открывает сейф (программно).
    /// </summary>
    public void ForceUnlock()
    {
        isUnlocked = true;
        TriggerAnimator(correctTriggerName);
        OnSafeUnlocked?.Invoke();
    }

    /// <summary>
    /// Принудительно закрывает/сбрасывает сейф (для перезагрузки уровня и т.д.).
    /// </summary>
    public void ForceLock()
    {
        isUnlocked = false;
        ResetInput();
    }

    #endregion
}
