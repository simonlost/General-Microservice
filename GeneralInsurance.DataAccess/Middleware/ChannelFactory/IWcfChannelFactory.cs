namespace GeneralInsurance.DataAccess.Middleware.ChannelFactory
{
    public interface IWcfChannelFactory<out T> where T:class
    {
        T CreateChannel();
    }
}