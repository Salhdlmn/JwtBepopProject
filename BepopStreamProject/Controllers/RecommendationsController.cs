using BepopStreamProject.Context;
using BepopStreamProject.Entities;
using BepopStreamProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace BepopStreamProject.Controllers
{
    public class RecommendationsController : Controller
    {
        private readonly BepopDbContext _context;
        private readonly RecommendationService _recommendationService;

        public RecommendationsController(BepopDbContext context, RecommendationService recommendationService)
        {
            _context = context;
            _recommendationService = recommendationService;
        }

        public IActionResult Index(int userId, int topN = 5)
        {
            // 1. Tüm şarkıların Id’leri
            var allSongIds = _context.Songs.Select(s => s.SongId).ToList();

            // 2. Kullanıcının daha önce dinledikleri
            var userPlayedSongIds = _context.PlayHistories
                .Where(p => p.UserId == userId)
                .Select(p => p.SongId)
                .Distinct()
                .ToList();

            // 3. Önerilen şarkıları bul
            var recommendedSongIds = _recommendationService.RecommendSongsForUser(userId, allSongIds, userPlayedSongIds, topN);

            // 4. Şarkı detaylarını getir
            var recommendedSongs = _context.Songs
                .Where(s => recommendedSongIds.Contains(s.SongId))
                .ToList();

            return View(recommendedSongs);
        }
    }
}
