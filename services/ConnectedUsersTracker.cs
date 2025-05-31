namespace WebApplicationFlowSync.services
{
    public class ConnectedUsersTracker
    {
        // userId -> list of connectionIds (يمكن أن يكون لدى المستخدم أكثر من اتصال)
        private static readonly Dictionary<string, List<string>> _connections = new();

        public static void AddConnection(string userId, string connectionId)
        {
            lock (_connections)
            {
                if (!_connections.ContainsKey(userId))
                    _connections[userId] = new List<string>();

                _connections[userId].Add(connectionId);
            }
        }

        public static void RemoveConnection(string userId, string connectionId)
        {
            lock (_connections)
            {
                if (_connections.ContainsKey(userId))
                {
                    _connections[userId].Remove(connectionId);
                    if (_connections[userId].Count == 0)
                        _connections.Remove(userId);
                }
            }
        }

        public static bool IsOnline(string userId)
        {
            lock (_connections)
            {
                return _connections.ContainsKey(userId);
            }
        }

        public static List<string> GetOnlineUserIds()
        {
            lock (_connections)
            {
                return _connections.Keys.ToList();
            }
        }
    }
}
