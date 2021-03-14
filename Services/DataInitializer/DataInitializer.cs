using Common;

namespace Services.DataInitializer
{
    public abstract class DataInitializer
    {
        public virtual int Order { get; } = 1;
    }
}
