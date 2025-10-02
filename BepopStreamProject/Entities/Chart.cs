namespace BepopStreamProject.Entities
{
    public class Chart
    {
        public int ChartId { get; set; }
        public string Name { get; set; } = null!;   // Weekly Top 100
        public string? Description { get; set; }
        public string? Country { get; set; }
        public string? Genre { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ChartSong> ChartSongs { get; set; }
    }
}
