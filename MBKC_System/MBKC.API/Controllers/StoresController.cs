﻿using FluentValidation;
using FluentValidation.Results;
using MBKC.API.Constants;
using MBKC.Service.Authorization;
using MBKC.Service.DTOs.Stores;
using MBKC.Service.Errors;
using MBKC.Service.Exceptions;
using MBKC.Service.Services.Interfaces;
using MBKC.Service.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace MBKC.API.Controllers
{
    [ApiController]
    public class StoresController : ControllerBase
    {
        private IStoreService _storeService;
        private IValidator<RegisterStoreRequest> _registerStoreValidator;
        private IValidator<UpdateStoreRequest> _updateStoreValidator;
        private IValidator<UpdateStoreStatusRequest> _updateStoreStatusValidator;
        private IValidator<ConfirmStoreRegistrationRequest> _confirmStoreRegistrationValidator;
        public StoresController(IStoreService storeService, IValidator<RegisterStoreRequest> registerStoreValidator, 
            IValidator<UpdateStoreRequest> updateStoreValidator, IValidator<UpdateStoreStatusRequest> updateStoreStatusValidator,
            IValidator<ConfirmStoreRegistrationRequest> confirmStoreRegistrationValidator)
        {
            this._storeService = storeService;
            this._registerStoreValidator = registerStoreValidator;
            this._updateStoreValidator = updateStoreValidator;
            this._updateStoreStatusValidator = updateStoreStatusValidator;
            this._confirmStoreRegistrationValidator = confirmStoreRegistrationValidator;
        }

        #region Get Stores
        /// <summary>
        /// Get stores in the system.
        /// </summary>
        /// <param name="itemsPerPage">The number of items that will display on a page.</param>
        /// <param name="currentPage">The position of the page.</param>
        /// <param name="searchValue">The search value about store's name.</param>
        /// <param name="idBrand">The brand's id</param>
        /// <param name="idKitchenCenter">The kitchen center's id</param>
        /// <returns>
        /// An object contains NumberItems, TotalPage, a list of stores.
        /// </returns>
        /// <remarks>
        ///     Sample request:
        ///
        ///         GET 
        ///         itemsPerPage = 5
        ///         currentPage = 1
        ///         searchValue = KFC Bình Thạnh
        ///         idBrand = 1
        ///         idKitchenCenter = 1
        ///         status = ACTIVE | INACTIVE | 
        /// </remarks>
        /// <response code="200">Get a list of stores Successfully.</response>
        /// <response code="400">Some Error about request data and logic data.</response>
        /// <response code="500">Some Error about the system.</response>
        /// <exception cref="BadRequestException">Throw Error about request data and logic bussiness.</exception>
        /// <exception cref="Exception">Throw Error about the system.</exception>
        [ProducesResponseType(typeof(GetStoresResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status500InternalServerError)]
        [Produces(MediaTypeConstant.ApplicationJson)]
        [PermissionAuthorize(PermissionAuthorizeConstant.MBKCAdmin, PermissionAuthorizeConstant.BrandManager, PermissionAuthorizeConstant.KitchenCenterManager)]
        [HttpGet(APIEndPointConstant.Store.StoresEndpoint)]
        public async Task<IActionResult> GetStoresAync([FromQuery] int? itemsPerPage, [FromQuery] int? currentPage, [FromQuery] string? searchValue,
            [FromQuery] int? idBrand, [FromQuery] int? idKitchenCenter, [FromQuery] string? status)
        {
            IEnumerable<Claim> claims = Request.HttpContext.User.Claims;
            GetStoresResponse stores = await this._storeService.GetStoresAsync(searchValue, currentPage, itemsPerPage, idBrand, idKitchenCenter, status, claims);
            return Ok(stores);
        }
        #endregion

        #region Get Store
        /// <summary>
        /// Get a specific store by store id.
        /// </summary>
        /// <param name="id">The store's id.</param>
        /// <returns>
        /// An object contains the store's information.
        /// </returns>
        /// <remarks>
        ///     Sample request:
        ///
        ///         GET 
        ///         id = 1
        /// </remarks>
        /// <response code="200">Get a specific store by id Successfully.</response>
        /// <response code="400">Some Error about request data and logic data.</response>
        /// <response code="404">Some Error about request data not found.</response>
        /// <response code="500">Some Error about the system.</response>
        /// <exception cref="BadRequestException">Throw Error about request data and logic bussiness.</exception>
        /// <exception cref="NotFoundException">Throw Error about request data that are not found.</exception>
        /// <exception cref="Exception">Throw Error about the system.</exception>
        [ProducesResponseType(typeof(GetStoreResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status500InternalServerError)]
        [Produces(MediaTypeConstant.ApplicationJson)]
        [PermissionAuthorize(PermissionAuthorizeConstant.MBKCAdmin, PermissionAuthorizeConstant.KitchenCenterManager, PermissionAuthorizeConstant.BrandManager)]
        [HttpGet(APIEndPointConstant.Store.StoreEndpoint)]
        public async Task<IActionResult> GetStoreAsync([FromRoute]int id)
        {
            IEnumerable<Claim> claims = Request.HttpContext.User.Claims;
            GetStoreResponse store = await this._storeService.GetStoreAsync(id, claims);
            return Ok(store);
        }
        #endregion

        #region Get profile
        /// <summary>
        /// Get a store profile
        /// </summary>
        /// <returns>
        /// An object contains the store's information.
        /// </returns>
        /// <response code="200">Get store profile Successfully.</response>
        /// <response code="500">Some Error about the system.</response>
        /// <exception cref="Exception">Throw Error about the system.</exception>
        [ProducesResponseType(typeof(GetStoreResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status500InternalServerError)]
        [Produces(MediaTypeConstant.ApplicationJson)]
        [PermissionAuthorize(PermissionAuthorizeConstant.StoreManager)]
        [HttpGet(APIEndPointConstant.Store.StoreProfileEndpoint)]
        public async Task<IActionResult> GetStoreProfileAsync()
        {
            IEnumerable<Claim> claims = Request.HttpContext.User.Claims;
            GetStoreResponse getStoreResponse = await this._storeService.GetStoreAsync(claims);
            return Ok(getStoreResponse);
        }
        #endregion

        #region Register New Store
        /// <summary>
        /// Register new store.
        /// </summary>
        /// <param name="storeRequest">A store object contains registered information.</param>
        /// <returns>
        /// A success message about registering store information.
        /// </returns>
        /// <remarks>
        ///     Sample request:
        ///
        ///         POST 
        ///         {
        ///             "Name": "Kitchen Center Example"
        ///             "Logo": [Imgage File]
        ///             "KitchenCenterId": 1
        ///             "BrandId": 1
        ///             "StoreManagerEmail": "abc@example.com"
        ///         }
        /// </remarks>
        /// <response code="200">Registered new store successfully.</response>
        /// <response code="400">Some Error about request data and logic data.</response>
        /// <response code="404">Some Error about request data not found.</response>
        /// <response code="500">Some Error about the system.</response>
        /// <exception cref="BadRequestException">Throw Error about request data and logic bussiness.</exception>
        /// <exception cref="NotFoundException">Throw Error about request data that are not found.</exception>
        /// <exception cref="Exception">Throw Error about the system.</exception>
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status500InternalServerError)]
        [Consumes(MediaTypeConstant.MultipartFormData)]
        [Produces(MediaTypeConstant.ApplicationJson)]
        [PermissionAuthorize(PermissionAuthorizeConstant.BrandManager)]
        [HttpPost(APIEndPointConstant.Store.StoresEndpoint)]
        public async Task<IActionResult> PostRegisterStoreAsync([FromForm] RegisterStoreRequest storeRequest)
        {
            ValidationResult validationResult = await this._registerStoreValidator.ValidateAsync(storeRequest);
            if(validationResult.IsValid == false)
            {
                string errors = ErrorUtil.GetErrorsString(validationResult);
                throw new BadRequestException(errors);
            }
            IEnumerable<Claim> claims = Request.HttpContext.User.Claims;
            await this._storeService.CreateStoreAsync(storeRequest, claims);
            return Ok(new
            {
                Message = MessageConstant.StoreMessage.RegisteredNewStoreSuccessfully
            });
        }
        #endregion

        #region Update Existed Store of A Brand
        /// <summary>
        /// Update information of an existed store of a brand.
        /// </summary>
        /// <param name="id">The store's id.</param>
        /// <param name="updateStoreRequest">An store object contains updated information.</param>
        /// <returns>
        /// A success message about updating store information.  
        /// </returns>
        /// <remarks>
        ///     Sample request:
        ///
        ///         PUT 
        ///         idStore = 1
        ///         
        ///         {
        ///             "Name": "Kitchen Center Example"
        ///             "Status": "Active | Inactive"
        ///             "Logo": [Imgage File]
        ///             "StoreManagerEmail": "abc@example.com"
        ///         }
        /// </remarks>
        /// <response code="200">Updated store information successfully.</response>
        /// <response code="400">Some Error about request data and logic data.</response>
        /// <response code="404">Some Error about request data not found.</response>
        /// <response code="500">Some Error about the system.</response>
        /// <exception cref="BadRequestException">Throw Error about request data and logic bussiness.</exception>
        /// <exception cref="NotFoundException">Throw Error about request data that are not found.</exception>
        /// <exception cref="Exception">Throw Error about the system.</exception>
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status500InternalServerError)]
        [Consumes(MediaTypeConstant.MultipartFormData)]
        [Produces(MediaTypeConstant.ApplicationJson)]
        [PermissionAuthorize(PermissionAuthorizeConstant.MBKCAdmin, PermissionAuthorizeConstant.BrandManager)]
        [HttpPut(APIEndPointConstant.Store.StoreEndpoint)]
        public async Task<IActionResult> PutUpdateStoreAsync([FromRoute]int id, [FromForm]UpdateStoreRequest updateStoreRequest)
        {
            ValidationResult validationResult = await this._updateStoreValidator.ValidateAsync(updateStoreRequest);
            if(validationResult.IsValid == false)
            {
                string errors = ErrorUtil.GetErrorsString(validationResult);
                throw new BadRequestException(errors);
            }
            IEnumerable<Claim> claims = Request.HttpContext.User.Claims;
            await this._storeService.UpdateStoreAsync(id, updateStoreRequest, claims);
            return Ok(new
            {
                Message = MessageConstant.StoreMessage.UpdatedStoreInformationSuccessfully
            });
        }
        #endregion

        #region Update status of existed store
        /// <summary>
        /// Update status of an existed store of a brand.
        /// </summary>
        /// <param name="id">The store's id.</param>
        /// <param name="updateStoreStatusRequest">An store object contains updated status.</param>
        /// <returns>
        /// A success message about updating store status.  
        /// </returns>
        /// <remarks>
        ///     Sample request:
        ///
        ///         PUT 
        ///         id = 1
        ///         
        ///         {
        ///             "Status": "Active | Inactive"
        ///         }
        /// </remarks>
        /// <response code="200">Updated store status successfully.</response>
        /// <response code="400">Some Error about request data and logic data.</response>
        /// <response code="404">Some Error about request data not found.</response>
        /// <response code="500">Some Error about the system.</response>
        /// <exception cref="BadRequestException">Throw Error about request data and logic bussiness.</exception>
        /// <exception cref="NotFoundException">Throw Error about request data that are not found.</exception>
        /// <exception cref="Exception">Throw Error about the system.</exception>
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status500InternalServerError)]
        [Consumes(MediaTypeConstant.ApplicationJson)]
        [Produces(MediaTypeConstant.ApplicationJson)]
        [PermissionAuthorize(PermissionAuthorizeConstant.MBKCAdmin, PermissionAuthorizeConstant.BrandManager)]
        [HttpPut(APIEndPointConstant.Store.UpdateingStatusStore)]
        public async Task<IActionResult> PutUpdateStoreStatusAsync([FromRoute]int id, [FromBody] UpdateStoreStatusRequest updateStoreStatusRequest)
        {
            ValidationResult validationResult = await this._updateStoreStatusValidator.ValidateAsync(updateStoreStatusRequest);
            if(validationResult.IsValid == false)
            {
                string errors = ErrorUtil.GetErrorsString(validationResult);
                throw new BadRequestException(errors);
            }
            IEnumerable<Claim> claims = Request.HttpContext.User.Claims;
            await this._storeService.UpdateStoreStatusAsync(id, updateStoreStatusRequest, claims);
            return Ok(new
            {
                Message = MessageConstant.StoreMessage.UpdatedStoreStatusSuccessfully
            });
        }
        #endregion

        #region Delete Existed Store
        /// <summary>
        /// Delete an existed store.
        /// </summary>
        /// <param name="id">The store's id</param>
        /// <returns>
        /// A success message about deleting exsited store.
        /// </returns>
        /// <remarks>
        ///     Sample request:
        ///
        ///         DELETE 
        ///         idStore = 1
        /// </remarks>
        /// <response code="200">Deleted a store successfully.</response>
        /// <response code="400">Some Error about request data and logic data.</response>
        /// <response code="404">Some Error about request data not found.</response>
        /// <response code="500">Some Error about the system.</response>
        /// <exception cref="BadRequestException">Throw Error about request data and logic bussiness.</exception>
        /// <exception cref="NotFoundException">Throw Error about request data that are not found.</exception>
        /// <exception cref="Exception">Throw Error about the system.</exception>
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status500InternalServerError)]
        [Produces(MediaTypeConstant.ApplicationJson)]
        [PermissionAuthorize(PermissionAuthorizeConstant.MBKCAdmin, PermissionAuthorizeConstant.BrandManager)]
        [HttpDelete(APIEndPointConstant.Store.StoreEndpoint)]
        public async Task<IActionResult> DeleteStoreAsync([FromRoute] int id)
        {
            IEnumerable<Claim> claims = Request.HttpContext.User.Claims;
            await this._storeService.DeleteStoreAsync(id, claims);
            return Ok(new
            {
                Message = MessageConstant.StoreMessage.DeletedStoreSuccessfully
            });
        }
        #endregion

        #region Confirm store registration
        /// <summary>
        /// Confirm a store registration.
        /// </summary>
        /// <param name="id">The store's id</param>
        /// <param name="confirmStoreRegistrationRequest">An object contains status property and rejected reason (if have)</param>
        /// <returns>
        /// A success message about confirming a store registration.
        /// </returns>
        /// <remarks>
        ///     Sample request:
        ///
        ///         PUT 
        ///         id = 1
        ///         
        ///         {
        ///             "status": "ACTIVE | REJECTED",
        ///             "rejectedReason": "Kitchen center does not have enough space."
        ///         }
        /// </remarks>
        /// <response code="200">Confirmed a store registration successfully.</response>
        /// <response code="400">Some Error about request data and logic data.</response>
        /// <response code="404">Some Error about request data not found.</response>
        /// <response code="500">Some Error about the system.</response>
        /// <exception cref="BadRequestException">Throw Error about request data and logic bussiness.</exception>
        /// <exception cref="NotFoundException">Throw Error about request data that are not found.</exception>
        /// <exception cref="Exception">Throw Error about the system.</exception>
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status500InternalServerError)]
        [Consumes(MediaTypeConstant.ApplicationJson)]
        [Produces(MediaTypeConstant.ApplicationJson)]
        [PermissionAuthorize(PermissionAuthorizeConstant.MBKCAdmin)]
        [HttpPut(APIEndPointConstant.Store.ConfirmRegistrationStore)]
        public async Task<IActionResult> PutConfirmRegistrationStore([FromRoute]int id, [FromBody]ConfirmStoreRegistrationRequest confirmStoreRegistrationRequest)
        {
            ValidationResult validationResult = await this._confirmStoreRegistrationValidator.ValidateAsync(confirmStoreRegistrationRequest);
            if(validationResult.IsValid == false)
            {
                string errors = ErrorUtil.GetErrorsString(validationResult);
                throw new BadRequestException(errors);
            }
            await this._storeService.ConfirmStoreRegistrationAsync(id, confirmStoreRegistrationRequest);
            return Ok(new
            {
                Message = MessageConstant.StoreMessage.ConfirmedStoreRegistrationSuccessfully
            });
        }
        #endregion
    }
}
