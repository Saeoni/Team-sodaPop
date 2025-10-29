using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Meryel.Serilog;
using Meryel.UnityCodeAssist.Editor.Logger;
using Meryel.UnityCodeAssist.MQTTnet;
using Meryel.UnityCodeAssist.MQTTnet.Adapter;
using Meryel.UnityCodeAssist.MQTTnet.Diagnostics;
using Meryel.UnityCodeAssist.MQTTnet.Implementations;
using Meryel.UnityCodeAssist.MQTTnet.Protocol;
using Meryel.UnityCodeAssist.MQTTnet.Server;
using Meryel.UnityCodeAssist.Newtonsoft.Json;
using Meryel.UnityCodeAssist.Synchronizer.Model;
using UnityEditor;
using UnityEngine;
using Task = System.Threading.Tasks.Task;
using Application = UnityEngine.Application;
using GameObject = Meryel.UnityCodeAssist.Synchronizer.Model.GameObject;
#pragma warning disable IDE0005

#pragma warning restore IDE0005


#nullable enable


//**--
// can also do this for better clear, sometimes it gets locked
// https://answers.unity.com/questions/704066/callback-before-unity-reloads-editor-assemblies.html#

namespace Meryel.UnityCodeAssist.Editor
{
    public class MQTTnetPublisher : IProcessor
    {
        //public readonly List<Synchronizer.Model.Connect> clients;
        private readonly ConcurrentDictionary<string, Connect> _clients;

        private readonly Manager syncMngr;

        private Connect? _self;
        private MqttServer? broker;

        private CancellationTokenSource? cancellationTokenSource;

        public MQTTnetPublisher()
        {
            // LogContext();

            Log.Debug("MQTTnet server initializing, begin");

            InitializeSelf();

            _clients = new ConcurrentDictionary<string, Connect>();
            syncMngr = new Manager(this);

            var port = Utilities.GetPortForMQTTnet(Self!.ProjectPath);


            // Create the options for our MQTT Broker
            var options = new MqttServerOptionsBuilder()
                    // set endpoint to localhost
                    .WithDefaultEndpoint()
                    // port used will be 707
                    .WithDefaultEndpointPort(port)
                    // handler for new connections
                    //.WithConnectionValidator(OnNewConnection)
                    // handler for new messages
                    //.WithApplicationMessageInterceptor(OnNewMessage)

                    // disable ipv6 for linux (and possibly macos too), otherwise socket exception is thrown
                    .WithDefaultEndpointBoundIPV6Address(IPAddress.None)

                    // for preventing socket ex after server restart https://github.com/dotnet/MQTTnet/issues/494
                    // System.Net.Sockets.SocketException (0x80004005): Only one usage of each socket address (protocol/network address/port) is normally permitted.
                    .WithTlsEndpointReuseAddress()
                ;

            IList<IMqttServerAdapter> DefaultServerAdapters = new List<IMqttServerAdapter>
            {
                new MqttTcpServerAdapter()
            };
            var logger = new MqttNetNullLogger();


            broker = new MqttServer(options.Build(), DefaultServerAdapters, logger);

            broker.InterceptingPublishAsync += Broker_InterceptingPublishAsync;
            broker.ClientDisconnectedAsync += Broker_ClientDisconnectedAsync;

            Log.Debug("MQTTnet server initializing, constructed broker, port: {Port}", port);

            try
            {
                //broker.StartAsync().GetAwaiter().GetResult();

                var startTask = Task.Run(() => broker.StartAsync());
                if (!startTask.Wait(TimeSpan.FromSeconds(5)))
                {
                    Log.Error("MQTTnet broker.StartAsync timed out.");
                    return;
                }


                Log.Debug("MQTTnet server initializing, started broker");
            }
            catch (SocketException socketEx)
            {
                Log.Error(socketEx, "Socket exception");
                LogContext();
                //Serilog.Log.Warning("Socket exception disposing pubSocket");
                //broker.Dispose();
                //broker = null;
                return;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "MQTTnet broker.StartAsync failed.");
                return;
            }


            //pubSocket.SendReady += PubSocket_SendReady;
            //SendConnect();

