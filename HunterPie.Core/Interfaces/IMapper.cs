﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunterPie.Core.Interfaces
{
    interface IMapper<T, K>
    {
        public K Map(T data);
    }
}
