namespace KubernetesController1;
public partial class ControllerWait
{
    public async Task WaitNewCrdConfigurationAsync()
    {
        try
        {
            var startProcess = DateTime.UtcNow;

            _log.Info("CRD Wait new CRD configuration...");
            _log.Debug($"From config: {_appConfiguation.Group} {_appConfiguation.Version} {_appConfiguation.NamespaceParameter} {_appConfiguation.Plural} {_appConfiguation.FieldSelector}");

            // Use the config object to create a client.
            var client = new Kubernetes(_config);

            var result = await client.ListNamespacedCustomObjectWithHttpMessagesAsync(
                group: _appConfiguation.Group,
                version: _appConfiguation.Version,
                namespaceParameter: _appConfiguation.NamespaceParameter,
                plural: _appConfiguation.Plural,
                watch: true,
                fieldSelector: _appConfiguation.FieldSelector,
                cancellationToken: _cancellationToken.Token
                )
                .ConfigureAwait(false);

            while (_cancellationToken.IsCancellationRequested == false)
            {
                _log.Debug($"Wait info {DateTime.Now}");
                result.Watch((WatchEventType method, object message) =>
                {
                    _log.Debug($"{DateTime.Now}: {method} - {message}");

                    string deployment, configMapName, uid;
                    try
                    {
                        var ojson = JObject.Parse(message.ToString());
                        var time = (string)ojson["metadata"]["creationTimestamp"];
                        var dt = DateTime.Parse(time);

                        if (startProcess > dt)
                        {
                            // Messages are old
                            _log.Info("Old CRD Messages");
                            return;
                        }

                        deployment = (string)ojson["spec"]["deploymentName"];
                        configMapName = (string)ojson["spec"]["configName"];
                        uid = (string)ojson["metadata"]["uid"];

                        _log.Info($"CRD deploymentName = '{deployment ?? "NULL"}' configName = '{configMapName ?? "NULL"}' uid = '{uid ?? "NULL"}'");
                        if (string.IsNullOrEmpty(deployment))
                        {
                            _log.Error("CRD deploymentName is null or empty");
                            return;
                        }
                        if (string.IsNullOrEmpty(configMapName))
                        {
                            _log.Error("CRD configName is null or empty");
                            return;
                        }
                        if (string.IsNullOrEmpty(uid))
                        {
                            _log.Error("CRD uid is null or empty");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        return;
                    }

                    _log.Info($"CRD {method}");

                    switch (method.ToString().ToLower())
                    {
                        case "added":
                        case "modified":
                            _crdInformation.AddCrdConfiguration(deployment, configMapName, uid);
                            break;
                        case "deleted":
                            _crdInformation.DeleteCrdConfiguration(uid);
                            break;
                        default:
                            _log.Warn("Unrecognized method");
                            break;
                    }

                    var allcrd = _crdInformation.GetCrdAll();
                    if (allcrd.Length == 0)
                    {
                        _log.Debug("CRD list is EMPTY");
                    }
                    else
                    {
                        foreach (var item in allcrd)
                        {
                            _log.Debug($"CRD: {item.Deployment} {item.ConfigMapName} {item.Uid}");
                        }
                    }
                });

                await Task.Delay(TimeSpan.FromHours(1), _cancellationToken.Token);
            }
        }
        catch (HttpOperationException httpOperationException)
        {
            var phase = httpOperationException.Response.ReasonPhrase;
            var content = httpOperationException.Response.Content;
            _log.Error($"Phase: {phase}\r\nContent: {content}");
            _cancellationToken.Cancel();
        }
        catch (Exception ex)
        {
            _log.Error($"CRD {ex.Message}", ex);
            _cancellationToken.Cancel();
        }
    }
}
