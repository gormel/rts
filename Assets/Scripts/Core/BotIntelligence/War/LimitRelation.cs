using Core.BotIntelligence.Memory;

namespace Core.BotIntelligence.War
{
    class LimitRelation
    {
        private readonly float mMelee;
        private readonly float mRanged;
        private readonly float mArtillery;

        private float Sum => mMelee + mRanged + mArtillery;

        public float Melee => mMelee / Sum;
        public float Ranged => mRanged / Sum;
        public float Artillery => mArtillery / Sum;

        public LimitRelation(float melee, float ranged, float artillery)
        {
            mMelee = melee;
            mRanged = ranged;
            mArtillery = artillery;
        }

        public int GetWarLimit(BotMemory memory)
            => memory.MeeleeWarriors.Count + memory.RangedWarriors.Count + memory.Artilleries.Count;
    }
}