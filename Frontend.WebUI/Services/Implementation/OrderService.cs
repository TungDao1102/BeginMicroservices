﻿using Frontend.WebUI.Models.DTOs;
using Frontend.WebUI.Services.Interface;

namespace Frontend.WebUI.Services.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IBaseService _baseService;

        public OrderService(IBaseService baseService)
        {
            _baseService = baseService;
        }
        public async Task<ResponseDTO?> CreateOrder(CartDTO cartDTO)
        {
            return await _baseService.SendAsync(new RequestDTO
            {
                Url = Utility.Constants.OrderAPIBase + "/api/OrderAPI/CreateOrder",
                Data = cartDTO,
                ApiType = Utility.Constants.ApiType.POST
            });
        }

        public async Task<ResponseDTO?> CreateStripeSession(StripeRequestDTO stripeRequestDTO)
        {
            return await _baseService.SendAsync(new RequestDTO
            {
                Url = Utility.Constants.OrderAPIBase + "/api/OrderAPI/CreateStripeSession",
                Data = stripeRequestDTO,
                ApiType = Utility.Constants.ApiType.POST
            });
        }

        public async Task<ResponseDTO?> GetAllOrder(string? userId)
        {
            return await _baseService.SendAsync(new RequestDTO
            {
                Url = Utility.Constants.OrderAPIBase + "/api/OrderAPI/GetAllOrder/" + userId,
                ApiType = Utility.Constants.ApiType.GET
            });
        }

        public async Task<ResponseDTO?> GetOrderById(int id)
        {
            return await _baseService.SendAsync(new RequestDTO
            {
                Url = Utility.Constants.OrderAPIBase + "/api/OrderAPI/GetOrderById/" + id,
                ApiType = Utility.Constants.ApiType.GET
            });
        }

        public async Task<ResponseDTO?> UpdateOrderStatus(int orderId, string newStatus)
        {
            return await _baseService.SendAsync(new RequestDTO
            {
                Url = Utility.Constants.OrderAPIBase + "/api/OrderAPI/UpdateOrderStatus/" + orderId,
                Data = newStatus,
                ApiType = Utility.Constants.ApiType.POST
            });
        }

        public async Task<ResponseDTO?> ValidateStripeSession(int orderHeaderId)
        {
            return await _baseService.SendAsync(new RequestDTO
            {
                Url = Utility.Constants.OrderAPIBase + "/api/OrderAPI/ValidateStripeSession",
                Data = orderHeaderId,
                ApiType = Utility.Constants.ApiType.POST
            });
        }
    }
}
