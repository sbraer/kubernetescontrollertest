namespace KubernetesController1;
public partial class ControllerWait : IDisposable
{
    private readonly ILog _log;
    private readonly AppConfiguration _appConfiguation;
    private readonly CancellationTokenSource _cancellationToken;
    private readonly CrdInformation _crdInformation;
    private readonly KubernetesClientConfiguration _config;
    private bool disposedValue;

    public ControllerWait(ILog log, AppConfiguration appConfiguation, CrdInformation crdInformation, CancellationTokenSource? cancellationToken = null, string? k8sConfigFile = null)
    {
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _appConfiguation = appConfiguation ?? throw new ArgumentNullException(nameof(appConfiguation));
        _crdInformation = crdInformation ?? throw new ArgumentNullException(nameof(crdInformation));
        _cancellationToken = cancellationToken ?? new CancellationTokenSource();

        if (string.IsNullOrEmpty(k8sConfigFile))
        {
            _config = KubernetesClientConfiguration.InClusterConfig();
        }
        else
        {
            _config = KubernetesClientConfiguration.BuildConfigFromConfigFile(new FileInfo(k8sConfigFile));
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _cancellationToken.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
