using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DistanceRando
{
    class RandoGame
    {
        internal Dictionary<string, RandoMap> maps = new Dictionary<string, RandoMap>();

        internal RandoCarAbilityState abilityState;

        internal int seed = 0;

        byte[] rawHash;
        string seedHash = "";
        internal string truncSeedHash = "";
        internal string friendlyHash = "";

        public RandoGame(string inputSeed, string randoVersion)
        {
            int integerSeed;
            try
            {
                integerSeed = int.Parse(inputSeed.Trim());
            }
            catch (FormatException)
            {
                integerSeed = inputSeed.Trim().GetHashCode();
            }

            this.seed = integerSeed;

            Randomize(integerSeed);

            rawHash = GenerateSeedHash(randoVersion, maps);
            seedHash = ConvertHashToString(rawHash);
            truncSeedHash = seedHash.Truncate(7);
            friendlyHash = FriendlyHash(rawHash);

            abilityState = new RandoCarAbilityState(this);
        }

        byte[] GenerateSeedHash(string version, Dictionary<string, RandoMap> mapList)
        {
            string mapsString = "";
            foreach (var map in mapList)
            {
                mapsString += map.Key +
                            map.Value.abilityEnabled +
                            map.Value.boostEnabled +
                            map.Value.jumpEnabled +
                            map.Value.wingsEnabled +
                            map.Value.jetsEnabled;
            }

            byte[] hashBytes = Encoding.UTF8.GetBytes(version + mapsString);

            //string hash = "";
            byte[] hash;
            using (SHA256 sha = SHA256.Create())
            {
                hash = sha.ComputeHash(hashBytes);
            }

            return hash;
        }

        string ConvertHashToString(byte[] hash)
        {
            StringBuilder strBuilder = new StringBuilder();

            foreach (var b in hash)
            {
                strBuilder.Append(b.ToString("x2"));
            }

            return strBuilder.ToString();
        }

        string FriendlyHash(byte[] hash)
        {
            string truncHash = Convert.ToBase64String(hash).Truncate(4);

            string friendlyHash = "";

            foreach (char l in truncHash)
            {
                try
                {
                    friendlyHash += $"{Metadata.FriendlyHashWords[l]} ";
                }
                catch (KeyNotFoundException)
                {
                    friendlyHash += $"{l} ";
                }
            }

            return friendlyHash;
        }

        void Randomize(int seed)
        {
            var logic = new RandoLogic();

            maps = logic.GenerateGame(seed);
        }
    }
}