            cancellationTokenSource = new CancellationTokenSource();
            //pullThread = new System.Threading.Thread(async () => await PullAsync(conn.pushPull, pullThreadCancellationTokenSource.Token));
            //pullThread = new System.Threading.Thread(() => InitPull(conn.pushPull, pullTaskCancellationTokenSource.Token));
            //pullThread.Start();
            //Task.Run(() => InitPullAsync());


            Log.Debug("MQTTnet server initializing, initialized");

            // need to sleep here, clients will take some time to start subscribing
            // https://github.com/zeromq/netmq/issues/482#issuecomment-182200323
            Thread.Sleep(1000);
            SendConnect();

            Log.Debug("MQTTnet server initializing, initialized at {port} with {projectPath}", port, Self!.ProjectPath);
        }

        public IEnumerable<Connect> Clients =>
            _clients.Values.Where(c => c.NodeKind != NodeKind.SemiClient_RoslynAnalyzer.ToString());

        private Connect Self => _self!;

        internal RequestUpdate? DelayedRequestUpdate { get; private set; }


        string IProcessor.Serialize<T>(T value)
        {
            //return System.Text.Json.JsonSerializer.Serialize<T>(value);
            //return Newtonsoft.Json.JsonConvert.SerializeObject(value);
            return SerializeObject(value);
        }

        T IProcessor.Deserialize<T>(string data)
        {
            //return System.Text.Json.JsonSerializer.Deserialize<T>(data)!;
            //return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data)!;
            //return TinyJson.JsonParser.FromJson<T>(data)!;
            //return Meryel.UnityCodeAssist.ProjectData.LitJson.JsonMapper.ToObject<T>(data);
            return JsonConvert.DeserializeObject<T>(data)!;

            //byte[] buffer = System.Text.Encoding.UTF8.GetBytes(data);
            //T val = OdinSerializer.SerializationUtility.DeserializeValue<T>(buffer, OdinSerializer.DataFormat.JSON);
            //return val;
        }

        //**--make sure all Synchronizer.Model.IProcessor.Process methods are thread-safe

        // a new client has connected
        void IProcessor.Process(Connect connect)
        {
            if (connect.ModelVersion != Self.ModelVersion)
            {
                Log.Error(
                    "Version mismatch with {ContactInfo}. Please update your Unity asset and reinstall the Visual Studio/VS Code extension. {ContactModel} != {SelfModel}",
                    connect.ContactInfo, connect.ModelVersion, Self.ModelVersion);
                return;
            }

            if (connect.ProjectPath != Self.ProjectPath)
            {
                Log.Error("Project mismatch with {ProjectName}. '{ConnectPath}' != '{SelfPath}'", connect.ProjectName,
                    connect.ProjectPath, Self.ProjectPath);
                return;
            }

            if (!string.IsNullOrEmpty(connect.LiteOrFull) && connect.LiteOrFull != Self.LiteOrFull)
                if (connect.LiteOrFull == "Lite")
                {
                    //**-- upgrade vsix to full here //**--//**--
                }

            var hasClient = _clients.TryGetValue(connect.ClientId, out var client);
            if (!hasClient)
            {
                _clients[connect.ClientId] = connect;
            }
            else
            {
                // LiteOrFull field might be updated
                client.ModelVersion = connect.ModelVersion;
                client.ProjectPath = connect.ProjectPath;
                client.ProjectName = connect.ProjectName;
                client.ContactInfo = connect.ContactInfo;
                client.AssemblyVersion = connect.AssemblyVersion;
                client.LiteOrFull = connect.LiteOrFull;
                client.NodeKind = connect.NodeKind;
                client.ClientId = connect.ClientId;
            }

            SendHandshake();
            if (ScriptFinder.GetActiveGameObject(out var activeGO))
                SendGameObject(activeGO);
            Assister.SendTagsAndLayers();
        }

        // a new client is online and requesting connection
        void IProcessor.Process(RequestConnect requestConnect)
        {
            SendConnect();
        }

        void IProcessor.Process(Disconnect disconnect)
        {
            var removed = _clients.TryRemove(disconnect.ClientId, out var client);
            Log.Debug("Synchronizer.Model.Disconnect {ClientId} {Removed}", disconnect.ClientId, removed);
        }

