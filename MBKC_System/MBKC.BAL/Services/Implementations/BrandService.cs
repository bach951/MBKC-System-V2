﻿using AutoMapper;
using MBKC.BAL.DTOs;
using MBKC.BAL.DTOs.Brands;
using MBKC.BAL.DTOs.FireBase;
using MBKC.BAL.DTOs.Verifications;
using MBKC.BAL.Exceptions;
using MBKC.BAL.Services.Interfaces;
using MBKC.BAL.Utils;
using MBKC.DAL.DBContext;
using MBKC.DAL.Enums;
using MBKC.DAL.Infrastructures;
using MBKC.DAL.Models;
using MBKC.DAL.RedisModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MBKC.BAL.Repositories.Implementations
{

   
    public class BrandService : IBrandService
    {
        private UnitOfWork _unitOfWork;
        private IMapper _mapper;
        public BrandService(IUnitOfWork unitOfWork, IMapper mapper)

        {
            this._unitOfWork = (UnitOfWork)unitOfWork;
            this._mapper = mapper;
        }
        #region CreateBrand
        public async Task CreateBrandAsync(PostBrandRequest postBrandRequest, FireBaseImage fireBaseImage, Email emailSystem)
        {
            string logoId = "";
            bool uploaded = false;
            try
            {
                var checkDupplicatedEmail = await _unitOfWork.AccountRepository.GetAccountByEmailAsync(postBrandRequest.ManagerEmail);
                if (checkDupplicatedEmail != null)
                {
                    throw new BadRequestException("Email already exists in the system.");
                }
                var brandManagerRole = await _unitOfWork.RoleRepository.GetRoleById((int)RoleEnum.Role.BRAND_MANAGER);

                // Upload image to firebase
                FileStream fileStream = Utils.FileUtil.ConvertFormFileToStream(postBrandRequest.Logo);
                FileUtil.SetCredentials(fireBaseImage);
                Guid guild = Guid.NewGuid();
                logoId = guild.ToString();
                string urlImage = await Utils.FileUtil.UploadImageAsync(fileStream, "Brands", logoId);
                if (urlImage != null)
                {
                    uploaded = true;
                }
                // Create account
                string unEncryptedPassword = RandomNumberUtil.GenerateEightDigitNumber().ToString();
                var account = new Account
                {
                    Email = postBrandRequest.ManagerEmail,
                    Password = StringUtil.EncryptData(unEncryptedPassword),
                    Status = (int)(AccountEnum.Status.ACTIVE),
                    Role = brandManagerRole
                };
                await _unitOfWork.AccountRepository.CreateAccountAsync(account);

                // Create brand
                Brand brand = new Brand
                {
                    Name = postBrandRequest.Name,
                    Address = postBrandRequest.Address,
                    Logo = urlImage + $"&logoId={logoId}",
                    Status = (int)BrandEnum.Status.ACTIVE,
                };
                await _unitOfWork.BrandRepository.CreateBrandAsync(brand);

                // Create brand account
                BrandAccount brandAccount = new BrandAccount
                {
                    Account = account,
                    Brand = brand,
                };
                await _unitOfWork.BrandAccountRepository.CreateBrandAccount(brandAccount);
                await _unitOfWork.CommitAsync();

                //Send password to email of Brand Manager
                await EmailUtil.SendEmailAndPasswordToEmail(emailSystem, account.Email, EmailUtil.MessageRegisterAccount(emailSystem.SystemName, account.Email, unEncryptedPassword, brand.Name), "Brand Manager");
            }
            catch (BadRequestException ex)
            {
                string fieldName = "";
                if (ex.Message.Equals("Email already exists in the system"))
                {
                    fieldName = "Email";
                }
                if (ex.Message.Equals("Upload image to firebase failed."))
                {
                    fieldName = "Upload image.";
                }
                else if (ex.Message.Equals("Delete image failed."))
                {
                    fieldName = "Delete image.";
                }
                string error = ErrorUtil.GetErrorString(fieldName, ex.Message);
                throw new BadRequestException(error);
            }
            catch (Exception ex)
            {
                if (uploaded)
                {
                    await FileUtil.DeleteImageAsync(logoId, "Brands");
                }
                string error = ErrorUtil.GetErrorString("Exception", ex.Message);
                throw new Exception(error);
            }
        }
        #endregion

        #region Update Brand
        public async Task UpdateBrandAsync(int brandId, UpdateBrandRequest updateBrandRequest, FireBaseImage fireBaseImage)
        {
            string logoId = "";
            bool uploaded = false;
            try
            {
                if (brandId <= 0)
                {
                    throw new BadRequestException("Brand Id is not suitable for the system.");
                }
                var brand = await _unitOfWork.BrandRepository.GetBrandByIdAsync(brandId);
                var brands = await _unitOfWork.BrandRepository.GetBrandsAsync();
                if (brand == null)
                {
                    throw new NotFoundException("Brand Id does not exist in the system");
                }


                if (updateBrandRequest.Logo != null)
                {
                    //Delete image from database
                    FileUtil.SetCredentials(fireBaseImage);
                    await FileUtil.DeleteImageAsync(FileUtil.GetImageIdFromUrlImage(brand.Logo, "logoId"),"Brands");

                    // Upload image to firebase
                    FileStream fileStream = Utils.FileUtil.ConvertFormFileToStream(updateBrandRequest.Logo);
                    Guid guild = Guid.NewGuid();
                    logoId = guild.ToString();
                    var urlImage = await Utils.FileUtil.UploadImageAsync(fileStream, "Brands", logoId);
                    if (urlImage != null)
                    {
                        uploaded = true;
                    }
                    brand.Logo = urlImage + $"&logoId={logoId}";
                }

                brand.Address = updateBrandRequest.Address;
                brand.Name = updateBrandRequest.Name;

                if (updateBrandRequest.Status.ToLower().Equals(CategoryEnum.Status.ACTIVE.ToString().ToLower()))
                {
                    brand.Status = (int)CategoryEnum.Status.ACTIVE;
                }
                else if (updateBrandRequest.Status.ToLower().Equals(CategoryEnum.Status.INACTIVE.ToString().ToLower()))
                {
                    brand.Status = (int)CategoryEnum.Status.INACTIVE;
                }

                _unitOfWork.BrandRepository.UpdateBrand(brand);
                _unitOfWork.Commit();
            }
            catch (BadRequestException ex)
            {
                string fieldName = "";
                if (ex.Message.Equals("Brand Id is not suitable for the system."))
                {
                    fieldName = "Brand Id";
                }
                if (ex.Message.Equals("Upload image to firebase failed."))
                {
                    fieldName = "Upload image.";
                }
                else if (ex.Message.Equals("Delete image failed."))
                {
                    fieldName = "Delete image.";
                }
                string error = ErrorUtil.GetErrorString(fieldName, ex.Message);
                throw new BadRequestException(error);
            }
            catch (NotFoundException ex)
            {
                string fieldName = "";
                if (ex.Message.Equals("Brand Id does not exist in the system"))
                {
                    fieldName = "Brand Id";
                }
                string error = ErrorUtil.GetErrorString(fieldName, ex.Message);
                throw new NotFoundException(error);
            }
            catch (Exception ex)
            {
                if (uploaded)
                {
                    await FileUtil.DeleteImageAsync(logoId, "Brands");
                }
                string error = ErrorUtil.GetErrorString("Exception", ex.Message);
                throw new Exception(error);
            }
        }
        #endregion

        #region Get Brands
        public async Task<GetBrandsResponse> GetBrandsAsync(string? keySearchName, string? keyStatusFilter, int? pageNumber, int? pageSize)
        {
            try
            {
                var brands = new List<Brand>();
                var brandResponse = new List<GetBrandResponse>();
                if (pageNumber != null && pageNumber <= 0)
                {
                    throw new BadRequestException("Page number is required greater than 0.");
                }
                else if (pageNumber == null)
                {
                    pageNumber = 1;
                }

                if (pageSize != null && pageSize <= 0)
                {
                    throw new BadRequestException("Page size is required greater than 0.");
                }
                else if (pageSize == null)
                {
                    pageSize = 5;
                }

                int? keyStatus = null;
                if (keyStatusFilter != null)
                {
                    if (BrandEnum.Status.ACTIVE.ToString().ToLower().Equals(keyStatusFilter.ToLower()))
                    {
                        keyStatus = (int)BrandEnum.Status.ACTIVE;
                    }
                    else if (BrandEnum.Status.INACTIVE.ToString().ToLower().Equals(keyStatusFilter.ToLower()))
                    {
                        keyStatus = (int)BrandEnum.Status.INACTIVE;
                    }
                    else if (BrandEnum.Status.DEACTIVE.ToString().ToLower().Equals(keyStatusFilter.ToLower()))
                    {
                        keyStatus = (int)BrandEnum.Status.DEACTIVE;
                    }
                    else
                    {
                        throw new BadRequestException("Key Status Filter is not suitable (ACTIVE, INACTIVE or DEACTIVE) in the system.");
                    }
                }

                int numberItems = 0;
                if (keySearchName != null && StringUtil.IsUnicode(keySearchName))
                {
                    numberItems = await this._unitOfWork.BrandRepository.GetNumberBrandsAsync(keySearchName, null, keyStatus);
                    brands = await this._unitOfWork.BrandRepository.GetBrandsAsync(keySearchName, null, keyStatus, pageSize.Value, pageNumber.Value);
                }
                else if (keySearchName != null && StringUtil.IsUnicode(keySearchName) == false)
                {
                    numberItems = await this._unitOfWork.BrandRepository.GetNumberBrandsAsync(null, keySearchName, keyStatus);
                    brands = await this._unitOfWork.BrandRepository.GetBrandsAsync(null, keySearchName, keyStatus, pageSize.Value, pageNumber.Value);
                }
                else if (keySearchName == null)
                {
                    numberItems = await this._unitOfWork.BrandRepository.GetNumberBrandsAsync(null, null, keyStatus);
                    brands = await this._unitOfWork.BrandRepository.GetBrandsAsync(null, null, keyStatus, pageSize.Value, pageNumber.Value);
                }

                this._mapper.Map(brands, brandResponse);

                foreach (var brand in brandResponse)
                {
                    if (brand.Status.Equals(((int)BrandEnum.Status.ACTIVE).ToString()))
                    {
                        brand.Status = BrandEnum.Status.ACTIVE.ToString().ToUpper()[0] + BrandEnum.Status.ACTIVE.ToString().ToLower().Substring(1);
                    }
                    else if (brand.Status.Equals(((int)BrandEnum.Status.INACTIVE).ToString()))
                    {
                        brand.Status = BrandEnum.Status.INACTIVE.ToString().ToUpper()[0] + BrandEnum.Status.INACTIVE.ToString().ToLower().Substring(1);
                    }
                    else if (brand.Status.Equals(((int)BrandEnum.Status.DEACTIVE).ToString()))
                    {
                        brand.Status = BrandEnum.Status.DEACTIVE.ToString().ToUpper()[0] + BrandEnum.Status.DEACTIVE.ToString().ToLower().Substring(1);
                    }
                }

                if (brandResponse == null || brandResponse.Count == 0)
                {
                    return new GetBrandsResponse()
                    {
                        Brands = brandResponse,
                        TotalItems = 0,
                        TotalPages = 0,
                    };
                }

                int totalPages = (int)((numberItems + pageSize) / pageSize); ;
                return new GetBrandsResponse()
                {
                    Brands = brandResponse,
                    TotalItems = numberItems,
                    TotalPages = totalPages,
                };
            }
            catch (BadRequestException ex)
            {
                string fieldName = "";
                if (ex.Message.Equals("Key Status Filter is not suitable (ACTIVE, INACTIVE or DEACTIVE) in the system."))
                {
                    fieldName = "Key Status Filter";
                }
                if (ex.Message.Equals("Page number is required greater than 0."))
                {
                    fieldName = "Page number";
                }
                if (ex.Message.Equals("Page size is required greater than 0."))
                {
                    fieldName = "Page size";
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
        #endregion

        #region Get Brand By Id
        public async Task<GetBrandResponse> GetBrandByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new BadRequestException("Brand Id is not suitable for the system.");
                }
                var brand = await _unitOfWork.BrandRepository.GetBrandByIdAsync(id);
                if (brand == null)
                {
                    throw new NotFoundException("Brand Id does not exist in the system.");
                }

                var brandResponse = this._mapper.Map<GetBrandResponse>(brand);
                if (brandResponse.Status.Equals(((int)BrandEnum.Status.ACTIVE).ToString()))
                {
                    brandResponse.Status = BrandEnum.Status.ACTIVE.ToString().ToUpper()[0] + BrandEnum.Status.ACTIVE.ToString().ToLower().Substring(1);
                }
                else if (brandResponse.Status.Equals(((int)BrandEnum.Status.INACTIVE).ToString()))
                {
                    brandResponse.Status = BrandEnum.Status.INACTIVE.ToString().ToUpper()[0] + BrandEnum.Status.INACTIVE.ToString().ToLower().Substring(1);
                }
                else if (brandResponse.Status.Equals(((int)BrandEnum.Status.DEACTIVE).ToString()))
                {
                    brandResponse.Status = BrandEnum.Status.DEACTIVE.ToString().ToUpper()[0] + BrandEnum.Status.DEACTIVE.ToString().ToLower().Substring(1);
                }

                return brandResponse;
            }
            catch (BadRequestException ex)
            {
                string fieldName = "";
                if (ex.Message.Equals("Brand Id is not suitable for the system."))
                {
                    fieldName = "Brand Id";
                }
                string error = ErrorUtil.GetErrorString(fieldName, ex.Message);
                throw new NotFoundException(error);
            }
            catch (NotFoundException ex)
            {
                string fieldName = "";
                if (ex.Message.Equals("Brand Id does not exist in the system."))
                {
                    fieldName = "Brand Id";
                }
                string error = ErrorUtil.GetErrorString(fieldName, ex.Message);
                throw new NotFoundException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorUtil.GetErrorString("Exception", ex.Message);
                throw new Exception(error);
            }
        }
        #endregion

        #region Deactive Brand By Id
        public async Task DeActiveBrandByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new BadRequestException("Brand Id is not suitable for the system.");
                }
                var brand = await _unitOfWork.BrandRepository.GetBrandByIdAsync(id);

                if (brand == null)
                {
                    throw new NotFoundException("Brand does not exist in the system");
                }

                // Deactive status of brand
                brand.Status = (int)BrandEnum.Status.DEACTIVE;

                // Deactive Manager Account of brand
                foreach (var brandAccount in brand.BrandAccounts)
                {
                    brandAccount.Account.Status = (int)(AccountEnum.Status.DEACTIVE);
                }

                // Deactive products belong to brand
                if (brand.Products.Any())
                {
                    foreach (var product in brand.Products)
                    {
                        product.Status = (int)ProductEnum.Status.DEACTIVE;
                    }
                }

                // Deactive categories belong to brand
                if (brand.Categories.Any())
                {
                    foreach (var category in brand.Categories)
                    {
                        category.Status = (int)CategoryEnum.Status.DEACTIVE;
                    }
                    // Deactive extra categories belong to brand
                    foreach (var category in brand.Categories)
                    {
                        foreach (var extra in category.ExtraCategoryProductCategories)
                        {
                            extra.Status = (int)ExtraCategoryEnum.Status.DEACTIVE;
                        }
                    }
                }

                // Change status brand's store to NOT_RENT
                if (brand.Stores.Any())
                {
                    foreach (var store in brand.Stores)
                    {
                        store.Status = (int)StoreEnum.Status.INACTIVE;
                    }
                }
                _unitOfWork.BrandRepository.UpdateBrand(brand);
                _unitOfWork.Commit();
            }
            catch (BadRequestException ex)
            {
                string fieldName = "";
                if (ex.Message.Equals("Brand Id is not suitable for the system."))
                {
                    fieldName = "Brand Id";
                }
                string error = ErrorUtil.GetErrorString(fieldName, ex.Message);
                throw new NotFoundException(error);
            }
            catch (NotFoundException ex)
            {
                string fileName = "";
                if (ex.Message.Equals("Brand Id does not exist in the system"))
                {
                    fileName = "Brand Id";
                }
                string error = ErrorUtil.GetErrorString(fileName, ex.Message);
                throw new NotFoundException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorUtil.GetErrorString("Error", ex.Message);
                throw new Exception(error);
            }
        }
        #endregion
    }
}