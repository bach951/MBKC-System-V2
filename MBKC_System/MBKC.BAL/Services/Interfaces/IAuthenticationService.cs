﻿using MBKC.BAL.DTOs.Accounts;
using MBKC.BAL.DTOs.AccountTokens;
using MBKC.BAL.DTOs.JWTs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.BAL.Services.Interfaces
{
    public interface IAuthenticationService
    {
        public Task<AccountResponse> LoginAsync(AccountRequest accountRequest, JWTAuth jwtAuth);
        public Task<AccountTokenResponse> ReGenerateTokensAsync(AccountTokenRequest accountTokenRequest, JWTAuth jwtAuth);
        public Task ChangePasswordAsync(ResetPasswordRequest resetPassword);
    }
}