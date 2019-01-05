using GameMaker.Implementations;
using System;
using System.Collections.Generic;

namespace GameMaker.Interfaces
{
    public  class Game
    {
        public string SessonId { get; set; }

        public string UserId { get; set; }
        public int NumberOfPlayers { get; set; }
        public List<Player> Players { get; set; }
        public GameState Gamestate { get; set; }
        
        public Guid TurnPlayerId { get; set; }
        
        public int MaxAllowedPlayer { get; set; }

        public string GameType { get; set; }


        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

    }

    public interface IGameController
    {
        StatusCode SetPlayerAlias(string userId, string sessionId, Player player);
        StatusCode StartGame(string UserId, string SessionId,int numberOfPlayers,string gameType, bool JustScoreKeeping=false);
        StatusCode PauseGame(string userId,string sessionId);

        StatusCode PauseAndExit(string userId, string sessionId);
        StatusCode EndGame(string userId, string sessionId);

        StatusCode TerminateGame(string userId, string sessionId);

        StatusCode ResumeGame(string userId, string sessionId);

        StatusCode RetrieveGame(string userId, string sessionId);

        List<Player> GetGameScore(string userId, string sessionId);

        StatusCode SetGameScore(List<Player> players, string userId, string sessionId);

        
    }

    public enum StatusCode
    {
        Success=1,
        Failure=0
    }
    public enum GameState
    {
       Starting=0,
       Active=1,
       Paused=2,
       Ended=3,
       Terminated=4

    }


}
