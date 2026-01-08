using System;
using System.Collections.Generic;
using UnityEngine;



public class FSM
{
    private IState currentState;
    private Type currentType;
    private Dictionary<Type, IState> states = new Dictionary<Type, IState>();


    public Type GetCurrentType()
    {
        return currentType;
    }

    // ×¢²á×´Ì¬
    public void AddState<T>(IState state) where T : IState
    {
        Type type = typeof(T);
        if (!states.ContainsKey(type))
        {
            states.Add(type, state);
        }
    }

    // ÇÐ»»×´Ì¬
    public void ChangeState<T>() where T : IState
    {
        Type type = typeof(T);
        if (type == currentType) return;
        else currentType = type;
        if (states.TryGetValue(type, out IState newState))
        {
            currentState?.OnExit();
            currentState = newState;
            currentState.OnEnter();
        }
        else
        {
            Debug.LogError($"×´Ì¬ {type} Î´×¢²á£¡");
        }
    }

    // Ã¿Ö¡¸üÐÂ
    public void OnUpdate()
    {
        currentState?.OnUpdate();
    }
}