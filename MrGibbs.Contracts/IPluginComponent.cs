﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MrGibbs.Contracts
{
    public interface IPluginComponent:IDisposable
    {
        IPlugin Plugin { get; }
    }
}