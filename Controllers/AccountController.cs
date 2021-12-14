using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Dtos;
using DatingApp.API.Interfaces;
using DatingApp.API.Models;
using FluentEmail.Core;
using FluentEmail.Handlebars;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IFluentEmail _singleEmail;

        public AccountController(UserManager<AppUser> userManager, 
        SignInManager<AppUser> signInManager,
        ITokenService 
        tokenService, 
        IMapper mapper, IFluentEmail singleEmail)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _singleEmail = singleEmail;
        }

        [HttpPost("register")]
        public async Task<ActionResult<object>> Register(RegisterDto registerDto)
        {
            Email.DefaultRenderer = new HandlebarsRenderer();
            var template = await System.IO.File.ReadAllTextAsync(
                "mail-templates/forgot-password.handlebars");
            if (await CheckExists(registerDto.Username)) return BadRequest("Username already exists");
            
            var user = _mapper.Map<AppUser>(registerDto);
            
                user.UserName = registerDto.Username.ToLower();

                var result =
                    await _userManager.CreateAsync(user, registerDto.Password);

                if (!result.Succeeded) return BadRequest(result.Errors);

                var roleResult = await _userManager.AddToRoleAsync(user, "Member");
                if (!roleResult.Succeeded) return BadRequest(roleResult.Errors);
                Console.WriteLine($"THIS IS THE ROOT: {Directory.GetDirectoryRoot(Directory.GetCurrentDirectory())}/mail-templates");
                try
                {
                    var email = _singleEmail.To("test@test.test")
                                            .Subject("Test email")
                                            .UsingTemplate(template, new
                                            {
                                                firstName = user.UserName, email = user.KnownAs,
                                                programme = user.Gender
                                            });
                    var sendResponse = await email.SendAsync();
                    return new
                    {
                        result = sendResponse
                    };
                }
                catch (Exception e)
                {
                    return BadRequest(e);
                }
            
            // return new UserDto
            // {
            //     Username = user.UserName,
            //     Token = await _tokenService.CreateToken(user),
            //     KnownAs = user.KnownAs,
            //     Gender = user.Gender
            // };
        }
        
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.Users.Include(p=>p.Photos)
            .SingleOrDefaultAsync(x => x
            .UserName ==
                loginDto.Username);
            
            if (user == null) return Unauthorized("Invalid Username");

            var result =
                await _signInManager.CheckPasswordSignInAsync(user,
                    loginDto.Password, false);
            if (!result.Succeeded) return Unauthorized();

            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x=>x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }
        
        
        private async Task<bool> CheckExists(string username)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower
            ());
        }
    }
}