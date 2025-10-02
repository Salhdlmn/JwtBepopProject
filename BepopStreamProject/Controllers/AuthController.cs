    using Azure;
    using BepopStreamProject.Context;
    using BepopStreamProject.DTO_s.Auth;
    using BepopStreamProject.Entities;
using BepopStreamProject.Helpers;
using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;


namespace BepopStreamProject.Controllers
{


    public class AuthController : Controller
    {
        private readonly BepopDbContext _context;
        private readonly IConfiguration _config;
        private readonly JwtTokenGenerator _jwtTokenGenerator;


        public AuthController(BepopDbContext context, IConfiguration config, JwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _config = config;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(UserRegisterDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
            {
                ViewBag.Message = "Bu email zaten kayıtlı.";
                return View(dto);
            }

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                MembershipLevel = 1
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(UserLoginDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                ViewBag.Message = "Hatalı email veya şifre.";
                return View(dto);
            }

            // JWT Token üret
            var token = _jwtTokenGenerator.GenerateToken(user);

            // Cookie içine yaz
            Response.Cookies.Append("jwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(60)
            });

            return RedirectToAction("Upgrade", "Membership");
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwtToken");
            return RedirectToAction("Login");
        }


    }

}
