namespace KubernetesController1;
public partial class ControllerWait
{
    public async Task WaitConfigMapAsync()
    {
        try
        {
            _log.Info("Wait ConfigMap...");
            var startProcess = DateTime.UtcNow;

            var client = new Kubernetes(_config);

            var result = client.ListConfigMapForAllNamespacesWithHttpMessagesAsync(
                watch: true,
                fieldSelector: _appConfiguation.FieldSelector,
                cancellationToken: _cancellationToken.Token
                );

            await foreach (var (type, item) in result.WatchAsync<V1ConfigMap, V1ConfigMapList>())
            {
                _log.Debug($"ConfigMap {DateTime.Now}: {type} - {JsonConvert.SerializeObject(item)}");
                if (item.Metadata.CreationTimestamp.HasValue == false)
                {
                    continue;
                }

                if (type.ToString().ToLower() != "modified")
                {
                    continue;
                }

                var dt = item.Metadata.CreationTimestamp;
                if (!dt.HasValue)
                {
                    _log.Info("Missing CreationTimestamp field");
                    continue;
                }

                if (startProcess > dt.Value)
                {
                    // Messages are old
                    _log.Info("Old ConfigMap Messages");
                    continue;
                }

                var configname = item.Metadata.Name;
                if (string.IsNullOrEmpty(configname))
                {
                    _log.Info($"ConfigMap: Configname is null or empty");
                    continue;
                }

                _log.Info($"ConfigMap Search {configname}");
                var crdInformation = _crdInformation.GetCrdFromConfigmapName(configname);
                if (crdInformation.Length == 0)
                {
                    _log.Info($"ConfigMap '{configname}' not found in saved information");
                    continue;
                }

                var nameSpace = item.Metadata.Namespace() ?? "default";

                foreach (CrdItem singleCrd in crdInformation)
                {
                    _log.Info($"ConfigMap Refresh {singleCrd.Deployment} {nameSpace}");
                    await RefreshDeploymentAsync(client, singleCrd.Deployment, nameSpace);
                }
            };
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
            _log.Error($"ConfigMap {ex.Message}", ex);
            _cancellationToken.Cancel();
        }
    }

    private async Task RefreshDeploymentAsync(Kubernetes client, string deployment, string nameSpace)
    {
        try
        {
            _log.Info($"RefreshDeployment '{deployment}' [{nameSpace}]");
            const string patchStr = @"
{
    ""spec"": {
        ""replicas"": {n}
    }
}";

            var reply = await client.ReadNamespacedDeploymentAsync(deployment, nameSpace);
            if (reply is null)
            {
                _log.Info($"RefreshDeployment {deployment} not found");
                return;
            }

            var numberOfPods = reply.Spec.Replicas ?? 0;
            _log.Info($"Num of pods: {numberOfPods}");
            if (numberOfPods == 0)
            {
                _log.Info($"RefreshDeployment '{deployment}' replicas is zero");
                return;
            }

            await client.PatchNamespacedDeploymentAsync(
                new V1Patch(patchStr.Replace("{n}", "0"),
                V1Patch.PatchType.MergePatch),
                deployment,
                nameSpace);

            await client.PatchNamespacedDeploymentAsync(
                new V1Patch(patchStr.Replace("{n}", numberOfPods.ToString()),
                V1Patch.PatchType.MergePatch),
                deployment,
                nameSpace);

            _log.Info($"RefreshDeployment ALL OK");
        }
        catch (Exception ex)
        {
            _log.Error($"RefreshDeployment {ex.Message}", ex);
        }
    }
}
