using UnityEngine;
using System.Collections;
using System;

namespace ReGolithSystems.UI
{
    /// <summary>
    /// A comprehensive UI fading system that provides smooth fade in/out animations for UI elements.
    /// Uses CanvasGroup for alpha transitions with customizable duration, easing curves, and automatic interactability management.
    /// Supports instant state changes, fade interruption, and debug logging for development.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    [AddComponentMenu("ReGolithSystems/UI/UiFader")]
    public class UiFader : MonoBehaviour
    {
        #region Serialized Fields
        
        [Header("🎯 Initial Configuration")]
        [Tooltip("Determines the starting state of the UI element when the scene loads.\n• Enabled: Starts visible and interactable\n• Disabled: Starts invisible and non-interactable")]
        [SerializeField] private DefaultState defaultState = DefaultState.Disabled;
        
        [Header("⚙️ Animation Properties")]
        [Tooltip("Duration of the fade animation in seconds. Higher values create slower, more gradual transitions.")]
        [Range(0.01f, 10f)]
        [SerializeField] private float fadeDuration = 1f;
        
        [Tooltip("Animation curve that defines the easing of the fade animation.\n• Linear: Constant speed\n• EaseInOut: Slow start and end, fast middle\n• Custom: Define your own curve")]
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Tooltip("Automatically manages interactable and blocksRaycasts properties based on the final alpha value.\n• Enabled: UI becomes interactable when alpha > 0.5\n• Disabled: Manual control required")]
        [SerializeField] private bool setInteractableOnComplete = true;
        
        [Tooltip("Controls whether this script should manage the CanvasGroup's interactable and blocksRaycasts properties.\n• Enabled: Script automatically sets these properties during fades and state changes\n• Disabled: Script only manages alpha, leaving interactable/raycasts to external control")]
        [SerializeField] private bool manageInteractableAndRaycasts = true;

        [SerializeField] private bool setCanvasGroupAlphaOnStart = false;
        
