﻿using AutoMapper;
using MBKC.BAL.DTOs.Accounts;
using MBKC.BAL.DTOs.AccountTokens;
using MBKC.BAL.DTOs.JWTs;
using MBKC.BAL.DTOs.Verifications;
using MBKC.BAL.Exceptions;
using MBKC.BAL.Services.Interfaces;
using MBKC.BAL.Utils;
using MBKC.DAL.Enums;
using MBKC.DAL.Infrastructures;
using MBKC.DAL.Models;
using MBKC.DAL.RedisModels;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.BAL.Services.Implementations
{
    public class AuthenticationService : IAuthenticationService
    {
        private UnitOfWork _unitOfWork;
        private IMapper _mapper;
        public AuthenticationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this._unitOfWork = (UnitOfWork)unitOfWork;
            this._mapper = mapper;
        }

        public async Task<AccountResponse> LoginAsync(AccountRequest accountRequest, JWTAuth jwtAuth)
        {
            try
            {
                Account existedAccount = await this._unitOfWork.AccountRepository.GetActiveAccountAsync(accountRequest.Email);
                if (existedAccount == null)
                {
                    throw new NotFoundException("Email does not exist in the system.");
                }
                if (existedAccount.Password.Equals(accountRequest.Password) == false)
                {
                    throw new BadRequestException("Email or Password is invalid.");
                }
                AccountResponse accountResponse = this._mapper.Map<AccountResponse>(existedAccount);
                accountResponse = await GenerateTokenAsync(accountResponse, jwtAuth);
                return accountResponse;
            }
            catch (NotFoundException ex)
            {
                string error = ErrorUtil.GetErrorString("Email", ex.Message);
                throw new NotFoundException(error);
            }
            catch (BadRequestException ex)
            {
                string fieldName = "";
                if (ex.Message.Equals("Email or Password is invalid."))
                {
                    fieldName = "Login Failed";
                }
                string error = ErrorUtil.GetErrorString(fieldName, ex.Message);
                throw new BadRequestException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorUtil.GetErrorString("Exception", ex.Message);
                throw new Exception(error);
            }
        }

        public async Task<AccountTokenResponse> ReGenerateTokensAsync(AccountTokenRequest accountTokenRequest, JWTAuth jwtAuth)
        {
            try
            {
                var jwtTokenHandler = new JwtSecurityTokenHandler();
                var secretKeyBytes = Encoding.UTF8.GetBytes(jwtAuth.Key);
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
                    ValidateLifetime = false,
                    ClockSkew = TimeSpan.Zero
                };


                //Check 1: Access token is valid format
                var tokenVerification = jwtTokenHandler.ValidateToken(accountTokenRequest.AccessToken, tokenValidationParameters, out var validatedToken);

                //Check 2: Check Alg
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase);
                    if (result == false)
                    {
                        throw new BadRequestException("Access Token is invalid.");
                    }
                }

                //Check 3: check accessToken expried?
                var utcExpiredDate = long.Parse(tokenVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                var expiredDate = DateUtil.ConvertUnixTimeToDateTime(utcExpiredDate);
                if (expiredDate > DateTime.UtcNow)
                {
                    throw new BadRequestException("Access token has not yet expired.");
                }

                //Check 4: Check refresh token exist in Redis Db
                string accountId = tokenVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sid).Value;
                AccountTokenRedisModel accountTokenRedisModel = await this._unitOfWork.AccountTokenRedisRepository.GetAccountToken(accountId);
                if (accountTokenRedisModel == null)
                {
                    throw new NotFoundException("You do not has the authentication tokens in the system.");
                }

                if (accountTokenRedisModel.RefreshToken.Equals(accountTokenRequest.RefreshToken) == false)
                {
                    throw new NotFoundException("Refresh token does not exist.");
                }

                //Check 5: Id of refresh token == id of access token
                var jwtId = tokenVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if (accountTokenRedisModel.JWTId.Equals(jwtId) == false)
                {
                    throw new BadRequestException("Your access token does not match the registered access token before.");
                }

                //Check 6: refresh token is expired
                if (accountTokenRedisModel.ExpiredDate < DateTime.UtcNow)
                {
                    throw new BadRequestException("Your refresh token expired now.");
                }

                Account existedAccount = await this._unitOfWork.AccountRepository.GetAccountAsync(accountId);
                AccountResponse accountResponse = this._mapper.Map<AccountResponse>(existedAccount);
                accountResponse = await GenerateTokenAsync(accountResponse, jwtAuth);
                return accountResponse.Tokens;
            }
            catch (NotFoundException ex)
            {
                string error = ErrorUtil.GetErrorString("Authentication Tokens", ex.Message);
                throw new NotFoundException(error);
            }
            catch (BadRequestException ex)
            {
                string error = ErrorUtil.GetErrorString("Authentication Tokens", ex.Message);
                throw new BadRequestException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorUtil.GetErrorString("Exception", ex.Message);
                throw new Exception(error);
            }
        }

        public async Task ChangePasswordAsync(ResetPasswordRequest resetPassword)
        {
            try
            {
                Account existedAccount = await this._unitOfWork.AccountRepository.GetAccountAsync(resetPassword.Email);
                if (existedAccount == null)
                {
                    throw new NotFoundException("Email does not exist in the system.");
                }
                EmailVerificationRedisModel emailVerificationRedisModel = await this._unitOfWork.EmailVerificationRedisRepository.GetEmailVerificationAsync(resetPassword.Email);
                if (emailVerificationRedisModel == null)
                {
                    throw new BadRequestException("Email has not been previously authenticated.");
                }
                if (emailVerificationRedisModel.IsVerified == Convert.ToBoolean((int)EmailVerificationEnum.Status.NOT_VERIFIRED))
                {
                    throw new BadRequestException("Email is not yet authenticated with the previously sent OTP code.");
                }
                existedAccount.Password = resetPassword.NewPassword;
                this._unitOfWork.AccountRepository.UpdateAccount(existedAccount);
                await this._unitOfWork.CommitAsync();
                await this._unitOfWork.EmailVerificationRedisRepository.DeleteEmailVerificationAsync(emailVerificationRedisModel);
            }
            catch (NotFoundException ex)
            {
                string error = ErrorUtil.GetErrorString("Email", ex.Message);
                throw new NotFoundException(error);
            }
            catch (BadRequestException ex)
            {
                string fieldName = "";
                if (ex.Message.Equals("Email has not been previously authenticated.")
                    || ex.Message.Equals("Email is not yet authenticated."))
                {
                    fieldName = "Email";
                }
                string error = ErrorUtil.GetErrorString(fieldName, ex.Message);
                throw new BadRequestException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorUtil.GetErrorString("Exception", ex.Message);
                throw new Exception(error);
            }
        }

        private async Task<AccountResponse> GenerateTokenAsync(AccountResponse accountResponse, JWTAuth jwtAuth)
        {
            try
            {
                var jwtTokenHandler = new JwtSecurityTokenHandler();
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuth.Key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);

                var tokenDescription = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, accountResponse.Email),
                        new Claim(JwtRegisteredClaimNames.Email, accountResponse.Email),
                        new Claim(JwtRegisteredClaimNames.Sid, accountResponse.AccountId.ToString()),
                        new Claim("Role", accountResponse.RoleName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = credentials
                };

                var token = jwtTokenHandler.CreateToken(tokenDescription);
                string accessToken = jwtTokenHandler.WriteToken(token);
                string refreshToken = GenerateRefreshToken();

                AccountToken accountToken = new AccountToken()
                {
                    JWTId = token.Id,
                    RefreshToken = refreshToken,
                    ExpiredDate = DateTime.UtcNow.AddDays(5),
                    AccountId = accountResponse.AccountId
                };

                AccountTokenRedisModel accountTokenRedisModel = this._mapper.Map<AccountTokenRedisModel>(accountToken);
                await this._unitOfWork.AccountTokenRedisRepository.AddAccountToken(accountTokenRedisModel);

                AccountTokenResponse tokens = new AccountTokenResponse()
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };
                accountResponse.Tokens = tokens;
                return accountResponse;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private string GenerateRefreshToken()
        {
            var random = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);
                return Convert.ToBase64String(random);
            }
        }

    }
}