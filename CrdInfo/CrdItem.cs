namespace CrdInfo;
public class CrdItem
{
    internal CrdItem(string deployment, string configMapName, string uid)
    {
        Deployment = deployment;
        ConfigMapName = configMapName;
        Uid = uid;
    }

    public string Deployment { get; internal set; }
    public string ConfigMapName { get; internal set; }
    public string Uid { get; internal set; }
}
