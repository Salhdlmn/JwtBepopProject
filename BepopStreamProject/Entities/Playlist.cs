namespace BepopStreamProject.Entities
{
    public class Playlist
    {
        public int PlaylistId { get; set; }
        public string Name { get; set; } = null!;
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; }
        public ICollection<PlaylistSong> PlaylistSongs { get; set; }
    }
}