        void IProcessor.Process(ConnectionInfo connectionInfo)
        {
            if (connectionInfo.ModelVersion != Self.ModelVersion)
            {
                Log.Error(
                    "Version mismatch with {ContactInfo}. Please update your Unity asset and reinstall the Visual Studio/VS Code extension. {ContactModel} != {SelfModel}",
                    connectionInfo.ContactInfo, connectionInfo.ModelVersion, Self.ModelVersion);
                return;
            }

            if (connectionInfo.ProjectPath != Self.ProjectPath)
            {
                Log.Error("Project mismatch with {ProjectName}. '{ConnectPath}' != '{SelfPath}'",
                    connectionInfo.ProjectName, connectionInfo.ProjectPath, Self.ProjectPath);
                return;
            }

            if (!_clients.TryGetValue(connectionInfo.ClientId, out _))
            {
                SendConnect();
            }
            else
            {
                SendHandshake();
                if (ScriptFinder.GetActiveGameObject(out var activeGO))
                    SendGameObject(activeGO);
                Assister.SendTagsAndLayers();
            }
        }

        void IProcessor.Process(RequestConnectionInfo requestConnectionInfo)
        {
            SendConnectionInfo();
        }

        /*
        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.Layers layers)
        {

        }
        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.Tags tags)
        {

        }
        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.SortingLayers sortingLayers)
        {

        }*/
        void IProcessor.Process(StringArray stringArray)
        {
            Log.Warning(
                "Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.StringArray)");
        }

        void IProcessor.Process(StringArrayContainer stringArrayContainer)
        {
            Log.Warning(
                "Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.StringArrayContainer)");
        }

        void IProcessor.Process(GameObject gameObject)
        {
            Log.Warning(
                "Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.GameObject)");
        }

        void IProcessor.Process(ComponentData component)
        {
            Log.Warning(
                "Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.ComponentData)");
        }

        void IProcessor.Process(Component_Animator component_Animator)
        {
            Log.Warning(
                "Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.Component_Animator)");
        }

        void IProcessor.Process(Component_Animation component_Animation)
        {
            Log.Warning(
                "Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.Component_Animation)");
        }

        void IProcessor.Process(Component_Material component_Material)
        {
            Log.Warning(
                "Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.Component_Material)");
        }

        void IProcessor.Process(RequestScript requestScript)
        {
            if (requestScript.DeclaredTypes == null || requestScript.DeclaredTypes.Length == 0)
                return;

            var documentPath = requestScript.DocumentPath;

            foreach (var declaredType in requestScript.DeclaredTypes)
                if (ScriptFinder.FindInstanceOfType(declaredType, documentPath, out var go, out var so))
                {
                    if (go != null)
                        SendGameObject(go);
                    else if (so != null)
                        SendScriptableObject(so);
                    else
                        Log.Warning("Invalid instance of type");
                }
                else
                {
                    SendScriptMissing(declaredType);
                }
        }

        void IProcessor.Process(RequestScriptFast requestScriptFast)
        {
            var documentPath = requestScriptFast.DocumentPath;

            //**--namespace?
            var possiblyDeclaredType = Path.GetFileNameWithoutExtension(documentPath);

            if (ScriptFinder.FindInstanceOfType(possiblyDeclaredType, documentPath, out var go, out var so))
            {
                if (go != null)
                    SendGameObject(go);
                else if (so != null)
                    SendScriptableObject(so);
                else
                    Log.Warning("Invalid instance of type");
            }
        }

        void IProcessor.Process(ScriptMissing scriptMissing)
        {
            Log.Warning(
                "Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.ScriptMissing)");
        }


        void IProcessor.Process(Handshake handshake)
        {
            // Do nothing
        }

        void IProcessor.Process(RequestInternalLog requestInternalLog)
        {
            SendInternalLog();
        }

        void IProcessor.Process(InternalLog internalLog)
        {
            ELogger.VsInternalLog = internalLog.LogContent;
        }

        void IProcessor.Process(AnalyticsEvent analyticsEvent)
        {
            Log.Warning(
                "Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.AnalyticsEvent)");
        }

