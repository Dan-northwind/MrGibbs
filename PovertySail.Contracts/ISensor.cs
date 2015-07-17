﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PovertySail.Models;

namespace PovertySail.Contracts
{
    public interface ISensor : IPluginComponent
    {
        void Update(State state);
    }
}
