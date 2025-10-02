namespace BepopStreamProject.Entities
{
    public class PlayHistory
    {
        public int Id { get; set; }
        public int HistoryId { get; set; }
        public int UserId { get; set; }
        public int SongId { get; set; }
        public DateTime PlayedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; }
        public Song Song { get; set; }
    }
}