        void IProcessor.Process(ErrorReport errorReport)
        {
            Log.Warning(
                "Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.ErrorReport)");
        }

        void IProcessor.Process(RequestVerboseType requestVerboseType)
        {
            Log.Warning(
                "Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.RequestVerboseType)");
        }

        void IProcessor.Process(RequestLazyLoad requestLazyLoad)
        {
            Monitor.LazyLoad(requestLazyLoad.Category);
        }

        void IProcessor.Process(RequestUpdate requestUpdate)
        {
            if (requestUpdate.App != "Unity" && requestUpdate.App != "SystemBinariesForDotNetStandard20")
                return;

            // cannot import package in play mode, so delay it
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Log.Information("Cannot import package in play mode, please exit play mode to update");
                DelayedRequestUpdate = requestUpdate;
                return;
            }

            DelayedRequestUpdate = null;

            // let unity update the package, don't unzip it, to prevent file already in use and other issues
            AssetDatabase.ImportPackage(requestUpdate.Path, requestUpdate.IsInteractive);
        }

        void IProcessor.Process(RelayDocumentShow relayDocumentShow)
        {
            ForwardRelayMessage(relayDocumentShow);
        }

        void IProcessor.Process(RelayDocumentSave relayDocumentSave)
        {
            ForwardRelayMessage(relayDocumentSave);
        }

        void IProcessor.Process(RelayDocumentViewportChanged relayDocumentViewportChanged)
        {
            ForwardRelayMessage(relayDocumentViewportChanged);
        }

        void IProcessor.Process(RelayLogMessage relayLogMessage)
        {
            ForwardRelayMessage(relayLogMessage);
        }

        void IProcessor.Process(RelayUpdateExport relayUpdateExport)
        {
            ForwardRelayMessage(relayUpdateExport);
        }

        void IProcessor.Process(RelayAdornmentText relayAdornmentText)
        {
            ForwardRelayMessage(relayAdornmentText);
        }

        private void InitializeSelf()
        {
            var projectPath = CommonTools.GetProjectPath();
            _self = new Connect
            {
                ModelVersion = Utilities.Version,
                ProjectPath = projectPath,
                ProjectName = getProjectName(),
                ContactInfo = $"Unity {Application.unityVersion}",
                AssemblyVersion = Assister.Version,
#if MERYEL_UCA_LITE_VERSION
                LiteOrFull = "Lite",
#else
                LiteOrFull = "Full",
#endif
                NodeKind = NodeKind.Server.ToString(),
                ClientId = ""
            };

            string getProjectName()
            {
                string[] s = projectPath.Split('/');
#pragma warning disable IDE0056
                var projectName = s[s.Length - 1];
#pragma warning restore IDE0056
                //Logg("project = " + projectName);
                return projectName;
            }
        }


        public static void LogContext()
        {
        }

        private Task Broker_ClientDisconnectedAsync(ClientDisconnectedEventArgs arg)
        {
            try
            {
                var removed = _clients.TryRemove(arg.ClientId, out _);
                Log.Debug("Broker_ClientDisconnectedAsync {ClientId} {Result}", arg.ClientId, removed);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "async exception at {Location}", nameof(Broker_ClientDisconnectedAsync));
            }

            return Task.CompletedTask;
        }

        private Task Broker_InterceptingPublishAsync(InterceptingPublishEventArgs arg)
        {
            try
            {
                // if server message
                if (string.IsNullOrEmpty(arg.ClientId))
                    return Task.CompletedTask;

                Log.Verbose("mqttnet consume {topic} {content}", arg.ApplicationMessage.Topic,
                    arg.ApplicationMessage.ConvertPayloadToString());

                var topic = arg.ApplicationMessage.Topic;
                var header = topic.Substring(3); // for "cs/" prefix
                var content = arg.ApplicationMessage.ConvertPayloadToString();

                MainThreadDispatcher.Add(() => syncMngr.ProcessMessage(header, content));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "async exception at {Location}", nameof(Broker_InterceptingPublishAsync));
            }

            return Task.CompletedTask;
        }

        public void Clear()
        {
            // LogContext();

            Log.Verbose("MQTTnet clearing {HasBroker}", broker != null);

            var server = broker;
            if (server != null)
            {
                server.InterceptingPublishAsync -= Broker_InterceptingPublishAsync;
                Log.Verbose("MQTTnet clearing, removed events");
            }

            cancellationTokenSource?.Cancel();
            cancellationTokenSource = null;
            Log.Verbose("MQTTnet clearing, cancelled async token");

            if (server == null)
                return;

            // broker?.StopAsync().GetAwaiter().GetResult(); // this line was freezing Unity editor, so calling Task.Run().Wait() instead
            try
            {
                var stopTask = Task.Run(() => server.StopAsync());
                if (!stopTask.Wait(TimeSpan.FromSeconds(5))) // give it five secs to complete
                    Log.Error("MQTTnet broker.StopAsync timed out.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "MQTTnet broker.StopAsync failed.");
            }

            Log.Verbose("MQTTnet clearing, stopped broker");
            try
            {
                server.Dispose();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "MQTTnet broker.Dispose failed.");
            }

            server = null;
            broker = null;

            Log.Debug("MQTTnet clearing, cleared");
        }

        private string SerializeObject<T>(T obj)
            where T : class
        {
            // Odin cant serialize string arrays, https://github.com/TeamSirenix/odin-serializer/issues/26
            //var buffer = OdinSerializer.SerializationUtility.SerializeValue<T>(obj, OdinSerializer.DataFormat.JSON);
            //var str = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);

            // Newtonsoft works fine, but needs package reference
            //var str = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

            // not working
            //var str = EditorJsonUtility.ToJson(obj);

            // needs nuget
            //System.Text.Json.JsonSerializer;

            //var str = TinyJson.JsonWriter.ToJson(obj);
            //var str = Meryel.UnityCodeAssist.ProjectData.LitJson.JsonMapper.ToJson(obj);
            var str = JsonConvert.SerializeObject(obj);

            return str;
        }

        private void SendAux(IMessage message, bool logContent = true)
        {
            if (message == null)
                return;

            SendAux(message.GetType().Name, message, logContent);
        }

        private void SendAux(string messageType, object content, bool logContent = true)
        {
            if (logContent)
                Log.Debug("Publishing {MessageType} {@Content}", messageType, content);
            else
                Log.Debug("Publishing {MessageType}", messageType);

            var publisher = broker;
            if (publisher != null)
                //publisher.SendMoreFrame(messageType).SendFrame(SerializeObject(content));
            {
                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("sc/" + messageType) // sc/ => server->client message
                    .WithRetainFlag(false)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                    .WithPayload(SerializeObject(content))
                    .Build();

                //broker?.InjectApplicationMessage(new InjectedMqttApplicationMessage(applicationMessage), cancellationTokenSource?.Token ?? default).GetAwaiter().GetResult();
                broker?.InjectApplicationMessage(new InjectedMqttApplicationMessage(applicationMessage),
                    cancellationTokenSource?.Token ?? default);
            }
            else
            {
                Log.Error("Publisher socket is null");
            }
        }

        public void SendConnect()
        {
            var connect = Self;

            SendAux(connect);
        }

        public void SendDisconnect()
        {
            var disconnect = new Disconnect
            {
                ModelVersion = Self.ModelVersion,
                ProjectPath = Self.ProjectPath,
                ProjectName = Self.ProjectName,
                ContactInfo = Self.ContactInfo,
                AssemblyVersion = Self.AssemblyVersion,
                LiteOrFull = Self.LiteOrFull,
                NodeKind = Self.NodeKind,
                ClientId = Self.ClientId
            };

            SendAux(disconnect);
        }

        public void SendConnectionInfo()
        {
            var connectionInfo = new ConnectionInfo
            {
                ModelVersion = Self.ModelVersion,
                ProjectPath = Self.ProjectPath,
                ProjectName = Self.ProjectName,
                ContactInfo = Self.ContactInfo,
                AssemblyVersion = Self.AssemblyVersion,
                LiteOrFull = Self.LiteOrFull,
                NodeKind = Self.NodeKind,
                ClientId = Self.ClientId
            };

            SendAux(connectionInfo);
        }

        public void SendHandshake()
        {
            var handshake = new Handshake();

            SendAux(handshake);
        }

        public void SendRequestInternalLog()
        {
            var requestInternalLog = new RequestInternalLog();

            SendAux(requestInternalLog);
        }

        public void SendRequestUpdate(string app, string path, bool isInteractive)
        {
            var requestUpdate = new RequestUpdate
            {
                App = app,
                Path = path,
                IsInteractive = isInteractive
            };

            SendAux(requestUpdate);
        }

        public void SendInternalLog()
        {
            var internalLog = new InternalLog
            {
                LogContent = ELogger.GetInternalLogContent()
            };

            SendAux(internalLog, false);
        }


        private void SendStringArrayAux(string id, string[] array)
        {
            var stringArray = new StringArray
            {
                Id = id,
                Array = array
            };

            SendAux(stringArray);
        }

        private void SendStringArrayContainerAux(params (string id, string[] array)[] container)
        {
            var stringArrayContainer = new StringArrayContainer
            {
                Container = new StringArray[container.Length]
            };

            for (var i = 0; i < container.Length; i++)
                stringArrayContainer.Container[i] = new StringArray
                {
                    Id = container[i].id,
                    Array = container[i].array
                };

            SendAux(stringArrayContainer);
        }

        public void SendTags(string[] tags)
        {
            SendStringArrayAux(Ids.Tags, tags);
        }

        public void SendLayers(string[] layerNames, string[] layerIndices)
        {
            SendStringArrayContainerAux(
                (Ids.Layers, layerNames),
                (Ids.LayerIndices, layerIndices));
        }

        public void SendSortingLayers(string[] sortingLayers, string[] sortingLayerIds, string[] sortingLayerValues)
        {
            SendStringArrayContainerAux(
                (Ids.SortingLayers, sortingLayers),
                (Ids.SortingLayerIds, sortingLayerIds),
                (Ids.SortingLayerValues, sortingLayerValues));
        }

        public void SendRenderingLayers(string[] renderingLayers, string[] renderingLayerIndices)
        {
            SendStringArrayContainerAux(
                (Ids.RenderingLayers, renderingLayers),
                (Ids.RenderingLayerIndices, renderingLayerIndices));
        }

        public void SendPlayerPrefs(string[] playerPrefKeys, string[] playerPrefValues,
            string[] playerPrefStringKeys, string[] playerPrefIntegerKeys, string[] playerPrefFloatKeys)
        {
            SendStringArrayContainerAux(
                (Ids.PlayerPrefKeys, playerPrefKeys),
                (Ids.PlayerPrefValues, playerPrefValues),
                (Ids.PlayerPrefStringKeys, playerPrefStringKeys),
                (Ids.PlayerPrefIntegerKeys, playerPrefIntegerKeys),
                (Ids.PlayerPrefFloatKeys, playerPrefFloatKeys)
            );
        }

        public void SendEditorPrefs(string[] editorPrefKeys, string[] editorPrefValues,
            string[] editorPrefStringKeys, string[] editorPrefIntegerKeys, string[] editorPrefFloatKeys,
            string[] editorPrefBooleanKeys)
        {
            SendStringArrayContainerAux(
                (Ids.EditorPrefKeys, editorPrefKeys),
                (Ids.EditorPrefValues, editorPrefValues),
                (Ids.EditorPrefStringKeys, editorPrefStringKeys),
                (Ids.EditorPrefIntegerKeys, editorPrefIntegerKeys),
                (Ids.EditorPrefFloatKeys, editorPrefFloatKeys),
                (Ids.EditorPrefBooleanKeys, editorPrefBooleanKeys)
            );
        }

        public void SendInputManager(string[] axisNames, string[] axisInfos, string[] buttonKeys, string[] buttonAxis,
            string[] joystickNames)
        {
            SendStringArrayContainerAux(
                (Ids.InputManagerAxes, axisNames),
                (Ids.InputManagerAxisInfos, axisInfos),
                (Ids.InputManagerButtonKeys, buttonKeys),
                (Ids.InputManagerButtonAxis, buttonAxis),
                (Ids.InputManagerJoystickNames, joystickNames)
            );
        }

        public void SendSceneList(string[] sceneNames, string[] scenePaths, string[] sceneBuildIndices,
            string[] sceneNamesAndPaths, string[] scenePathsAndNames)
        {
            SendStringArrayContainerAux(
                (Ids.SceneNames, sceneNames),
                (Ids.ScenePaths, scenePaths),
                (Ids.SceneBuildIndices, sceneBuildIndices),
                (Ids.SceneNamesAndPaths, sceneNamesAndPaths),
                (Ids.ScenePathsAndNames, scenePathsAndNames)
            );
        }

        public void SendScriptMissing(string component)
        {
            var scriptMissing = new ScriptMissing
            {
                Component = component
            };

            SendAux(scriptMissing);
        }

        public void SendComponentHumanTrait(string[] bones, string[] muscles)
        {
            //var humanTrait = new Synchronizer.Model.Components.HumanTrait();

            var boneIndices = new string[bones.Length];
            var boneNames = new string[bones.Length];
            for (var i = 0; i < bones.Length; i++)
            {
                boneIndices[i] = i.ToString();
                boneNames[i] = bones[i];
            }

            var muscleIndices = new string[muscles.Length];
            var muscleNames = new string[muscles.Length];
            for (var i = 0; i < muscles.Length; i++)
            {
                muscleIndices[i] = i.ToString();
                muscleNames[i] = muscles[i];
            }

            SendStringArrayContainerAux(
                (Ids.AnimationHumanBones, boneNames),
                (Ids.AnimationHumanBoneIndices, boneIndices),
                (Ids.AnimationHumanMuscles, muscleNames),
                (Ids.AnimationHumanMuscleIndices, muscleIndices)
            );
        }

        public void SendShaderGlobalKeywords()
        {
            SendStringArrayAux(Ids.ShaderGlobalKeywords, Shader.globalKeywords.Select(k => k.name).ToArray());
        }

        public void SendGameObject(UnityEngine.GameObject go)
        {
            if (!go)
                return;

            Log.Debug("SendGO: {GoName}", go.name);

            var dataOfSelf = go.ToSyncModel(10000);
            if (dataOfSelf != null)
                SendAux(dataOfSelf);

            var dataOfHierarchy = go.ToSyncModelOfHierarchy();
            if (dataOfHierarchy != null)
                foreach (var doh in dataOfHierarchy)
                    SendAux(doh);

            var dataOfComponents = go.ToSyncModelOfComponents();
            if (dataOfComponents != null)
                foreach (var doc in dataOfComponents)
                    SendAux(doc);

            var dataOfComponentAnimator = go.ToSyncModelOfComponentAnimator();
            if (dataOfComponentAnimator != null)
                SendAux(dataOfComponentAnimator);

            var dataOfComponentAnimation = go.ToSyncModelOfComponentAnimation();
            if (dataOfComponentAnimation != null)
                SendAux(dataOfComponentAnimation);

            var dataOfComponentMaterial = go.ToSyncModelOfComponentMaterial();
            if (dataOfComponentMaterial != null)
                SendAux(dataOfComponentMaterial);
        }

        public void SendScriptableObject(ScriptableObject so)
        {
            Log.Debug("SendSO: {SoName}", so.name);

            var dataOfSo = so.ToSyncModel();
            if (dataOfSo != null)
                SendAux(dataOfSo);
        }

        public void SendAnalyticsEvent(string type, string content)
        {
            var analyticsEvent = new AnalyticsEvent
            {
                EventType = type,
                EventContent = content
            };
            SendAux(analyticsEvent);
        }

        public void SendErrorReport(string errorMessage, string stack, string type)
        {
            var errorReport = new ErrorReport
            {
                ErrorMessage = errorMessage,
                ErrorStack = stack,
                ErrorType = type
            };
            SendAux(errorReport);
        }

        public void SendRequestVerboseType(string type, string docPath)
        {
            var requestVerboseType = new RequestVerboseType
            {
                Type = type,
                DocPath = docPath
            };
            SendAux(requestVerboseType);
        }

        public void ForwardRelayMessage(IRelayMessage relayMessage)
        {
            SendAux(relayMessage);
        }
    }
}