namespace WcfSamples.DynamicProxy
{
    public interface IDynamicProxyFactory
    {
        DynamicObject CreateProxy(string contractName);
        DynamicObject CreateMainProxy();
    }
}