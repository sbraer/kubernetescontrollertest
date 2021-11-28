using KubernetesController1;
using System.Reflection;

// main
ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

var config = new AppConfiguration();

log.Info("Start check controller");
using var crdInformation = new CrdInformation();
using var cw = new ControllerWait(log, config, crdInformation);

var newConfiguration = cw.WaitNewCrdConfigurationAsync();
var waitConfigMap = cw.WaitConfigMapAsync();

await newConfiguration;
await waitConfigMap;

log.Info("End check controller");