        [Header("🔧 Component References")]
        [Tooltip("Reference to the CanvasGroup component. If null, will automatically find one on this GameObject.")]
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("🐛 Debug & Development")]
        [Tooltip("Enable detailed debug logging for all fade operations. Useful for troubleshooting animation issues.")]
        [SerializeField] private bool showDebugLogs = false;
        
        #endregion
        
        #region Private Fields
        
        /// <summary>
        /// Reference to the currently running fade coroutine. Null when no fade animation is active.
        /// Used to prevent multiple simultaneous fade operations and allow for fade interruption.
        /// </summary>
        private Coroutine currentFadeCoroutine;
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Event that is invoked when FadeIn() is called.
        /// Allows other components to react to fade in operations.
        /// </summary>
        public event Action OnFadeInStarted;
        
        /// <summary>
        /// Event that is invoked when FadeOut() is called.
        /// Allows other components to react to fade out operations.
        /// </summary>
        public event Action OnFadeOutStarted;
        
        #endregion
        
        #region Enums
        
        /// <summary>
        /// Defines the possible initial states for the UI element when the scene starts.
        /// Determines whether the UI begins visible and interactable or hidden.
        /// </summary>
        public enum DefaultState
        {
            /// <summary>
            /// UI element starts visible (alpha = 1) and fully interactable.
            /// Use this for UI elements that should be immediately available to the player.
            /// </summary>
            Enabled,
            
            /// <summary>
            /// UI element starts invisible (alpha = 0) and non-interactable.
            /// Use this for UI elements that should be hidden initially and shown later.
            /// </summary>
            Disabled
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        /// <summary>
        /// Initializes the UiFader component by automatically finding the CanvasGroup component
        /// and setting the initial state based on the defaultState configuration.
        /// Called automatically by Unity when the GameObject becomes active.
        /// </summary>
        private void Start()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    if (showDebugLogs)
                        Debug.LogError("UiFader requires a CanvasGroup component!", this);
                    return;
                }
            }
            
            if(setCanvasGroupAlphaOnStart)
            {
                if(defaultState == DefaultState.Enabled)
                {
                    canvasGroup.alpha = 0f;
                    FadeIn();
                }
                else
                {
                    canvasGroup.alpha = 1f;
                    FadeOut();
                }
            }
            else
            {
                SetDefaultState();
            }
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Sets the initial state of the UI element based on the defaultState configuration.
        /// Called during Start() to ensure the UI begins in the correct state.
        /// </summary>
        private void SetDefaultState()
        {
            if (defaultState == DefaultState.Enabled)
            {
                canvasGroup.alpha = 1f;
                if (manageInteractableAndRaycasts)
                {
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                }
            }
            else
            {
                canvasGroup.alpha = 0f;
                if (manageInteractableAndRaycasts)
                {
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                }
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Smoothly fades the UI element in to full opacity (alpha = 1).
        /// Uses the configured fade duration and animation curve for a polished transition.
        /// Automatically stops any existing fade animation before starting the new one.
        /// </summary>
        /// <remarks>
        /// This method is safe to call multiple times and will interrupt any ongoing fade animation.
        /// The UI will become interactable when the fade begins (if setInteractableOnComplete is enabled).
        /// </remarks>
        public void FadeIn()
        {
            if (canvasGroup == null)
            {
                if (showDebugLogs)
                    Debug.LogError($"UiFader on {gameObject.name}: CanvasGroup component is missing! Cannot fade in.", this);
                return;
            }
            
            if (showDebugLogs)
                Debug.Log($"FadeIn called on {gameObject.name}", this);
            
            OnFadeInStarted?.Invoke();
            FadeTo(1f);
        }
        
        /// <summary>
        /// Smoothly fades the UI element out to complete transparency (alpha = 0).
        /// Uses the configured fade duration and animation curve for a polished transition.
        /// Automatically stops any existing fade animation before starting the new one.
        /// </summary>
        /// <remarks>
        /// This method is safe to call multiple times and will interrupt any ongoing fade animation.
        /// The UI will become non-interactable when the fade completes (if setInteractableOnComplete is enabled).
        /// </remarks>
        public void FadeOut()
        {
            if (canvasGroup == null)
            {
                if (showDebugLogs)
                    Debug.LogError($"UiFader on {gameObject.name}: CanvasGroup component is missing! Cannot fade out.", this);
                return;
            }
            
            if (showDebugLogs)
                Debug.Log($"FadeOut called on {gameObject.name}", this);
            
            OnFadeOutStarted?.Invoke();
            FadeTo(0f);
        }
        
        /// <summary>
        /// Smoothly fades the UI element to a specific alpha value.
        /// Provides precise control over the final opacity of the UI element.
        /// </summary>
        /// <param name="targetAlpha">The target alpha value between 0 (transparent) and 1 (opaque). Values outside this range will be clamped.</param>
        /// <remarks>
        /// This method is safe to call multiple times and will interrupt any ongoing fade animation.
        /// The fade will use the configured duration and animation curve for consistent behavior.
        /// </remarks>
        public void FadeTo(float targetAlpha)
        {
            if (canvasGroup == null)
            {
                if (showDebugLogs)
                    Debug.LogError($"UiFader on {gameObject.name}: CanvasGroup component is missing! Cannot fade to {targetAlpha}.", this);
                return;
            }
            
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
            }
            
            currentFadeCoroutine = StartCoroutine(FadeCoroutine(targetAlpha));
        }
        
        /// <summary>
        /// Instantly sets the alpha value and interactable state without any animation.
        /// Useful for immediate state changes or when you need to bypass the fade system.
        /// </summary>
        /// <param name="alpha">The alpha value between 0 (transparent) and 1 (opaque). Values outside this range will be clamped.</param>
        /// <param name="interactable">Whether the UI element should be interactable. Also controls blocksRaycasts. Only applied if manageInteractableAndRaycasts is enabled.</param>
        /// <remarks>
        /// This method will stop any ongoing fade animation and immediately apply the new state.
        /// Use this for instant UI state changes or when you need to reset the UI to a known state.
        /// The interactable parameter is only applied if manageInteractableAndRaycasts is true.
        /// </remarks>
        public void SetInstant(float alpha, bool interactable)
        {
            if (canvasGroup == null)
            {
                if (showDebugLogs)
                    Debug.LogError($"UiFader on {gameObject.name}: CanvasGroup component is missing! Cannot set instant state.", this);
                return;
            }
            
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
                currentFadeCoroutine = null;
            }
            
            canvasGroup.alpha = Mathf.Clamp01(alpha);
            if (manageInteractableAndRaycasts)
            {
                canvasGroup.interactable = interactable;
                canvasGroup.blocksRaycasts = interactable;
            }
            
            if (showDebugLogs)
                Debug.Log($"Instant set: alpha={alpha}, interactable={interactable} on {gameObject.name}", this);
        }
        
        /// <summary>
        /// Stops any currently running fade animation immediately.
        /// The UI element will remain in its current state when the fade is stopped.
        /// </summary>
        /// <remarks>
        /// This method is safe to call even when no fade animation is running.
        /// Useful for interrupting fades or ensuring clean state transitions.
        /// </remarks>
        public void StopFade()
        {
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
                currentFadeCoroutine = null;
                
                if (showDebugLogs)
                    Debug.Log($"Fade stopped on {gameObject.name}", this);
            }
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Coroutine that handles the smooth fade animation between alpha values.
        /// Uses the configured fade duration and animation curve for consistent behavior.
        /// </summary>
        /// <param name="targetAlpha">The target alpha value to fade to (0-1).</param>
        /// <returns>IEnumerator for coroutine execution.</returns>
        /// <remarks>
        /// This coroutine automatically manages interactable and blocksRaycasts properties
        /// based on the setInteractableOnComplete setting and final alpha value.
        /// </remarks>
        private IEnumerator FadeCoroutine(float targetAlpha)
        {
            float startAlpha = canvasGroup.alpha;
            float elapsedTime = 0f;
            
            // Enable interactable and blocksRaycasts at the start of fade in
            if (targetAlpha > startAlpha && manageInteractableAndRaycasts)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / fadeDuration;
                float curveValue = fadeCurve.Evaluate(normalizedTime);
                
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, curveValue);
                
                yield return null;
            }
            
            // Ensure we reach the exact target value
            canvasGroup.alpha = targetAlpha;
            
            // Set interactable and blocksRaycasts based on final state
            if (setInteractableOnComplete && manageInteractableAndRaycasts)
            {
                bool shouldBeInteractable = targetAlpha > 0.5f; // Consider anything above 50% as "visible"
                canvasGroup.interactable = shouldBeInteractable;
                canvasGroup.blocksRaycasts = shouldBeInteractable;
            }
            
            currentFadeCoroutine = null;
            
            if (showDebugLogs)
                Debug.Log($"Fade completed: alpha={targetAlpha}, interactable={canvasGroup.interactable} on {gameObject.name}", this);
        }
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Gets a value indicating whether a fade animation is currently running.
        /// Returns true if any fade operation is in progress, false otherwise.
        /// </summary>
        /// <value>True if currently fading, false if no fade animation is active.</value>
        public bool IsFading => currentFadeCoroutine != null;
        
        /// <summary>
        /// Gets the duration of the fade animation in seconds.
        /// </summary>
        /// <value>The fade duration between 0.01 and 10 seconds.</value>
        public float FadeDuration => fadeDuration;
        
        /// <summary>
        /// Gets the current alpha value of the UI element.
        /// Returns 0 if the CanvasGroup component is not available.
        /// </summary>
        /// <value>The current alpha value between 0 (transparent) and 1 (opaque).</value>
        public float CurrentAlpha => canvasGroup != null ? canvasGroup.alpha : 0f;
        
        /// <summary>
        /// Gets a value indicating whether the UI element is currently visible.
        /// Considers the UI visible when alpha is greater than 0.5 (50% opacity).
        /// </summary>
        /// <value>True if the UI is visible (alpha > 0.5), false otherwise.</value>
        public bool IsVisible => canvasGroup != null && canvasGroup.alpha > 0.5f;
        
        #endregion
        
        #region Editor
        
        #if UNITY_EDITOR
        /// <summary>
        /// Called when the component is first added to a GameObject or when Reset is called in the inspector.
        /// Automatically assigns the CanvasGroup component from the same GameObject.
        /// </summary>
        /// <remarks>
        /// This method is called automatically by Unity when the component is added via the Add Component menu.
        /// Since RequireComponent ensures a CanvasGroup exists, we can safely retrieve it here.
        /// </remarks>
        private void Reset()
        {
            // Auto-assign CanvasGroup when component is first added
            canvasGroup = GetComponent<CanvasGroup>();
        }
        
        /// <summary>
        /// Editor-only method that validates and updates values when properties are changed in the inspector.
        /// Ensures all values are within valid ranges and provides immediate feedback for invalid configurations.
        /// </summary>
        /// <remarks>
        /// This method is called automatically by Unity when values are changed in the inspector.
        /// It helps prevent invalid configurations and provides immediate visual feedback.
        /// </remarks>
        private void OnValidate()
        {
            // Ensure fade duration is within valid range
            fadeDuration = Mathf.Clamp(fadeDuration, 0.01f, 10f);
            
            // Validate animation curve
            if (fadeCurve == null || fadeCurve.keys.Length == 0)
            {
                fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            }
            
            // Ensure curve starts at 0 and ends at 1 for predictable behavior
            if (fadeCurve.keys.Length > 0)
            {
                var firstKey = fadeCurve.keys[0];
                var lastKey = fadeCurve.keys[fadeCurve.keys.Length - 1];
                
                if (firstKey.time != 0f || firstKey.value != 0f)
                {
                    fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                }
                else if (lastKey.time != 1f || lastKey.value != 1f)
                {
                    fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                }
            }
            
            // Auto-assign CanvasGroup if not set
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }
        #endif
        
        #endregion
    }
}
