using System;
using UnityEngine;



/// <summary>
/// Performance optimized DebugLogger Logger that only logs messages when conditional "DEBUG" argument is provided in build settings.
/// </summary>
public static class DebugLogger
{
    private const string ScriptingDefineSymbol = "Enable_Debug_Logging";

    [System.Diagnostics.Conditional(ScriptingDefineSymbol)]
    public static void Log(object message)
    {
        Debug.Log(message);
    }

    [System.Diagnostics.Conditional(ScriptingDefineSymbol)]
    public static void LogWarning(object message)
    {
        Debug.LogWarning(message);
    }

    [System.Diagnostics.Conditional(ScriptingDefineSymbol)]
    public static void LogError(object message)
    {
        Debug.LogError(message);
    }

    [System.Diagnostics.Conditional(ScriptingDefineSymbol)]
    public static void LogError(object message, bool logCondition)
    {
        if (logCondition)
        {
            Debug.LogError(message);
        }
    }
}