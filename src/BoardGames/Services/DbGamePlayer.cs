namespace Samples.BoardGames.Services
{
    public class DbGamePlayer
    {
        public string GameId { get; set; }
        public long UserId { get; set; }
        public int Index { get; set; }
        public long Score { get; set; }
    }
}
