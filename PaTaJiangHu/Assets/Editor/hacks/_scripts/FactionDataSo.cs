using System;
using System.Collections.Generic;
using _GameClient.Models;
using Server.Configs.Battles;
using UnityEngine;

// 门派数据So
internal class FactionDataSo : ScriptableObject, IFaction
{
    [SerializeField] private int 银两;
    [SerializeField] private int 元宝;
    [SerializeField] private int 行动令;
    [SerializeField] private Dictionary<string, Dizi> 弟子;
    [SerializeField] private IReadOnlyList<IWeapon> 武器;
    [SerializeField] private IReadOnlyList<IArmor> 防具;

    public int Silver => 银两;
    public int YuanBao => 元宝;
    public int ActionLing => 行动令;
    public int ActionLingMax { get; } = 100;
    private Dictionary<string, Dizi> DiziMap => 弟子;
    public IReadOnlyList<IWeapon> Weapons => 武器;
    public IReadOnlyList<IArmor> Armors => 防具;
    public IReadOnlyList<Dizi> DiziList { get; }

    public Dizi GetDizi(string guid)
    {
        throw new NotImplementedException();
    }

    private event Action<IFaction> OnFactionUpdate;//注册唯一的门派信息更新消息事件
    public void RegUpdater(Action<IFaction> onUpdateAction) => OnFactionUpdate = onUpdateAction;
}