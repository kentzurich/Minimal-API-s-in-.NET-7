using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI;
using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


//Endpoints before app.run

//GET ALL
app.MapGet("api/coupon", (ILogger<Program> _logger) => {
    _logger.Log(LogLevel.Information, "Getting all coupons.");

    ApiResponse response = new();
    response.Result = CouponStore.CouponList;
    response.StatusCode = HttpStatusCode.OK;
    response.IsSuccess = true;
   
    return Results.Ok(response);
})
.WithName("GetCoupons")
.Produces<IEnumerable<ApiResponse>>((int)HttpStatusCode.OK);

//GET
app.MapGet("api/coupon/{id:int}", (int id) => {
    ApiResponse response = new();
    response.Result = CouponStore.CouponList.FirstOrDefault(x => x.Id == id);
    response.StatusCode = HttpStatusCode.OK;
    response.IsSuccess = true;

    return Results.Ok(response);
})
.WithName("GetCoupon")
.Produces<ApiResponse>((int)HttpStatusCode.OK);

//INSERT
app.MapPost("api/coupon", 
async (IMapper _mapper, 
        IValidator<CouponCreateDto> _validation,
        [FromBody] CouponCreateDto couponCreateDto) => {

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

    if(CouponStore.CouponList.FirstOrDefault(x => x.Name.ToLower() == couponCreateDto.Name.ToLower()) != null)
    {
        response.ErrorMessages.Add("Coupon Name already exist.");
        return Results.BadRequest(response);
    }

    Coupon coupon = _mapper.Map<Coupon>(couponCreateDto);

    coupon.Id = CouponStore.CouponList.OrderByDescending(x => x.Id).FirstOrDefault().Id + 1;
    CouponStore.CouponList.Add(coupon);

    CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);

    response.StatusCode = HttpStatusCode.Created;
    response.IsSuccess = true;
    response.Result = couponDTO;

    return Results.Ok(response);
    //return Results.CreatedAtRoute("GetCoupon", new { id = coupon.Id }, response.Result);
    //return Results.Created($"/api/coupon/{coupon.Id}", coupon);
})
.WithName("CreateCoupon")
.Accepts<CouponCreateDto>("application/json")
.Produces<ApiResponse>((int)HttpStatusCode.Created)
.Produces((int)HttpStatusCode.BadRequest);

//UPDATE
app.MapPut("api/coupon", 
async (IMapper _mapper,
 IValidator<CouponUpdateDto> _validation,
 [FromBody] CouponUpdateDto couponUpdateDto) => {
     ApiResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

     var validationResult = await _validation.ValidateAsync(couponUpdateDto);

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

     if (CouponStore.CouponList.FirstOrDefault(x => x.Name.ToLower() == couponUpdateDto.Name.ToLower()) != null)
     {
         response.ErrorMessages.Add("Coupon Name already exist.");
         return Results.BadRequest(response);
     }

     Coupon couponFromStore = CouponStore.CouponList.FirstOrDefault(x => x.Id == couponUpdateDto.Id);
     couponFromStore.IsActive = couponUpdateDto.IsActive;
     couponFromStore.Name = couponUpdateDto.Name;
     couponFromStore.Percent = couponUpdateDto.Percent;

     response.Result = _mapper.Map<CouponDTO>(couponFromStore);
     response.StatusCode = HttpStatusCode.OK;
     response.IsSuccess = true;

     return Results.Ok(response);

 })
.WithName("UpdateCoupon")
.Accepts<CouponUpdateDto>("application/json")
.Produces<ApiResponse>((int)HttpStatusCode.OK)
.Produces((int)HttpStatusCode.BadRequest);

//DELETE
app.MapDelete("api/coupon/{id:int}", 
(int id) => {
    ApiResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

    Coupon couponFromStore = CouponStore.CouponList.FirstOrDefault(x => x.Id == id);
    if(couponFromStore == null)
    {
        response.ErrorMessages.Add("Invalid Id.");
        return Results.BadRequest(response);
    }

    CouponStore.CouponList.Remove(couponFromStore);

    response.StatusCode = HttpStatusCode.NoContent;
    response.IsSuccess = true;

    return Results.Ok(response);
})
.WithName("DeleteCoupon")
.Produces<ApiResponse>((int)HttpStatusCode.OK)
.Produces((int)HttpStatusCode.BadRequest);


app.UseHttpsRedirection();

app.Run();