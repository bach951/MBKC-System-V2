﻿using MBKC.DAL.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.DAL.DAOs
{
    public class CashierDAO
    {
        private MBKCDbContext _dbContext;
        public CashierDAO(MBKCDbContext dbContext)
        {
            this._dbContext = dbContext;
        }
    }
}
