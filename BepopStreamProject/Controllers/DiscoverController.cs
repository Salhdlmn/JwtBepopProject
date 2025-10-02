using BepopStreamProject.Context;
using BepopStreamProject.Entities;
using BepopStreamProject.Helpers;
using BepopStreamProject.Models;
using BepopStreamProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BepopStreamProject.Controllers
{
    public class DiscoverController : Controller
    {
        private readonly BepopDbContext _context;
        readonly RecommendationService _recommendationService = new RecommendationService();

        public DiscoverController(BepopDbContext context, RecommendationService recommendationService)
        {
            _context = context;
            _recommendationService = recommendationService;
        }

        public IActionResult Index()
        {

            var userId = User.GetUserId();

            // Eğer giriş yapılmamışsa
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            // 🔹 1. Modeli eğit
            var historyData = _context.PlayHistories
                .Include(p => p.Song)
                .ToList();

            if (historyData.Any())
            {
                _recommendationService.TrainModel(historyData);
            }

            // 🔹 2. Tüm şarkılar
            var allSongIds = _context.Songs.Select(s => s.SongId).ToList();

            // 🔹 3. Kullanıcının dinledikleri
            var userPlayedSongIds = _context.PlayHistories
                .Where(p => p.UserId == userId)
                .Select(p => p.SongId)
                .Distinct()
                .ToList();

            // 🔹 4. Önerilen şarkılar
            var recommendedSongIds = new List<int>();
            if (historyData.Any())
            {
                recommendedSongIds = _recommendationService
                    .RecommendSongsForUser(userId, allSongIds, userPlayedSongIds, 6);
            }

            var recommendedSongs = _context.Songs
                .Include(s => s.Artist)
                .Include(s => s.Album)
                .Where(s => recommendedSongIds.Contains(s.SongId))
                .Select(s => new SongViewModel
                {
                    SongId = s.SongId,
                    Title = s.Title,
                    ArtistName = s.Artist.Name,
                    FileUrl = s.FileUrl,
                    Level = s.Level,
                    ImageUrl = s.Album.CoverImageUrl
                })
                .ToList();

            // 🔹 5. Eğer öneri boşsa fallback ekle
            if (!recommendedSongs.Any())
            {
                recommendedSongs = _context.Songs
                    .Include(s => s.Artist)
                    .Include(s => s.Album)
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(6)
                    .Select(s => new SongViewModel
                    {
                        SongId = s.SongId,
                        Title = s.Title,
                        ArtistName = s.Artist.Name,
                        FileUrl = s.FileUrl,
                        Level = s.Level,
                        ImageUrl = s.Album.CoverImageUrl
                    })
                    .ToList();
            }


            var model = new Models.DiscoverViewModel
            {
                TopTracks = _context.PlayHistories
            .GroupBy(p => new
            {
                p.Song.SongId,
                p.Song.Title,
                p.Song.FileUrl,
                p.Song.Level,
                ArtistName = p.Song.Artist.Name,
                AlbumCover = p.Song.Album.CoverImageUrl
            })
            .OrderByDescending(g => g.Count())
            .Take(6)
            .Select(g => new SongViewModel
            {
                SongId = g.Key.SongId,
                Title = g.Key.Title,
                ArtistName = g.Key.ArtistName,
                FileUrl = g.Key.FileUrl,
                Level = g.Key.Level,
                ImageUrl = g.Key.AlbumCover
            })
            .ToList(),






                // 🔹 Son çıkan albümler
                FeaturedAlbums = _context.Albums
                .Include(a => a.Artist) // Artist bilgisi de gelsin
            .OrderByDescending(a => a.ReleaseDate)
            .Take(8)
            .Select(a => new AlbumViewModel
            {
                AlbumId = a.AlbumId,
                Title = a.Title,
                ReleaseDate = a.ReleaseDate,
                CoverImageUrl = a.CoverImageUrl,
                ArtistName = a.Artist.Name

            })
            .ToList(),

                // 🔹 Yeni eklenen şarkılar
                RecentlyAdded = _context.Songs
                 .Include(s => s.Artist) // sanatçı bilgisi
                   .Include(s => s.Album)
            .OrderByDescending(s => s.CreatedAt)
            .Take(10)
            .Select(s => new SongViewModel
            {
                SongId = s.SongId,
                Title = s.Title,
                ArtistName = s.Artist.Name,
                FileUrl = s.FileUrl,
                Level = s.Level,
                ImageUrl = s.Album.CoverImageUrl
            })
            .ToList(),
                RecommendedSongs = recommendedSongs
            };

            return View(model);
        }
    }
}
