using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Repository;
using MagicVilla_CouponAPI.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MagicVilla_CouponAPI.Endpoints
{
    public static class AuthEndpoints
    {
        public static void ConfigureAuthEndpoint(this WebApplication app)
        {
            app.MapPost("api/login", Login)
                .WithName("Login")
                .Accepts<LoginRequestDto>("application/json")
                .Produces<ApiResponse>((int)HttpStatusCode.OK)
                .Produces((int)HttpStatusCode.BadRequest);

            app.MapPost("api/register", Register)
                .WithName("Register")
                .Accepts<RegistrationRequestDto>("application/json")
                .Produces<ApiResponse>((int)HttpStatusCode.Created)
                .Produces((int)HttpStatusCode.BadRequest);
        }

        private async static Task<IResult> Login(IAuthRepository _auth, [FromBody] LoginRequestDto loginRequestDto)
        {
            ApiResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

            var loginResponse = await _auth.Login(loginRequestDto);

            if (loginResponse == null) return Results.BadRequest("Username/Password is incorrect.");

            response.StatusCode = HttpStatusCode.OK;
            response.IsSuccess = true;
            response.Result = loginResponse;

            return Results.Ok(response);
        }

        private async static Task<IResult> Register(IAuthRepository _auth, 
            [FromBody] RegistrationRequestDto registrationRequestDto)
        {
            ApiResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

            bool isUsernameUnique = await _auth.IsUniqueUser(registrationRequestDto.UserName);

            if (!isUsernameUnique)
            {
                response.ErrorMessages.Add("Username is not unique.");
                return Results.BadRequest(response);
            }

            var registrationResponse = await _auth.Register(registrationRequestDto);

            if(registrationResponse == null || string.IsNullOrEmpty(registrationResponse.UserName))
            {
                response.ErrorMessages.Add("Error when registering.");
                return Results.BadRequest(response);
            }

            response.StatusCode = HttpStatusCode.Created;
            response.IsSuccess = true;

            return Results.Ok(response);
        }
    }
}
