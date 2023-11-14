﻿using MBKC.Service.DTOs.DashBoards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Service.Services.Interfaces
{
    public interface IDashBoardService
    {
        public Task<GetStoreDashBoardResponse> GetStoreDashBoardAsync(IEnumerable<Claim> claims);
        public Task<GetBrandDashBoardResponse> GetBrandDashBoardAsync(IEnumerable<Claim> claims, GetSearchDateDashBoardRequest getSearchDateDashBoardRequest);
    }
}
