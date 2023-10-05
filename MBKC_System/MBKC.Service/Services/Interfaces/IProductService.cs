﻿using MBKC.Service.DTOs.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Service.Services.Interfaces
{
    public interface IProductService
    {
        public Task CreateProductAsync(CreateProductRequest createProductRequest, IEnumerable<Claim> claims);
        public Task<GetProductsResponse> GetProductsAsync(string? searchName, int? currentPage, int? itemsPerPage,
            string? productType, bool? isGetAll, int? idCategory, int? idStore, IEnumerable<Claim> claims);
        public Task<GetProductResponse> GetProductAsync(int idProduct, IEnumerable<Claim> claims);
        public Task DeleteProductAsync(int idProduct, IEnumerable<Claim> claims);
        public Task UpdateProductStatusAsync(int idProduct, UpdateProductStatusRequest updateProductStatusRequest, IEnumerable<Claim> claims);
        public Task UpdateProductAsync(int idProduct, UpdateProductRequest updateProductRequest, IEnumerable<Claim> claims);
    }
}
