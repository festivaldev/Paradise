# Server Configuration
## Web Services
This is a list of available web service options, which can be configured in `<path to Paradise.WebServices>\ParadiseSettings.Server.xml`.

| Key | Type | Default | Description |
|---|---|---|---|
| EnableSSL | Boolean | `true` | Controls the usage of HTTPS. Requires SSL certificates to be bound to selected ports (see "Setting up HTTPS/SSL").
| WebServiceHostName | String | `localhost` | Sets the hostname that Web Services are listening on. Can also be an IP address or FQDN. Certificate names must match server hostnames!
| WebServicePort | Number | `8080` | Sets the port that Web Services are listening on. Allowed values are `1024`-`65535`.
| WebServicePrefix | String | `UberStrike.DataCenter.WebService.CWS.` | The prefix used for web service names. _Currently unused by clients – this setting should not be changed._
| WebServiceSuffix | String | `Contract.svc` | The suffix used for web service names. _Currently unused by clients – this setting should not be changed._
| FileServerHostName | String | `localhost` | Sets the hostname that the File Server is listening on. Can also be an IP address or FQDN. Certificate names must match server hostnames! Also serves as the host name for the update system.
| FileServerPort | Number | `8081` | Sets the port that File Server listening on. Allowed values are `1024`-`65535`.

## Realtime Server
This is a list of settings used by the Photon realtime server, which can be configured in `<path to Paradise.Realtime>\bin\Paradise.Realtime.json`.  
If no such file exists, default values are used instead.

| Key | Type | Default | Description |
|---|---|---|---|
| compositeHashes | Array&lt;String&gt; | `[]` | Used for heartbeat data calculation. Still don't know what this is used for. |
| junkHashes | Array&lt;String&gt; | `[]` | Dito |
| webServiceBaseUrl | String | `https://localhost:5053/2.0` | Used by the realtime server to connect to the web service API. If both the Web Services and the Realtime server are running on the same machine, this can be left at default. Otherwise, you need to replace `localhost` with the IP address or FQDN of the server the Web Services are running on.
| heartbeatInterval | Int | `5` | The interval in which to send heartbeat checks to clients
| heartbeatTimeout | Int | `5` | The initial timeout to wait after client connection to send heartbeat checks