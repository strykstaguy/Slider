using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SettingUIElement<T> : MonoBehaviour
{
    [SerializeField] protected UnityEvent<T> onValueLoad;
    [SerializeField] protected SettingRetriever settingRetriever;

    protected virtual void OnEnable()
    {
        try 
        {
            T settingValue = (T)settingRetriever.ReadSettingValue();
            onValueLoad?.Invoke(settingValue);
        } catch (InvalidCastException)
        {
            Debug.LogError($"[{gameObject.name}] Tried to cast type '{settingRetriever.ReadSettingValue().GetType()}' to type '{typeof(T)}'");
        }
        
    }

    public virtual void WriteSettingValue(T settingValue)
    {
        settingRetriever.WriteSettingValue(settingValue);
    }
}
