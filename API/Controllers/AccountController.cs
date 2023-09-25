using System.Text;
using System.Security.Cryptography;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.DTOs;
using API.Interfaces;

namespace API.Controllers;

public class AccountController : BaseApiController
{
    private readonly DataContext _context;

    private readonly ITokenService _tokenService;

    private const string USER_PASSWORD_ERROR_MESSAGE = "Usuario o contrasena incorrectos";
    public AccountController(DataContext context, ITokenService tokenService)
    {
            _context = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if(await UserExists(registerDto.Username)) 
            return BadRequest(USER_PASSWORD_ERROR_MESSAGE);


        using var hmac = new HMACSHA512();

        var user = new AppUser
        {
            UserName = registerDto.Username,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserDto {
            UserName = user.UserName,
            Token = _tokenService.CreateToken(user)
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto LoginDto)
    {
        var user = await _context.Users.SingleOrDefaultAsync( x => 
            x.UserName.ToLower() == LoginDto.Username.ToLower());

        if (user == null) return Unauthorized(USER_PASSWORD_ERROR_MESSAGE);

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(LoginDto.Password));

        for(int i = 0; i < computedHash.Length; i++ )
        {
            if(computedHash[i] != user.PasswordHash[i]) return Unauthorized("Usuario o contrasena incorrectos");
        }
        return new UserDto {
            UserName = user.UserName,
            Token = _tokenService.CreateToken(user)
        };
    }


    private async Task<bool> UserExists( string username )
    {
        return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
    }




    
}
