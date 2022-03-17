using System;

namespace Assets.Core.Game
{
    public class Upgrade<TStat>
    {
        private readonly Func<TStat, int, TStat> mCalculator;
        public int Level { get; private set; }
        public int MaxLevel { get; }

        public Upgrade(int maxLevel, Func<TStat, int, TStat> calculator)
        {
            mCalculator = calculator;
            MaxLevel = maxLevel;
        }

        public TStat Calculate(TStat baseValue)
        {
            return mCalculator(baseValue, Level);
        }
        
        public void LevelUp()
        {
            Level++;
        }
    }
}