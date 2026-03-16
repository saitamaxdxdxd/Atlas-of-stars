using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Retropolis.Input
{
    /// <summary>
    /// Capa de abstracción de input. Soporta touch (mobile) y mouse (PC/Editor).
    /// Detecta tap, swipe y hold. Agregar este componente a un GameObject persistente.
    /// </summary>
    public class InputHandler : MonoBehaviour, IInputHandler
    {
        [SerializeField] private float _swipeThreshold = 50f;   // pixels mínimos para considerar swipe
        [SerializeField] private float _tapMaxDuration = 0.2f;  // segundos máximos para considerar tap
        [SerializeField] private float _holdDuration = 0.5f;    // segundos para activar hold

        public event Action<Vector2> OnTap;
        public event Action<Vector2, Vector2> OnSwipe;
        public event Action<Vector2> OnHoldStart;
        public event Action OnHoldEnd;
        public event Action<Vector2> OnPointerDown;
        public event Action<Vector2> OnPointerUp;

        private Vector2 _pointerDownPos;
        private float _pointerDownTime;
        private bool _isHolding;
        private Coroutine _holdCoroutine;

        // ─── Lifecycle ────────────────────────────────────────────────────────

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            Touch.onFingerDown += HandleFingerDown;
            Touch.onFingerUp += HandleFingerUp;
        }

        private void OnDisable()
        {
            Touch.onFingerDown -= HandleFingerDown;
            Touch.onFingerUp -= HandleFingerUp;
            EnhancedTouchSupport.Disable();
        }

        private void Update()
        {
            // Mouse solo en editor y standalone — en mobile Mouse.current es null
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
                BeginPointer(mouse.position.ReadValue());
            else if (mouse.leftButton.wasReleasedThisFrame)
                EndPointer(mouse.position.ReadValue());
        }

        // ─── Touch ────────────────────────────────────────────────────────────

        private void HandleFingerDown(Finger finger)
        {
            if (finger.index != 0) return; // solo primer dedo
            BeginPointer(finger.screenPosition);
        }

        private void HandleFingerUp(Finger finger)
        {
            if (finger.index != 0) return;
            EndPointer(finger.screenPosition);
        }

        // ─── Lógica de gestos ─────────────────────────────────────────────────

        private void BeginPointer(Vector2 pos)
        {
            _pointerDownPos = pos;
            _pointerDownTime = Time.time;
            _isHolding = false;

            OnPointerDown?.Invoke(pos);

            if (_holdCoroutine != null)
                StopCoroutine(_holdCoroutine);
            _holdCoroutine = StartCoroutine(HoldRoutine(pos));
        }

        private void EndPointer(Vector2 pos)
        {
            if (_holdCoroutine != null)
            {
                StopCoroutine(_holdCoroutine);
                _holdCoroutine = null;
            }

            OnPointerUp?.Invoke(pos);

            if (_isHolding)
            {
                _isHolding = false;
                OnHoldEnd?.Invoke();
                return;
            }

            Vector2 delta = pos - _pointerDownPos;
            float distance = delta.magnitude;
            float duration = Time.time - _pointerDownTime;

            if (distance >= _swipeThreshold)
            {
                OnSwipe?.Invoke(delta.normalized, delta);
            }
            else if (duration <= _tapMaxDuration)
            {
                OnTap?.Invoke(pos);
            }
        }

        private IEnumerator HoldRoutine(Vector2 pos)
        {
            yield return new WaitForSeconds(_holdDuration);
            _isHolding = true;
            OnHoldStart?.Invoke(pos);
        }
    }
}
