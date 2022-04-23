namespace Assets.Core.Game
{
    class ResourceStorage
    {
        public int Resources { get; private set; }

        public ResourceStorage()
        {
        }

        public ResourceStorage(int initialResources)
        {
            Resources = initialResources;
        }

        public void Store(int amount)
        {
            Resources += amount;
        }

        public bool Spend(int amount)
        {
            if (Resources < amount)
                return false;

            Resources -= amount;
            return true;
        }
    }
}