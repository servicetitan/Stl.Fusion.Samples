namespace Samples.BoardGames.UI.Shared
{
    public static class LinkBuilder
    {
        public static string Game(string engineId, string gameId = "") => $"/game/{engineId}/{gameId}";
    }
}
