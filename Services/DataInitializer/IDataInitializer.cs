using Common;

namespace Services.DataInitializer
{
    public interface IDataInitializer : IScopedDependency
    {
        int Order { get;  }
        void InitializeData();
    }
}
