using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;
using AutoMapper;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        public AccountController(DataContext context,ITokenService tokenService,IMapper mapper)
        {
            _context=context;
            _tokenService = tokenService;
            _mapper = mapper;
        }

         [HttpPost("register")]
         public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
         {
            if(await UserExists(registerDto.Username)) return BadRequest("Username is taken");            
            var user = _mapper.Map<AppUser>(registerDto);
            using var hmac = new HMACSHA512();
            user.UserName = registerDto.Username.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            user.PasswordSalt = hmac.Key;
            if(string.IsNullOrEmpty(user.Interests))
                user.Interests = string.Empty;
            if(string.IsNullOrEmpty(user.Introduction))
                user.Introduction = string.Empty;
            if(string.IsNullOrEmpty(user.LookingFor))
                user.LookingFor = string.Empty;
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return new UserDto 
            { 
                Username = user.UserName, 
                Token = _tokenService.CreateToken(user), 
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
         }

         [HttpPost("login")]
         public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
         {
            var user= await _context.Users
                        .Include(a=>a.Photos)
                        .SingleOrDefaultAsync(x=>x.UserName == loginDto.Username);
            if(user == null) return Unauthorized("Invalid username");
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            for(int i=0;i<computedHash.Length-1;i++)
            {
                if(computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            }
            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                PhotoUrl = user.Photos?.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
         }
         private async Task<bool> UserExists(string username)
         {
            return await _context.Users.AnyAsync(x=>x.UserName == username.ToLower());
         }
    }
}