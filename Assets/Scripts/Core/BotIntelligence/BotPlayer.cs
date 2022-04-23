using System;
using System.Linq;
using Assets.Core.Game;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.BotIntelligence
{
    class BotPlayer : Player
    {
        private readonly Game mGame;
        private Worker mControlledWorker;
        
        public BotPlayer(Game game, IGameObjectFactory externalFactory, int team) 
            : base(externalFactory, team)
        {
            mGame = game;
        }

        public void Update(TimeSpan detaTime)
        {
            if (mControlledWorker == null)
            {
                mControlledWorker = mGame.RequestPlayerObjects<Worker>(this).FirstOrDefault();
            }

            if (mControlledWorker != null)
            {
                if (mControlledWorker.IntelligenceTag == Unit.IdleIntelligenceTag)
                {
                    var dx = Random.Range(-2f, 2f);
                    var dy = Random.Range(-2f, 2f);
                    var t = mControlledWorker.GoTo(mControlledWorker.Position + new Vector2(dx, dy));
                }
            }
        }
    }
}