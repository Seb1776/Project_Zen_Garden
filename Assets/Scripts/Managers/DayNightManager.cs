using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightManager : MonoBehaviour
{
    public static DayNightManager instance;
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private LightingPreset Preset;
    [SerializeField] private Light[] otherLights;
    [SerializeField] private float otherLightMultiplier;
    [SerializeField] private ParticleSystem poolFog;
    public bool day = true;
    public float timeOfDay;
    public bool toDay, toNight;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    void Update()
    {
        if (toNight)
        {
            timeOfDay += Time.deltaTime;
            UpdateLighting(timeOfDay / 12f);

            if (timeOfDay >= 12)
            {
                toNight = false;
                timeOfDay = 12;
            }
        }

        if (toDay)
        {
            timeOfDay -= Time.deltaTime;
            UpdateLighting(timeOfDay / 12f);

            if (timeOfDay <= 0)
            {
                toDay = false;
                timeOfDay = 0;
            }
        }
    }

    public void TriggerNight(bool pool = false)
    {
        timeOfDay = 0f;
        toNight = true;
        day = false;

        if (pool)
            poolFog.Play();
    }

    public void TriggerDay(bool pool = false)
    {
        timeOfDay = 12f;
        toDay = true;
        day = true;

        if (pool)
            poolFog.Stop();
    }

    private void UpdateLighting(float timePercent)
    {
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.fogColor = Preset.FogColor.Evaluate(timePercent);
        RenderSettings.skybox.SetColor("_Tint", Preset.AmbientColor.Evaluate(timePercent));
        
        if (otherLights.Length > 0)
            foreach (Light l in otherLights)
                l.intensity = timePercent * otherLightMultiplier;

        if (DirectionalLight != null)
        {
            DirectionalLight.color = Preset.DirectionalColor.Evaluate(timePercent);

            DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0));
        }

    }

    private void OnValidate()
    {
        if (DirectionalLight != null)
            return;


        if (RenderSettings.sun != null)
        {
            DirectionalLight = RenderSettings.sun;
        }

        else
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    DirectionalLight = light;
                    return;
                }
            }
        }
    }
}