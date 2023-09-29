using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Repository;
using MagicVilla_CouponAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MagicVilla_CouponAPI.Endpoints
{
    public static class CouponEndpoints
    {
        public static void ConfigureCouponEndpoint(this WebApplication app)
        {
            //GET ALL
            app.MapGet("api/coupon", GetAllCoupon)
                .WithName("GetCoupons")
                .Produces<IEnumerable<ApiResponse>>((int)HttpStatusCode.OK)
                .Produces((int)HttpStatusCode.Unauthorized);
                //.RequireAuthorization("AdminOnly");

            //GET
            app.MapGet("api/coupon/{id:int}", GetCoupon)
                .WithName("GetCoupon")
                .Produces<ApiResponse>((int)HttpStatusCode.OK)
                .Produces((int)HttpStatusCode.Unauthorized)
                .AddEndpointFilter(async(context, next) =>
                {
                    var id = context.GetArgument<int>(1);
                    if (id == 0) return Results.BadRequest("Cannot have 0 in id.");

                    //action to do before execution of endpoint
                    Console.WriteLine("before 1st filter");

                    var result = await next(context);

                    //action to do after execution of endpoint
                    Console.WriteLine("after 1st filter");

                    return result;
                })
                .AddEndpointFilter(async (context, next) =>
                {
                    //action to do before execution of endpoint
                    Console.WriteLine("before 2nd filter");

                    var result = await next(context);

                    //action to do after execution of endpoint
                    Console.WriteLine("after 2nd filter");

                    return result;
                });

            //INSERT
            app.MapPost("api/coupon", CreateCoupon)
                .WithName("CreateCoupon")
                .Accepts<CouponCreateDto>("application/json")
                .Produces<ApiResponse>((int)HttpStatusCode.Created)
                .Produces((int)HttpStatusCode.BadRequest)
                .Produces((int)HttpStatusCode.Unauthorized);

            //UPDATE
            app.MapPut("api/coupon", UpdateCoupon)
                .WithName("UpdateCoupon")
                .Accepts<CouponUpdateDto>("application/json")
                .Produces<ApiResponse>((int)HttpStatusCode.OK)
                .Produces((int)HttpStatusCode.BadRequest)
                .Produces((int)HttpStatusCode.Unauthorized);

            //DELETE
            app.MapDelete("api/coupon/{id:int}", DeleteCoupon)
                .WithName("DeleteCoupon")
                .Produces<ApiResponse>((int)HttpStatusCode.OK)
                .Produces((int)HttpStatusCode.BadRequest)
                .Produces((int)HttpStatusCode.Unauthorized);
        }

        private async static Task<IResult> GetAllCoupon(ICouponRepository _couponRepo, ILogger<Program> _logger)
        {
            _logger.Log(LogLevel.Information, "Getting all coupons.");

            ApiResponse response = new();
            //response.Result = CouponStore.CouponList;
            response.Result = await _couponRepo.GetAllAsync();
            response.StatusCode = HttpStatusCode.OK;
            response.IsSuccess = true;

            return Results.Ok(response);
        }

        //[Authorize]
        private async static Task<IResult> GetCoupon(ICouponRepository _couponRepo, int id)
        {
            Console.WriteLine("endpoint executed");
            ApiResponse response = new();
            //response.Result = CouponStore.CouponList.FirstOrDefault(x => x.Id == id);
            response.Result = await _couponRepo.GetAsync(id);
            response.StatusCode = HttpStatusCode.OK;
            response.IsSuccess = true;

            return Results.Ok(response);
        }

        //[Authorize]
        private async static Task<IResult> CreateCoupon(ICouponRepository _couponRepo,
            IMapper _mapper,
            IValidator<CouponCreateDto> _validation,
            [FromBody] CouponCreateDto couponCreateDto)
        {
            ApiResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

            var validationResult = await _validation.ValidateAsync(couponCreateDto);

            if (!validationResult.IsValid)
            {
                response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
                return Results.BadRequest(response);
            }

            if (string.IsNullOrEmpty(couponCreateDto.Name))
            {
                response.ErrorMessages.Add("Invalid Coupon Name.");
                return Results.BadRequest(response);
            }

            if (await _couponRepo.GetAsync(couponCreateDto.Name) != null)
            {
                response.ErrorMessages.Add("Coupon Name already exist.");
                return Results.BadRequest(response);
            }

            //if(CouponStore.CouponList.FirstOrDefault(x => x.Name.ToLower() == couponCreateDto.Name.ToLower()) != null)
            //{
            //    response.ErrorMessages.Add("Coupon Name already exist.");
            //    return Results.BadRequest(response);
            //}

            Coupon coupon = _mapper.Map<Coupon>(couponCreateDto);

            //coupon.Id = CouponStore.CouponList.OrderByDescending(x => x.Id).FirstOrDefault().Id + 1;
            //CouponStore.CouponList.Add(coupon);

            await _couponRepo.CreateAsync(coupon);
            await _couponRepo.SaveAsync();

            CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);

            response.StatusCode = HttpStatusCode.Created;
            response.IsSuccess = true;
            response.Result = couponDTO;

            return Results.Ok(response);
            //return Results.CreatedAtRoute("GetCoupon", new { id = coupon.Id }, response.Result);
            //return Results.Created($"/api/coupon/{coupon.Id}", coupon);
        }

        //[Authorize]
        private async static Task<IResult> UpdateCoupon(ICouponRepository _couponRepo,
            IMapper _mapper,
            IValidator<CouponUpdateDto> _validation,
            [FromBody] CouponUpdateDto couponUpdateDto)
        {
            ApiResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

            var validationResult = await _validation.ValidateAsync(couponUpdateDto);

            if(await _couponRepo.GetAsync(couponUpdateDto.Id) == null)
            {
                response.ErrorMessages.Add("No id exists.");
                return Results.BadRequest(response);
            }

            if (!validationResult.IsValid)
            {
                response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
                return Results.BadRequest(response);
            }

            if (string.IsNullOrEmpty(couponUpdateDto.Name))
            {
                response.ErrorMessages.Add("Invalid Coupon Name.");
                return Results.BadRequest(response);
            }

            if (await _couponRepo.GetAsync(couponUpdateDto.Name) != null)
            {
                response.ErrorMessages.Add("Coupon Name already exist.");
                return Results.BadRequest(response);
            }

            //if (CouponStore.CouponList.FirstOrDefault(x => x.Name.ToLower() == couponUpdateDto.Name.ToLower()) != null)
            //{
            //    response.ErrorMessages.Add("Coupon Name already exist.");
            //    return Results.BadRequest(response);
            //}

            //Coupon couponFromStore = CouponStore.CouponList.FirstOrDefault(x => x.Id == couponUpdateDto.Id);
            //Coupon couponFromStore = await _couponRepo.GetAsync(couponUpdateDto.Id); ;
            //couponFromStore.IsActive = couponUpdateDto.IsActive;
            //couponFromStore.Name = couponUpdateDto.Name;
            //couponFromStore.Percent = couponUpdateDto.Percent;

            await _couponRepo.UpdateAsync(_mapper.Map<Coupon>(couponUpdateDto));
            await _couponRepo.SaveAsync();

            response.Result = _mapper.Map<CouponDTO>(await _couponRepo.GetAsync(couponUpdateDto.Id));
            response.StatusCode = HttpStatusCode.OK;
            response.IsSuccess = true;

            return Results.Ok(response);
        }

        //[Authorize]
        private async static Task<IResult> DeleteCoupon(ICouponRepository _couponRepo, int id)
        {
            ApiResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

            //Coupon couponFromStore = CouponStore.CouponList.FirstOrDefault(x => x.Id == id);
            Coupon coupon = await _couponRepo.GetAsync(id);
            if (coupon == null)
            {
                response.ErrorMessages.Add("Invalid Id.");
                return Results.BadRequest(response);
            }

            //CouponStore.CouponList.Remove(couponFromStore);
            await _couponRepo.RemoveAsync(coupon);
            await _couponRepo.SaveAsync();

            response.StatusCode = HttpStatusCode.NoContent;
            response.IsSuccess = true;

            return Results.Ok(response);
        }
    }
}
