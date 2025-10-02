using BepopStreamProject.Context;
using BepopStreamProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BepopStreamProject.Controllers
{
    public class ArtistController : Controller
    {
        private readonly BepopDbContext _context;

        public ArtistController(BepopDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string country)
        {
            var artistsQuery = _context.Artists.AsQueryable();

        if (!string.IsNullOrEmpty(country) && country != "All countries")
            {
                artistsQuery = artistsQuery.Where(a => a.Country == country);
            }

        var artists=artistsQuery
                .Select(a=>new ArtistViewModel
                {
                    ArtistId = a.ArtistId,
                    Name = a.Name,
                    Country = a.Country,
                    ImageUrl = a.ProfileImageUrl
                }).ToList();

            var countries = _context.Artists
                .Select(a => a.Country)
                .Distinct()
                .ToList();
            countries.Insert(0, "All countries");

            var topTracks = _context.PlayHistories
              .Include(p => p.Song).ThenInclude(s => s.Artist)
              .Include(p => p.Song).ThenInclude(s => s.Album)
              .Include(p => p.Song).ThenInclude(s => s.SongGenres).ThenInclude(sg => sg.Genre)
              .AsEnumerable() // GroupBy SQL’de çevrilemediği için
              .GroupBy(p => p.Song)
              .OrderByDescending(g => g.Count())
              .Take(10)
              .Select(g => new SongViewModel
              {
                  SongId = g.Key.SongId,
                  Title = g.Key.Title,
                  ArtistName = g.Key.Artist.Name,
                  FileUrl = g.Key.FileUrl,
                  ImageUrl = g.Key.Album.CoverImageUrl,
                  Level = g.Key.Level,
                  Genres = g.Key.SongGenres.Select(sg => sg.Genre.Name).ToList()
              })
              .ToList();
            ViewBag.v1=_context.Artists.Count();
            var vm = new ArtistPageViewModel
            {
                Artists = artists,
                Countries = countries,
                TopTracks = topTracks
            };
            return View(vm);
        }

        public IActionResult Detail(int id)
        {
            var artist = _context.Artists
        .Include(a => a.Songs)
        .ThenInclude(s => s.Album)
        .FirstOrDefault(a => a.ArtistId == id);

            var topTracks = _context.PlayHistories
             .Include(p => p.Song).ThenInclude(s => s.Artist)
             .Include(p => p.Song).ThenInclude(s => s.Album)
             .Include(p => p.Song).ThenInclude(s => s.SongGenres).ThenInclude(sg => sg.Genre)
             .AsEnumerable() // GroupBy SQL’de çevrilemediği için
             .GroupBy(p => p.Song)
             .OrderByDescending(g => g.Count())
             .Take(10)
             .Select(g => new SongViewModel
             {
                 SongId = g.Key.SongId,
                 Title = g.Key.Title,
                 ArtistName = g.Key.Artist.Name,
                 FileUrl = g.Key.FileUrl,
                 ImageUrl = g.Key.Album.CoverImageUrl,
                 Level = g.Key.Level,
                 Genres = g.Key.SongGenres.Select(sg => sg.Genre.Name).ToList()
             })
             .ToList();

            if (artist == null) return NotFound();

            var albums = artist.Songs
            .Select(s => s.Album)               // Şarkılardan albümleri al
            .Distinct()                         // Tekilleştir
            .Select(a => new AlbumViewModel
            {
                AlbumId = a.AlbumId,
                Title = a.Title,
                CoverImageUrl = a.CoverImageUrl,
                ReleaseDate = a.ReleaseDate,
                ArtistName = artist.Name
            })
            .ToList();

            var vm = new ArtistDetailViewModel
            {
                ArtistId = artist.ArtistId,
                Name = artist.Name,
                Country = artist.Country,
                ImageUrl = artist.ProfileImageUrl,
                TopTracks=topTracks,
                Songs = artist.Songs.Select(s => new SongViewModel
                {
                    SongId = s.SongId,
                    Title = s.Title,
                    ArtistName = artist.Name,
                    FileUrl = s.FileUrl,
                    ImageUrl = s.Album.CoverImageUrl,
                    Level = s.Level
                }).ToList(),
                Albums = albums 


            };

            return View(vm);

        }
    }
}
