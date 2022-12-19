using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceExercise {
    class Service : IService {
        object _lock = new object();
        IConnectionProvider _connectionProvider;
        ConcurrentQueue<Request> _queue;
        int _sum;
        Task _task;
        bool _keepProcessing;
        List<Task> _actions;

        public Service(int maxConnections) {
            _sum = 0;
            _keepProcessing = true;
            _connectionProvider = new ConnectionProvider(maxConnections);
            _queue = new ConcurrentQueue<Request>();
            _actions = new List<Task>();

            _task = Task.Run(() => {
                while (_keepProcessing) {
                    while (_queue.Count > 0) {

                        bool success = _queue.TryDequeue(out Request request);

                        if (success) {
                            Task task = Task.Run(async () => { await act(request); }) ;
                            _actions.Add(task);
                        }
                    }

                    Thread.Sleep(10);
                }
            });
        }


        public int getSummary() {
            _keepProcessing = false;
            _task.Wait();
            Task.WaitAll(_actions.ToArray());

            return _sum;
        }

        public void sendRequest(Request request) {
            _queue.Enqueue(request);
        }

        private async Task act(Request request)
        {
            int command = request.Command;

            ConnectionItem connectionItem = await _connectionProvider.getConnection();

            int result = connectionItem.Connection.runCommand(command);

            _connectionProvider.makeConnectionAvailable(connectionItem);


            lock (_lock)
            {
                _sum += result;
            }
        }
    }
}
