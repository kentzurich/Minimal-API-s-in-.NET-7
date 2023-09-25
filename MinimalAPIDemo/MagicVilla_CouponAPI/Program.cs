using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


//Endpoints before app.run
app.MapGet("api/coupon", (ILogger<Program> _logger) => {
    _logger.Log(LogLevel.Information, "Getting all coupons.");
    return Results.Ok(CouponStore.CouponList);
})
.WithName("GetCoupons")
.Produces<IEnumerable<Coupon>>(200);

app.MapGet("api/coupon/{id:int}", (int id) => {
    return Results.Ok(CouponStore.CouponList.FirstOrDefault(x => x.Id == id));
})
.WithName("GetCoupon")
.Produces<Coupon>(200);

app.MapPost("api/coupon", ([FromBody] CouponCreateDto couponCreateDto) => {

    if (string.IsNullOrEmpty(couponCreateDto.Name))
        return Results.BadRequest("Invalid Coupon Name.");

    if(CouponStore.CouponList.FirstOrDefault(x => x.Name.ToLower() == couponCreateDto.Name.ToLower()) != null)
        return Results.BadRequest("Coupon Name already exist.");

    Coupon coupon = new()
    {
        IsActive = couponCreateDto.IsActive,
        Name = couponCreateDto.Name,
        Percent = couponCreateDto.Percent,
        Created = DateTime.UtcNow
    };

    coupon.Id = CouponStore.CouponList.OrderByDescending(x => x.Id).FirstOrDefault().Id + 1;
    CouponStore.CouponList.Add(coupon);

    CouponDTO couponDTO = new()
    {
        Id = coupon.Id,
        IsActive = coupon.IsActive,
        Name = coupon.Name,
        Percent = coupon.Percent,
        Created = coupon.Created
    };

    return Results.CreatedAtRoute("GetCoupon", new { id = coupon.Id }, couponDTO);
    //return Results.Created($"/api/coupon/{coupon.Id}", coupon);
})
.WithName("CreateCoupon")
.Accepts<CouponCreateDto>("application/json")
.Produces<CouponDTO>(201)
.Produces(400);

app.MapPut("api/coupon", () => {
}).WithName("UpdateCoupon");

app.MapDelete("api/coupon/{id:int}", (int id) => {
}).WithName("DeleteCoupon");


app.UseHttpsRedirection();

app.Run();