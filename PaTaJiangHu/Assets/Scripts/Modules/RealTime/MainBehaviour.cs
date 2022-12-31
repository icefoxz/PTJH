using System;
using System.Collections.Generic;
using System.Linq;
using BattleM;
using UnityEngine;

namespace RealTime
{
    /// <summary>
    /// 主体
    /// </summary>
    public class Principal
    {
        private ResourceManager ResourceMgr { get; }
        private ProductionManager ProductionMgr { get; }
        private MemberManager MemberMgr { get; }
        private AssignmentManager AssignmentMgr { get; }

        public Principal(ResourceManager resourceMgr, ProductionManager productionMgr, MemberManager memberMgr, AssignmentManager assignmentMgr)
        {
            ResourceMgr = resourceMgr;
            ProductionMgr = productionMgr;
            MemberMgr = memberMgr;
            AssignmentMgr = assignmentMgr;
        }

        public void CreateProduction(Production.Config cfg) => ProductionMgr.Add(cfg);
        public void RemoveProduction(Production pro)
        {
            var assignments = AssignmentMgr.List.Where(m => m.Production == pro).ToArray();
            foreach (var assignment in assignments) AssignmentMgr.Remove(assignment);
            ProductionMgr.Remove(pro);
        }

        public void CreateMember(Member.Config cfg) => MemberMgr.Add(cfg);
        public void RemoveMember(Member mem)
        {
            var assignments = AssignmentMgr.List.Where(m => m.Member == mem).ToArray();
            foreach (var assignment in assignments) AssignmentMgr.Remove(assignment);
            MemberMgr.Remove(mem);
        }
    }

    /// <summary>
    /// 委派管理器
    /// </summary>
    public class AssignmentManager
    {
        public IReadOnlyList<Assignment> List => _list;
        private List<Assignment> _list = new List<Assignment>();
        public bool Remove(Assignment item) => _list.Remove(item);

        public void Add(Assignment.Config cfg,Member mem,Production pro)
        {
            _list.Add(new Assignment(cfg, mem, pro));
        }
    }

    /// <summary>
    /// 产出管理器
    /// </summary>
    public class ProductionManager
    {
        public IReadOnlyList<Production> List => _list;
        private List<Production> _list = new List<Production>();
        public bool Remove(Production item) => _list.Remove(item);

        public void Add(Production.Config cfg)
        {
            _list.Add(new Production(cfg));
        }
    }
    /// <summary>
    /// 小人管理器
    /// </summary>
    public class MemberManager 
    {
        public IReadOnlyList<Member> List => _list;
        private List<Member> _list = new List<Member>();
        public void Add(Member.Config cfg) => _list.Add(new Member(cfg));
        public bool Remove(Member item) => _list.Remove(item);
    }

    /// <summary>
    /// 资源管理器
    /// </summary>
    public class ResourceManager
    {
        public int Silver { get; private set; }

        public void Add(int value)
        {
            Silver += value;
        }
        public void SetZero() => Silver = 0;
    }

    /// <summary>
    /// 委派实例
    /// </summary>
    public class Assignment
    {
        public Member Member { get; }
        public Production Production { get; }
        public long AssignTime { get; }
        public long LastUpdate { get; }

        public Assignment(Config c,Member member,Production production)
        {
            throw new NotImplementedException();
        }
        public class Config
        {

        }
    }

    /// <summary>
    /// 产出单位
    /// </summary>
    public class Production
    {
        public string Name { get; }
        public bool IsActive { get; }
        public long LastActiveTicks { get; }
        public long NextTriggerTicks { get; }

        public int MaxMembers { get; }
        public int MaxProductions { get; }
        public int MaxFoodSupply { get; }
        public int Cost { get; }


        public Production(Config c)
        {
            throw new NotImplementedException();
        }
        public class Config
        {
        
        }
    }

    /// <summary>
    /// 执行单位(游戏角色)
    /// </summary>
    public class Member
    {
        public string Name { get; }
        public int Strength { get; }
        public int Agility { get; }
        public ConValue Hp { get; }
        public ConValue Food { get; }

        public Member(Config c)
        {
            Name = c.Name;
            Strength = c.Strength;
            Agility = c.Agility;
            Hp = new ConValue(c.Hp);
            Food = new ConValue(c.Food);
        }

        [Serializable] public class Config
        {
            [SerializeField] private string _name;
            [SerializeField] private int _strength;
            [SerializeField] private int _agility;
            [SerializeField] private int _hp;
            [SerializeField] private int _food;
            public string Name => _name;
            public int Strength => _strength;
            public int Agility => _agility;
            public int Hp => _hp;
            public int Food => _food;
        }
    }
}