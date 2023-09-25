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
app.MapGet("api/coupon", () => {
    return Results.Ok(CouponStore.CouponList);
});

app.MapGet("api/coupon/{id:int}", (int id) => {
    return Results.Ok(CouponStore.CouponList.FirstOrDefault(x => x.Id == id));
});

app.MapPost("api/coupon", ([FromBody] Coupon coupon) => {

    if (coupon.Id != 0 || string.IsNullOrEmpty(coupon.Name))
        return Results.BadRequest("Invalid Id or Coupon Name.");

    if(CouponStore.CouponList.FirstOrDefault(x => x.Name.ToLower() == coupon.Name.ToLower()) != null)
        return Results.BadRequest("Coupon Name already exist.");

    coupon.Id = CouponStore.CouponList.OrderByDescending(x => x.Id).FirstOrDefault().Id + 1;
    CouponStore.CouponList.Add(coupon);
    return Results.Ok(coupon);
});

app.MapPut("api/coupon", () => {
});

app.MapDelete("api/coupon/{id:int}", (int id) => {
});


app.UseHttpsRedirection();

app.Run();