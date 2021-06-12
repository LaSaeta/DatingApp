using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    public class PresenceTracker
    {
        private static readonly Dictionary<string, List<string>> _onlineUsersDict = new Dictionary<string, List<string>>();

        public Task<bool> UserConnected(string userName, string connectionId)
        {
            bool isOnline = false;

            lock (_onlineUsersDict)
            {
                if (_onlineUsersDict.ContainsKey(userName))
                {
                    _onlineUsersDict[userName].Add(connectionId);
                }
                else
                {
                    _onlineUsersDict.Add(userName, new List<string> { connectionId });
                    isOnline = true;
                }
            }

            return Task.FromResult(isOnline);
        }

        public Task<bool> UserDisconnected(string userName, string connectionId)
        {
            bool isOffline = false;

            lock (_onlineUsersDict)
            {
                if (!_onlineUsersDict.ContainsKey(userName))
                    return Task.FromResult(isOffline);

                _onlineUsersDict[userName].Remove(connectionId);

                if (_onlineUsersDict[userName].Count == 0)
                {
                    _onlineUsersDict.Remove(userName);
                    isOffline = true;
                }

            }

            return Task.FromResult(isOffline);
        }

        public Task<List<string>> GetConnectionsForUser(string userName)
        {
            List<string> connectionIds;

            lock (_onlineUsersDict)
            {
                connectionIds = _onlineUsersDict.GetValueOrDefault(userName);
            }

            return Task.FromResult(connectionIds);
        }

        public Task<string[]> GetOnlineUsers()
        {
            string[] onlineUserNames;

            lock (_onlineUsersDict)
            {
                onlineUserNames = _onlineUsersDict.OrderBy(u => u.Key).Select(u => u.Key).ToArray();
            }

            return Task.FromResult(onlineUserNames);
        }
    }
}
