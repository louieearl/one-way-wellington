﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeController : MonoBehaviour
{
    public static TimeController Instance;
    public float timeOWW;
    public TextMeshProUGUI timeDisplay;

    // Start is called before the first frame update
    void Start()
    {
        timeOWW = 0;
        if (Instance == null) Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        timeOWW += Time.deltaTime;
        if (timeOWW >= 1440) timeOWW = 0;
        timeDisplay.text = ((int) timeOWW / 60).ToString("D2") + ":" + ((int) (timeOWW % 60)).ToString("D2");

    }

    public void PauseTime()
    {
        Time.timeScale = 0;
    }

    public void ResumeTime()
    {
        Time.timeScale = 1;
    }

    public void DoubleTime()
    {
        Time.timeScale = 2;
    }

    public void QuadrupleTime()
    {
        Time.timeScale = 4;
    }
}