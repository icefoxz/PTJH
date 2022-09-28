using System;
using System.Collections.Generic;
using System.Linq;
using Systems;
using Systems.Utls;
using UnityEngine;
using Utls;

/// <summary>
/// 游戏主体
/// </summary>
public class Game : UnitySingleton<Game>
{
    public static ResMgr ResMgr { get; private set; }
    public static FrameUpdater Updater { get; private set; }
    public void Init(ResMgr resMgr)
    {
        this.Log();
        ResMgr = resMgr;
        Updater = new FrameUpdater();
    }
    void Update()
    {
        Updater?.Update();
    }
}

public class FrameUpdater
{
    private List<(object obj, Action update)> MonoUpdates { get; } = new List<(object obj, Action update)>();

    public void RegMonoUpdate(object obj, Action update)
    {
        if (MonoUpdates.Any(m => m.obj == obj))
            throw new InvalidOperationException($"{obj} duplicated!");
        MonoUpdates.Add((obj, update));
    }

    public void RemoveMonoUpdate(object className)
    {
        var mono = MonoUpdates.FirstOrDefault(m => m.obj == className);
        if(mono == default)
            throw new InvalidOperationException($"{className} not found!");
        MonoUpdates.Remove(mono);
    }

    public void Update()
    {
        for (var i = 0; i < MonoUpdates.Count; i++)
        {
            var (_, updateMethod) = MonoUpdates[i];
            updateMethod();
        }
    }
}