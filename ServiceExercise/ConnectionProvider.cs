using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceExercise {
    class ConnectionItem {
        public Connection Connection { get; set; }
        public bool IsAvailable { get; set; }
    }

    class ConnectionProvider : IConnectionProvider {
        private object _lock = new object();
        private List<ConnectionItem> _connections = null;

        public ConnectionProvider(int maxConnections) {
            _connections = new List<ConnectionItem>();

            for (int i = 0; i < maxConnections; i++) {
                _connections.Add(new ConnectionItem() { 
                    Connection = new Connection(), IsAvailable = true
                });
            }
        }

        public Task<ConnectionItem> getConnection() {
            return Task<ConnectionItem>.Run(() => {
                ConnectionItem connection = null;

                while (connection == null) {
                    lock (_lock) {
                        connection = _connections.FirstOrDefault(conn => conn.IsAvailable);

                        if (connection != null) {
                            connection.IsAvailable = false;
                        }
                    }

                    if (connection == null)
                    {
                        Console.WriteLine("waiting for available connection");
                        Thread.Sleep(10);
                    }
                }

                return connection;
            });
        }

        public void makeConnectionAvailable(ConnectionItem connectionItem) {
            lock(_lock) {
                connectionItem.IsAvailable = true;
            }
        }
    }
}
