using AutoMapper;
using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_CouponAPI.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private string secretKey;

        public AuthRepository(AppDbContext db, IMapper mapper, IConfiguration config)
        {
            _db = db;
            _mapper = mapper;
            secretKey = config.GetValue<string>("ApiSettings:Secret");
        }

        public async Task<bool> IsUniqueUser(string username)
        {
            var user = await _db.LocalUsers.FirstOrDefaultAsync(x => x.UserName == username);

            if (user == null) return true;

            return false;
        }

        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
        {
            var user = await _db.LocalUsers
                .FirstOrDefaultAsync(x => x.UserName == loginRequestDto.UserName 
                    && x.Password == loginRequestDto.Password);

            if (user == null) return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, user.Role),
                }),
                Expires= DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var writeToken = new JwtSecurityTokenHandler().WriteToken(token);

            LoginResponseDto loginResponseDto = new()
            {
                User = _mapper.Map<UserDto>(user),
                Token = writeToken
            };

            return loginResponseDto;
        }

        public async Task<UserDto> Register(RegistrationRequestDto registrationRequestDto)
        {
            LocalUser user = new()
            {
                UserName = registrationRequestDto.UserName,
                Password = registrationRequestDto.Password,
                Name = registrationRequestDto.Name,
                Role = "admin"
            };

            _db.LocalUsers.Add(user);
            await _db.SaveChangesAsync();

            user.Password = string.Empty;

            return _mapper.Map<UserDto>(user);
        }
    }
}
