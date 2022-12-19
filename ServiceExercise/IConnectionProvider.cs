using System.Threading.Tasks;

namespace ServiceExercise {
    interface IConnectionProvider {
        Task<ConnectionItem> getConnection();
        void makeConnectionAvailable(ConnectionItem connectionItem);
    }
}