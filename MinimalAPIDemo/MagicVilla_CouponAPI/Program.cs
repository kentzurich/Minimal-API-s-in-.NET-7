using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
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

app.MapPost("api/coupon", ([FromBody] Coupon coupon) => {

    if (coupon.Id != 0 || string.IsNullOrEmpty(coupon.Name))
        return Results.BadRequest("Invalid Id or Coupon Name.");

    if(CouponStore.CouponList.FirstOrDefault(x => x.Name.ToLower() == coupon.Name.ToLower()) != null)
        return Results.BadRequest("Coupon Name already exist.");

    coupon.Id = CouponStore.CouponList.OrderByDescending(x => x.Id).FirstOrDefault().Id + 1;
    CouponStore.CouponList.Add(coupon);
    return Results.CreatedAtRoute("GetCoupon", new { id = coupon.Id }, coupon);
    //return Results.Created($"/api/coupon/{coupon.Id}", coupon);
})
.WithName("CreateCoupon")
.Accepts<Coupon>("application/json")
.Produces<Coupon>(201)
.Produces(400);

app.MapPut("api/coupon", () => {
}).WithName("UpdateCoupon");

app.MapDelete("api/coupon/{id:int}", (int id) => {
}).WithName("DeleteCoupon");


app.UseHttpsRedirection();

app.Run();