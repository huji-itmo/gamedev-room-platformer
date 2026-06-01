using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class SafeMechanismPause : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private string interactActionName = "Interact";
    [SerializeField] private LayerMask playerLayer = 1 << 3; // Layer 3 = Player
    [SerializeField] private bool interactiveEnabled = true;
    [SerializeField] private bool showOnEnter = true;   // Авто-показ при входе
    [SerializeField] private bool hideOnExit = false;   // Авто-скрытие при выходе
    [SerializeField] private bool requireInteractKey = false; // Требовать нажатие кнопки

    [Header("References")]
    [Tooltip("Animator для канваса с пин-кодом")]
    public Animator pinCodeAnimator;
    
    [Tooltip("Имя int-параметра: 0 = скрыт, 1 = показан")]
    public string canvasStateParam = "CanvasState";

    [Header("Screen States")]
    public int hiddenState = 0;
    public int activeState = 1;

    [Header("Cursor Settings")]
    [Tooltip("Вернуть захват курсора после закрытия интерфейса? (для FPS)")]
    public bool lockCursorOnClose = true;
    [Tooltip("Скрыть курсор после закрытия интерфейса? (для FPS)")]
    public bool hideCursorOnClose = true;

    private InputAction _interactAction;
    private bool _isPlayerInside;
    private bool _interactPressed;
    private bool _isInterfaceOpen = false;

    private void Awake()
    {
        // Инициализация Input Action
        if (InputSystem.actions != null && InputSystem.actions.FindAction(interactActionName) != null)
        {
            _interactAction = InputSystem.actions[interactActionName];
        }
        else
        {
            Debug.LogWarning($"[SafeMechanismPause] Input Action '{interactActionName}' not found.", this);
        }

        // Проверка коллайдера
        var collider = GetComponent<Collider>();
        if (collider != null && !collider.isTrigger)
        {
            Debug.LogWarning($"[SafeMechanismPause] Collider on '{gameObject.name}' is not marked as Trigger!", this);
        }

        if (pinCodeAnimator == null)
            pinCodeAnimator = GetComponent<Animator>();
    }

    private void OnEnable() => _interactAction?.Enable();
    private void OnDisable() => _interactAction?.Disable();

    private void Update()
    {
        if (!interactiveEnabled) return;

        if (_interactAction != null)
            _interactPressed = _interactAction.WasPressedThisFrame();

        if (_isPlayerInside)
        {
            bool shouldShow = showOnEnter || (_interactPressed && requireInteractKey);
            
            if (shouldShow && pinCodeAnimator != null && !_isInterfaceOpen)
            {
                int currentState = pinCodeAnimator.GetInteger(canvasStateParam);
                if (currentState == hiddenState)
                {
                    ShowPinCodeCanvas();
                }
            }
            
            if (_interactPressed) _interactPressed = false;
        }
    }

    #region Trigger Events (LayerMask-based)

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            _isPlayerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            _isPlayerInside = false;
            
            if (hideOnExit && interactiveEnabled && _isInterfaceOpen)
            {
                HidePinCodeCanvas();
            }
        }
    }

    #endregion

    #region ⬇️ CURSOR & INTERFACE LOGIC (no Time.timeScale) ⬇️

    /// <summary>
    /// Освобождает курсор для взаимодействия с UI.
    /// Игра НЕ ставится на паузу — время продолжает идти.
    /// </summary>
    private void EnableUIMode()
    {
        if (_isInterfaceOpen) return;
        
        _isInterfaceOpen = true;
        
        // Освобождаем курсор для кликов по UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Возвращает курсор в игровой режим.
    /// Игра продолжает работать без паузы.
    /// </summary>
    private void DisableUIMode()
    {
        if (!_isInterfaceOpen) return;
        
        _isInterfaceOpen = false;
        
        // Возвращаем захват курсора для FPS
        if (lockCursorOnClose)
            Cursor.lockState = CursorLockMode.Locked;
        if (hideCursorOnClose)
            Cursor.visible = false;
    }

    /// <summary>
    /// Возвращает состояние интерфейса.
    /// </summary>
    public bool IsInterfaceOpen() => _isInterfaceOpen;

    #endregion

    #region ⬇️ PUBLIC METHODS (для UI кнопок) ⬇️

    /// <summary>
    /// Показывает интерфейс пин-кода и освобождает курсор.
    /// Игра продолжает работать (без паузы).
    /// Назначь на кнопку "Открыть" или вызывай из других скриптов.
    /// </summary>
    public void ShowPinCodeCanvas()
    {
        EnableUIMode();
        SetCanvasState(activeState);
    }

    /// <summary>
    /// Скрывает интерфейс и возвращает курсор в игровой режим.
    /// Игра продолжает работать (без паузы).
    /// Назначь на кнопки "Назад", "Закрыть" или после успешного ввода кода.
    /// </summary>
    public void HidePinCodeCanvas()
    {
        SetCanvasState(hiddenState);
        DisableUIMode();
    }

    /// <summary>
    /// Алиас для HidePinCodeCanvas (удобно для инспектора).
    /// </summary>
    public void CloseSafeInterface() => HidePinCodeCanvas();

    /// <summary>
    /// Принудительно меняет состояние канваса (без управления курсором).
    /// </summary>
    public void SetCanvasVisibility(bool isVisible)
    {
        SetCanvasState(isVisible ? activeState : hiddenState);
    }

    /// <summary>
    /// Возвращает, находится ли игрок в зоне взаимодействия.
    /// </summary>
    public bool IsPlayerInside() => _isPlayerInside;

    /// <summary>
    /// Включает/выключает взаимодействие (для квестов, блокировок и т.д.).
    /// </summary>
    public void SetInteractive(bool enabled) => interactiveEnabled = enabled;

    #endregion

    #region Internal Helpers

    private void SetCanvasState(int state)
    {
        if (pinCodeAnimator)
        {
            pinCodeAnimator.SetInteger(canvasStateParam, state);
        }
    }

    /// <summary>
    /// Вызывается из SafeMechanism после успешного ввода кода.
    /// </summary>
    public void OnSafeOpened()
    {
        Debug.Log($"🔓 Safe '{gameObject.name}' opened!", this);
        // Здесь можно:
        // - Запустить анимацию открытия двери
        // - Заспавнить лут
        // - Отключить коллайдер: GetComponent<Collider>().enabled = false;
    }

    #endregion

    #region Editor Helpers

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        var collider = GetComponent<Collider>();
        if (collider == null) return;

        Gizmos.color = new Color(1f, 0.8f, 0f, 0.4f);

        if (collider is SphereCollider sphere)
        {
            Vector3 worldCenter = transform.TransformPoint(sphere.center);
            Gizmos.DrawWireSphere(worldCenter, sphere.radius);
        }
        else if (collider is BoxCollider box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
        else if (collider is CapsuleCollider capsule)
        {
            Vector3 worldCenter = transform.TransformPoint(capsule.center);
            Gizmos.DrawWireSphere(worldCenter, capsule.radius);
        }
    }
#endif

    #endregion
}
