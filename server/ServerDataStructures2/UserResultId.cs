

using System;

namespace ServerDataStructures
{
    public struct UserResultId
    {
        public UserResultId(string userName, int gameNumberForUser, int gameId)
        {
            UserName = userName;
            GameNumberForUser = gameNumberForUser;
            GameId = gameId;
        }

        public string UserName { get;  }
        public int GameNumberForUser { get; }
        public int GameId { get; }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 103;
                hash = hash * 83 + GameNumberForUser;
                hash = hash * 83 + GameId;
                hash = hash * 83 + CalculateHashInt(UserName, 2100000017);
                return hash;
            }
        }

        public static int CalculateHashInt(string read, int moduloInt)
        {
            UInt64 hashedValue = 3074457345618258791ul;
            for (int i = 0; i < read.Length; i++)
            {
                hashedValue += read[i];
                hashedValue *= 3074457345618258799ul;
            }
            return (int)hashedValue % moduloInt;
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            if (!(obj is UserResultId))
            {
                return false;
            }

            UserResultId other = (UserResultId) obj;
            return (other.GameNumberForUser == GameNumberForUser 
                    && other.GameId == GameId 
                    &&
                    other.UserName == UserName);
        }
    }
}
