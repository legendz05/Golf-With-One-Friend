using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerIndicator : MonoBehaviour
{
    private Slider slider;
    private bool changeDirection = false;
    public float powerValue;
    public float powerSpeed = 1f;


    void Start()
    {
        slider = FindAnyObjectByType(typeof(Slider)) as Slider;
        slider.value = 0;

        if (powerSpeed <= 0)
        {
            powerSpeed = 1f;
        }

        GameManager.instance.ResetPlayer -= ResetSlider;
        GameManager.instance.ResetPlayer += ResetSlider;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        CheckPowerStateInMatch();
    }

    public void CheckPowerStateInMatch()
    {
        if (GameManager.instance.matchState == MatchState.GolfBallPower)
        {
            slider.gameObject.SetActive(true);
            PowerValueChange();

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    Debug.Log("Touch detected in PowerIndicator");
                    powerValue = slider.value;
                    GameManager.instance.SetPower?.Invoke();
                }
            }
        }
        else
        {
            slider.gameObject.SetActive(false);
        }

    }

    public void PowerValueChange()
    {
        if (!changeDirection)
        {
            slider.value += powerSpeed * Time.deltaTime;
            if (slider.value >= slider.maxValue)
            {
                slider.value = slider.maxValue;
                changeDirection = true;
            }
        }
        else
        {
            slider.value -= powerSpeed * Time.deltaTime;
            if (slider.value <= slider.minValue)
            {
                slider.value = slider.minValue;
                changeDirection = false;
            }
        }
    }


    public void ResetSlider()
    {
        slider.value = 0;

        GameManager.instance.ResetPlayer -= ResetSlider;
        GameManager.instance.ResetPlayer += ResetSlider;
    }
}
