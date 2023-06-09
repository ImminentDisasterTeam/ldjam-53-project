﻿using System;
using _Game.Scripts.UI.Base;
using DG.Tweening;
using GeneralUtils;
using GeneralUtils.UI;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.UI {
    public class TargetTimer : UIElement {
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private ProgressBar _bar;
        [SerializeField] private Vector2 _edgeOffset;
        [SerializeField] private RectTransform _arrow;

        private UpdatedValue<float> _timerValue;
        private Transform _target;
        private bool _loaded;
        private AudioSource _soundController;
        private Tween _animation;

        public void Load(float initialValue, UpdatedValue<float> timerValue, Transform target) {
            _loaded = true;
            _bar.Load(0, initialValue);
            _timerValue = timerValue;
            _timerValue.Subscribe(OnValueChanged, true);
            _target = target;
        }

        public void Update() {
            if (State.Value != EState.Shown && State.Value != EState.Showing) {
                return;
            }

            if (_timerValue.Value > 5f && _timerValue.Value < 6f) {
                if (!_soundController)
                    _soundController = SoundController.Instance.PlaySound("timer-tick", 0.7f);
            } 
            else {
                if (_soundController && _timerValue.Value > 6f) {
                    _soundController.Stop();
                    _soundController = null;
                }
            } // kappa


            var viewportPoint = Locator.Instance.MainCamera.WorldToViewportPoint(_target.position);
            var flippedPoint = (Vector2) (viewportPoint.z > 0
                ? viewportPoint
                : (viewportPoint - Vector3.one) * -1);
            var canvasSize = ((RectTransform) Locator.Instance.Canvas.transform).sizeDelta;
            var rect = (RectTransform) transform;
            if (rect.anchorMin != rect.anchorMax && rect.anchorMax != Vector2.one * 0.5f) {
                throw new Exception("I don't wanna handle this");
            }

            var timerCenterPosition = AnchoredToCentered(rect, canvasSize,(flippedPoint - rect.anchorMax) * canvasSize);
            var maxPos = canvasSize * 0.5f - rect.sizeDelta * 0.5f - _edgeOffset;
            var centerX = Mathf.Sign(timerCenterPosition.x) * Mathf.Clamp(Mathf.Abs(timerCenterPosition.x), 0, maxPos.x);
            var centerY = Mathf.Sign(timerCenterPosition.y) * Mathf.Clamp(Mathf.Abs(timerCenterPosition.y), 0, maxPos.y);
            var clampedCenterPosition = new Vector2(centerX, centerY);
            rect.anchoredPosition = CenteredToAnchored(rect, canvasSize, clampedCenterPosition);

            var showArrow = /*flippedPoint.x < 0 || flippedPoint.x > 1 || flippedPoint.y < 0 || flippedPoint.y > 1;*/ timerCenterPosition != clampedCenterPosition;
            _arrow.gameObject.SetActive(showArrow);

            var pivotPoint = timerCenterPosition + rect.sizeDelta * (rect.pivot - Vector2.one * 0.5f);
            var direction = (pivotPoint - clampedCenterPosition).normalized;
            var angle = Vector2.SignedAngle(Vector2.up, direction);
            _arrow.rotation = Quaternion.Euler(0, 0, angle);
        }

        private static Vector2 AnchoredToCentered(RectTransform rect, Vector2 parentSize, Vector2 position) {
            return position
                   + (rect.anchorMax - Vector2.one * 0.5f) * parentSize
                   + (Vector2.one * 0.5f - rect.pivot) * rect.sizeDelta;
        }

        private static Vector2 CenteredToAnchored(RectTransform rect, Vector2 parentSize, Vector2 position) {
            return position
                   - (rect.anchorMax - Vector2.one * 0.5f) * parentSize
                   - (Vector2.one * 0.5f - rect.pivot) * rect.sizeDelta;
        }

        public override void Clear() {
            if (!_loaded) {
                return;
            }

            _loaded = false;
            _timerValue.Unsubscribe(OnValueChanged);
        }

        private void OnValueChanged(float value) {
            if (State.Value == EState.Hiding) {
                return;
            }

            _bar.CurrentValue = value;
            _label.text = $"{Mathf.Round(value)}";
        }

        protected override void PerformShow(Action onDone = null) {
            _animation?.Complete(true);

            const float duration = 0.2f;
            transform.localScale = Vector3.zero;
            _animation = transform
                .DOScale(Vector3.one, duration)
                .SetEase(Ease.OutBack)
                .OnComplete(() => onDone?.Invoke());
        }

        protected override void PerformHide(Action onDone = null) {
            _animation?.Complete(true);

            const float duration = 0.2f;
            _animation = transform
                .DOScale(Vector3.zero, duration)
                .SetEase(Ease.InSine)
                .OnComplete(() => onDone?.Invoke());
        }
    }
}