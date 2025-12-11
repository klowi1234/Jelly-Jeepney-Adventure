using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PowerUpHUD : MonoBehaviour
{
    public static PowerUpHUD instance;

    [Header("Power-Up Sliders")]
    public Slider clingSlider;
    public Slider inflateSlider;
    public Slider speedBoostSlider;

    // Keep track of active coroutines per power-up
    private Dictionary<Slider, Coroutine> activeCoroutines = new Dictionary<Slider, Coroutine>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        clingSlider.gameObject.SetActive(false);
        inflateSlider.gameObject.SetActive(false);
        speedBoostSlider.gameObject.SetActive(false);
    }

    public void StartClingTimer(float duration)
    {
        StartTimer(clingSlider, duration);
    }

    public void StartInflateTimer(float duration)
    {
        StartTimer(inflateSlider, duration);
    }

    public void StartSpeedBoostTimer(float duration)
    {
        StartTimer(speedBoostSlider, duration);
    }

    // --- Generic timer logic ---
    private void StartTimer(Slider slider, float duration)
    {
        // If a timer is already running, stop it
        if (activeCoroutines.ContainsKey(slider) && activeCoroutines[slider] != null)
            StopCoroutine(activeCoroutines[slider]);

        // Start new timer
        Coroutine c = StartCoroutine(UpdateSlider(slider, duration));
        activeCoroutines[slider] = c;
    }

    private IEnumerator UpdateSlider(Slider slider, float duration)
    {
        slider.maxValue = duration;
        slider.value = duration;
        slider.gameObject.SetActive(true);

        float timer = duration;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            slider.value = timer;
            yield return null;
        }

        slider.gameObject.SetActive(false);
        activeCoroutines[slider] = null;
    }
}
