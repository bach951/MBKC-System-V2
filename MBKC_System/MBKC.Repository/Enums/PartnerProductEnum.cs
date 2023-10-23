﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.Enums
{
    public static class PartnerProductEnum
    {
        public enum Status
        {
            INACTIVE = 0,
            ACTIVE = 1,
            DEACTIVE = 2
        }

        public enum KeySort
        {
            ASC,
            DESC
        }
    }
}