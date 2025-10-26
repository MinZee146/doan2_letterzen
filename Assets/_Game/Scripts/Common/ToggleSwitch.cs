using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleSwitch : MonoBehaviour, IPointerClickHandler
{
    [Header("Slider Setup")]
    [SerializeField, Range(0, 1f)] private float _sliderValue = 1f;

    [Header("Animation")]
    [SerializeField, Range(0, 1f)] private float _animationDuration = 0.5f;
    [SerializeField] private AnimationCurve _slideEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Events")]
    [SerializeField] private UnityEvent _onToggleOn;
    [SerializeField] private UnityEvent _onToggleOff;

    [Header("BG")]
    [SerializeField] private GameObject _offBG, _onBG;

    public bool CurrentValue { get; private set; } = true;

    private Slider _slider;
    private Coroutine _animationCoroutine;

    public void OnPointerClick(PointerEventData eventData)
    {
        Toggle();
    }

    protected void OnValidate()
    {
        SetUpToggleComponents();

        if (_slider != null)
            _slider.value = _sliderValue;
    }

    private void SetUpToggleComponents()
    {
        if (_slider == null)
            _slider = GetComponent<Slider>();

        SetUpSliderComponents();
    }

    private void SetUpSliderComponents()
    {
        _slider.interactable = false;

        var sliderColor = _slider.colors;
        sliderColor.disabledColor = Color.white;
        _slider.colors = sliderColor;
        _slider.transition = Selectable.Transition.None;
    }

    private void Awake()
    {
        SetUpToggleComponents();
    }

    public void GetPrefs(bool state)
    {
        if (state) return;
        
        Toggle();
    }

    private void Toggle()
    {
        SetStateAndStartAnimation(!CurrentValue);
    }

    private void SetStateAndStartAnimation(bool state)
    {
        CurrentValue = state;

        if (CurrentValue)
            _onToggleOn?.Invoke();
        else
            _onToggleOff?.Invoke();

        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);

        _animationCoroutine = StartCoroutine(AnimationSlider());
    }

    private IEnumerator AnimationSlider()
    {
        var startValue = _slider.value;
        var endValue = CurrentValue ? 1 : 0;

        float time = 0;
        if (_animationDuration > 0)
        {
            while (time < _animationDuration)
            {
                time += Time.deltaTime;

                var lerpFactor = _slideEase.Evaluate(time / _animationDuration);
                _slider.value = _sliderValue = Mathf.Lerp(startValue, endValue, lerpFactor);

                yield return null;
            }
        }

        _sliderValue = _slider.value = endValue;
        _onBG.SetActive(CurrentValue);
        _offBG.SetActive(!CurrentValue);
    }
}
