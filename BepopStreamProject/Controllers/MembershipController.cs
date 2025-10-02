using BepopStreamProject.Context;
using BepopStreamProject.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BepopStreamProject.Controllers
{
    public class MembershipController : Controller
    {
        private readonly BepopDbContext _context;
        private readonly JwtTokenGenerator _jwtTokenGenerator;

        public MembershipController(BepopDbContext context, JwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        [HttpGet]
        public IActionResult Upgrade()
        {
            var userLevel = User.GetUserLevel();
            ViewBag.SelectedPackage = userLevel;
            return View();
        }

        [HttpPost]
        public IActionResult Upgrade(int newLevel)
        {
            // 1. Kullanıcıyı Token’dan bul
            var token = Request.Cookies["jwtToken"];
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Kullanıcı giriş yapmamış.");
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Sub yerine NameIdentifier claim’ini oku
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId") ??
        jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier) ??
        jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null) return Unauthorized("Token içinde kullanıcı bilgisi yok.");

            var userId = int.Parse(userIdClaim.Value);
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            // 2. Level güncelle
            user.MembershipLevel = newLevel;
            _context.SaveChanges();

            // 3. Yeni Token oluştur
            var newToken = _jwtTokenGenerator.GenerateToken(user);

            // 4. Cookie’ye yaz
            Response.Cookies.Append("jwtToken", newToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(60)
            });

            TempData["Message"] = "Paket başarıyla yükseltildi!";
            return RedirectToAction("Upgrade", "Membership");
        }

    }
}

