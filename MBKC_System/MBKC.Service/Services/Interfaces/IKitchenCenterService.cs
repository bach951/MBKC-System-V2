﻿using MBKC.Service.DTOs.KitchenCenters;

namespace MBKC.Service.Services.Interfaces
{
    public interface IKitchenCenterService
    {
        public Task CreateKitchenCenterAsync(CreateKitchenCenterRequest newKitchenCenter);
        public Task<GetKitchenCenterResponse> GetKitchenCenterAsync(int kitchenCenterId);
        public Task<GetKitchenCentersResponse> GetKitchenCentersAsync(int? itemsPerPage, int? currentPage, string? searchValue, bool? isGetAll);
        public Task UpdateKitchenCenterAsync(int kitchenCenterId, UpdateKitchenCenterRequest updatedKitchenCenter);
        public Task DeleteKitchenCenterAsync(int kitchenCenterId);
        public Task UpdateKitchenCenterStatusAsync(int kitchenCenterId, UpdateKitchenCenterStatusRequest updateKitchenCenterStatusRequest);
    }
}