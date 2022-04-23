using System;

namespace Assets.Core.Game
{
    public class Upgrade<TStat>
    {
        private readonly Func<TStat, int, TStat> mCalculator;
        public int Level { get; private set; }
        public int MaxLevel { get; }

        private bool mLevelingUp;

        public bool LevelUpAvaliable => !mLevelingUp && Level < MaxLevel;

        public Upgrade(int maxLevel, Func<TStat, int, TStat> calculator)
        {
            mCalculator = calculator;
            MaxLevel = maxLevel;
        }

        public TStat Calculate(TStat baseValue)
        {
            return mCalculator(baseValue, Level);
        }

        public void BeginLevelUp()
        {
            mLevelingUp = true;
        }

        public void CancelLevelUp()
        {
            mLevelingUp = false;
        }

        public void EndLevelUp()
        {
            if (!mLevelingUp)
                return;

            mLevelingUp = false;
            Level++;
        }
    }
}