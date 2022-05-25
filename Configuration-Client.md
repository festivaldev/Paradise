# Client Configuration
This is a list of available client options, which can be configured in `<path to UberStrike>\UberStrike_Data\ParadiseSettings.Client.xml`.

| Key | Type | Default | Description |
|---|---|---|---|
| WebServiceBaseUrl | String | `https://localhost:8080/2.0/` | The URL for the client to connect to the Web Services. If connecting to a server other than `localhost`, you need to enter its IP or FQDN, followed by the port and the API Version (`2.0` for UberStrike 4.7.1, `1.0.1` for [[legacy versions|Legacy-Support]])
| ImagePath | String | `https://localhost:8081/` | The URL for the client to download map thumbnails. If connecting to a server other than `localhost`, you need to enter its IP or FQDN, followed by the port.
| UpdateUrl | String | `https://localhost:8081/updates` | The URL for the client to download updates from, usually the same address as the File Server but with `/updates` appended. If connecting to a server other than `localhost`, you need to enter its IP or FQDN, followed by the port and the path to be used for updates.
| WebServicePrefix | String | `UberStrike.DataCenter.WebService.CWS.` | The prefix used for web service names. **Currently unused!**
| WebServiceSuffix | String | `Contract.svc` | The suffix used for web service names. **Currently unused!**