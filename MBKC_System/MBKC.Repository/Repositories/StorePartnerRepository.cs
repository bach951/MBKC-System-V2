﻿using MBKC.Repository.DBContext;
using MBKC.Repository.Enums;
using MBKC.Repository.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.Repositories
{
    public class StorePartnerRepository
    {
        private MBKCDbContext _dbContext;
        public StorePartnerRepository(MBKCDbContext dbContext)
        {
            this._dbContext = dbContext;
        }
        public async Task<StorePartner> GetStorePartnerByPartnerIdAndStoreIdAsync(int partnerId, int storeId)
        {
            try
            {
                return await this._dbContext.StorePartners.SingleOrDefaultAsync(s => s.PartnerId == partnerId && s.StoreId == storeId && s.Status != (int)StorePartnerEnum.Status.DEACTIVE);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
