using BepopStreamProject.Context;
using BepopStreamProject.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BepopStreamProject.Controllers
{
    public class SongsController : Controller
    {
        private readonly BepopDbContext _context;

        public SongsController(BepopDbContext context)
        {
            _context = context;
        }

        public IActionResult Play(int songId)
        {
            var userLevel = User.GetUserLevel();
            var song = _context.Songs.FirstOrDefault(s => s.SongId == songId);

            if (song == null) return NotFound();

            if (userLevel >= song.Level)
            {
                // Kullanıcının seviyesi yeterliyse şarkıyı çal
                return Json(new { url = song.FileUrl });
            }
            else
            {
                TempData["Error"] = "Bu şarkıyı dinlemek için paket yükseltmeniz gerekiyor.";
                return RedirectToAction("Upgrade", "Membership");
            }
        }
    }
}
