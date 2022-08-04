using System;
// ReSharper disable InconsistentNaming

namespace SteamLauncher.SteamClient.Interfaces.CustTypes
{
    public class CGameID
    {
        public enum EGameID
        {
            k_EGameIDTypeApp = 0,
            k_EGameIDTypeGameMod = 1,
            k_EGameIDTypeShortcut = 2,
            k_EGameIDTypeP2P = 3
        }

        private readonly BitVector64 _gameID;

        public CGameID()
            : this((ulong)0)
        {
        }
        public CGameID(UInt64 id)
        {
            _gameID = new BitVector64(id);
        }
        public CGameID(Int32 nAppID)
            : this()
        {
        }
        public CGameID(GameID_t gid)
            : this()
        {
            AppID = gid.m_nAppID & 0xFFFFFF;
            AppType = (EGameID)gid.m_nType;
            ModID = gid.m_nModID;
        }

        public static implicit operator UInt64(CGameID gid)
        {
            return gid._gameID.Data;
        }

        public static implicit operator CGameID(UInt64 id)
        {
            return new CGameID(id);
        }

        public static implicit operator CGameID(GameID_t gid)
        {
            return new CGameID(gid);
        }

        public UInt32 AppID
        {
            get => (UInt32)_gameID[0, 0xFFFFFF];
            set => _gameID[0, 0xFFFFFF] = (UInt64)value;
        }
        public EGameID AppType
        {
            get => (EGameID)_gameID[24, 0xFF];
            set => _gameID[24, 0xFF] = (UInt64)value;
        }
        public UInt32 ModID
        {
            get => (UInt32)_gameID[32, 0xFFFFFFFF];
            set => _gameID[32, 0xFFFFFFFF] = (UInt64)value;
        }

        public UInt64 ConvertToUint64()
        {
            return _gameID.Data;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            var gid = obj as CGameID;
            if (gid == null)
                return false;

            return _gameID.Data == gid._gameID.Data;
        }

        public bool Equals(CGameID gid)
        {
            if (gid == null)
                return false;

            return _gameID.Data == gid._gameID.Data;
        }

        public static bool operator ==(CGameID a, CGameID b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if ((a == null) || (b == null))
                return false;

            return a._gameID.Data == b._gameID.Data;
        }

        public static bool operator !=(CGameID a, CGameID b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return _gameID.GetHashCode();
        }
    }
}
