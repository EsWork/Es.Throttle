namespace Es.Throttle
{
    internal class HashCodeHelper
    {
        private const int InitialPrime = 23;

        private const int FactorPrime = 29;

        public static int GetHashCode(params int[] hashCodesForProperties)
        {
            unchecked
            {
                int hash = InitialPrime;
                foreach (var code in hashCodesForProperties)
                    hash = hash * FactorPrime + code;
                return hash;
            }
        }
    }
}