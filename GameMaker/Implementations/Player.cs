using System;

namespace GameMaker.Implementations
{
    public class Player
    {
        public string PlayerName { get; set; }
        public Guid PlayerId { get; set; }

        public int PlayerScore { get; set; }
        public int PlayerScoreUncommited { get; set; }

        public PlayerState PlayerState { get; set; }

        public string PlayerAlias { get; set; }

        public char PlayerBookmark { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        

    }

    public enum PlayerState
    {
        Active = 0,
        Quit = 1
    }
}