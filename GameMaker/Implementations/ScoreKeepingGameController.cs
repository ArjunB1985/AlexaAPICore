using GameMaker.DataStorageImplementation;
using GameMaker.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameMaker.Implementations
{
    public class ScoreKeepingGameController : IGameController
    {
        GameUnitOfWork<Game> unitOfWork = new GameUnitOfWork<Game>();
        GameRepositoryImpl<Game> repo;

        GameUnitOfWork<Player> playerUnitOfWork = new GameUnitOfWork<Player>();
        GameRepositoryImpl<Player> playerRepo;
        public ScoreKeepingGameController()
        {
            repo = (GameRepositoryImpl<Game>)unitOfWork.GetGameRepository();

            playerRepo = (GameRepositoryImpl<Player>)playerUnitOfWork.GetEntityRepository();
        }
        public StatusCode EndGame(string userId, string sessionId)
        {
            throw new NotImplementedException();
        }

        public StatusCode SetPlayerAlias(string userId, string sessionId,Player player)
        {
            try
            {
                playerRepo.Update(player);
                playerUnitOfWork.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return StatusCode.Failure;
            }
            return StatusCode.Success;
        }
        public List<Player> GetGameScore(string userId, string sessionId)
        {
            var game=repo.Get(p => p.UserId == userId && p.Gamestate!=GameState.Terminated,null,"Players").FirstOrDefault();
            if (game == null)
            {
                return null;
            }
            else
            {
                return game.Players.OrderBy(q=>q.PlayerName).ToList();
            }
        }

        public StatusCode PauseAndExit(string userId, string sessionId)
        {
            throw new NotImplementedException();
        }

        public StatusCode PauseGame(string userId, string sessionId)
        {
            throw new NotImplementedException();
        }

        public StatusCode ResumeGame(string userId, string sessionId)
        {
            throw new NotImplementedException();
        }

        public StatusCode RetrieveGame(string userId, string sessionId)
        {
            throw new NotImplementedException();
        }

        public StatusCode SetGameScore(List<Player> players, string userId, string sessionId)
        {
            throw new NotImplementedException();
        }

   
        public StatusCode StartGame(string UserId,string SessionId,int numberOfPlayers,string gametype, bool JustScoreKeeping = false)
        {
            //start a game
            try
            {
                //TODO:terminate the last game



                if(numberOfPlayers<2 || SessionId==null || SessionId.Length==0)
                {
                    return StatusCode.Failure;
                }
                ScoreKeepingGame game = new ScoreKeepingGame();
                game.NumberOfPlayers = numberOfPlayers;
                game.SessonId = SessionId;
                game.UserId = UserId;
                game.GameType = gametype;
                game.Players = new List<Player>();
                for(int i = 1; i <= numberOfPlayers; i++)
                {
                    game.Players.Add(new Player() { PlayerId = Guid.NewGuid(), PlayerName = "Player " + i, PlayerScore = 0, PlayerState = PlayerState.Active, PlayerAlias= "Player " + i });
                }
                game.TurnPlayerId = game.Players.First().PlayerId;
                game.Gamestate = GameState.Starting;
                
               
                repo.Add(game);
                unitOfWork.SaveChanges();
                game.Gamestate = GameState.Active;
                unitOfWork.SaveChanges();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return StatusCode.Failure;
            }
            return StatusCode.Success;
        }

        public StatusCode TerminateGame(string userId, string sessionId)
        {
            throw new NotImplementedException();
        }

       
    }
}
