﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services.CouponAPI.Data;
using Services.CouponAPI.Models;
using Services.CouponAPI.Models.DTOs;


namespace Services.CouponAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class CouponAPIController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly IMapper _mapper;
		private ResponseDTO _response;
		public CouponAPIController(AppDbContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
			_response = new ResponseDTO();
		}
		[HttpGet("GetAllCoupon")]
		public async Task<ActionResult<ResponseDTO>> GetAll()
		{
			var coupons = await _context.Coupons.ToListAsync();
			_response.Result = _mapper.Map<List<CouponDTO>>(coupons);
			if (coupons is null)
			{
				_response.IsSuccess = false;
				_response.Message = "Cannot find out the coupon";
			}
			return _response;
		}

		[HttpGet("GetCouponById/{id:int}")]
		public async Task<ActionResult<ResponseDTO>> GetCouponById(int id)
		{
			var coupon = await _context.Coupons.SingleOrDefaultAsync(x => x.CouponId == id);
			_response.Result = _mapper.Map<CouponDTO>(coupon);
			if (coupon is null)
			{
				_response.IsSuccess = false;
				_response.Message = "Cannot find out the coupon";
			}
			return _response;
		}

		[HttpGet("GetByCode/{code}")]
		public async Task<ActionResult<ResponseDTO>> GetByCode(string code)
		{
			var coupon = await _context.Coupons.SingleOrDefaultAsync(x => x.CouponCode.ToLower() == code.ToLower());
            if (coupon is null)
            {
                _response.IsSuccess = false;
                _response.Message = "Cannot find out the coupon";
            }
            _response.Result = _mapper.Map<CouponDTO>(coupon);
			return _response;
		}
		[HttpPost("CreateCoupon")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ResponseDTO>> CreateCoupon([FromBody] CouponDTO couponDTO)
		{
			var coupon = _mapper.Map<Coupon>(couponDTO);
			await _context.Coupons.AddAsync(coupon);
			await _context.SaveChangesAsync();
			
			// create stripe coupon
            var options = new Stripe.CouponCreateOptions
            {
                AmountOff = (long)couponDTO.DiscountAmount*100,
				Name = couponDTO.CouponCode,
				Currency = "USD",
				Id = couponDTO.CouponCode
            };
            var service = new Stripe.CouponService();
            await service.CreateAsync(options);

            _response.Result = _mapper.Map<CouponDTO>(coupon);
			return _response;
		}

		[HttpPut("UpdateCoupon/{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ResponseDTO>> UpdateCoupon([FromRoute] int id, [FromBody] CouponDTO couponDTO)
		{
			var coupon = await _context.Coupons.FindAsync(id);
			if(id != couponDTO.CouponId)
			{
				return BadRequest();
			}
			if (coupon is null)
			{
				_response.IsSuccess = false;
				_response.Message = "Cannot find out the coupon";
			}
			else
			{
				_mapper.Map(couponDTO, coupon);
				_context.Update(coupon);
				await _context.SaveChangesAsync();
				_response.Result = _mapper.Map<CouponDTO>(coupon);
			}
			return _response;
		}

		[HttpDelete("DeleteCoupon/{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ResponseDTO>> DeleteCoupon(int id)
		{
			var coupon = await _context.Coupons.FindAsync(id);
			if (coupon is null)
			{
				_response.IsSuccess = false;
				_response.Message = "Cannot find out the coupon";
			}
			else
			{
				_context.Remove(coupon);
				await _context.SaveChangesAsync();
                
                var service = new Stripe.CouponService();
                await service.DeleteAsync(coupon.CouponCode);
            }
			return _response;
		}
	}
}
