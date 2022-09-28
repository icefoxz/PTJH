using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.WSA;

namespace ILAdaptors
{
    public interface IMonoUpdate
    {
        string ClassFullName { get; }
        void Update();
    }
}
