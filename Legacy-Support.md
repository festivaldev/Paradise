# Legacy Support

Paradise includes limited support for getting legacy versions of UberStrike HD (such as [4.3.8](http://web.archive.org/web/20121101205430/http://static.cmune.com/UberStrike/Client/UberStrike-4.3.8-Setup.exe), [4.3.9](http://web.archive.org/web/20121127142747/http://static.cmune.com/UberStrike/Client/UberStrike-4.3.9-Setup.exe) and [4.3.10.1](http://web.archive.org/web/20130131044157/http://static.cmune.com/UberStrike/Client/UberStrike-4.3.10.1-Setup.exe)) to run. This includes the main menu, the shooting range and a range of maps to explore.

To enable legacy support, replace the following lines of code with the code below in `Paradise.WebServices.ServiceHost\ParadiseService.cs` (starting at line 73):
```csharp
// old code
Services = new Dictionary<string, WebServiceBase> {
    ["Application"] = new ApplicationWebService(HttpBinding, WebServiceSettings, this),
    ["Authentication"] = new AuthenticationWebService(HttpBinding, WebServiceSettings, this),
    ["Clan"] = new ClanWebService(HttpBinding, WebServiceSettings, this),
    ["Moderation"] = new ModerationWebService(HttpBinding, WebServiceSettings, this),
    ["PrivateMessage"] = new PrivateMessageWebService(HttpBinding, WebServiceSettings, this),
    ["Relationship"] = new RelationshipWebService(HttpBinding, WebServiceSettings, this),
    ["Shop"] = new ShopWebService(HttpBinding, WebServiceSettings, this),
    ["User"] = new UserWebService(HttpBinding, WebServiceSettings, this)
};

// new code
Services = new Dictionary<string, WebServiceBase> {
    ["Application"] = new ApplicationWebService_Legacy(HttpBinding, WebServiceSettings, this),
    ["Authentication"] = new AuthenticationWebService_Legacy(HttpBinding, WebServiceSettings, this),
    ["Clan"] = new ClanWebService_Legacy(HttpBinding, WebServiceSettings, this),
    ["Moderation"] = new ModerationWebService_Legacy(HttpBinding, WebServiceSettings, this),
    ["PrivateMessage"] = new PrivateMessageWebService_Legacy(HttpBinding, WebServiceSettings, this),
    ["Relationship"] = new RelationshipWebService_Legacy(HttpBinding, WebServiceSettings, this),
    ["Shop"] = new ShopWebService_Legacy(HttpBinding, WebServiceSettings, this),
    ["User"] = new UserWebService_Legacy(HttpBinding, WebServiceSettings, this)
    
    // TL;DR: Append "_Legacy" to all service constructors
};
```

Legacy support allows you to launch the game, bypass the login screen (just enter any valid – as in [RFC 5322](https://datatracker.ietf.org/doc/html/rfc5322) formatted – e-mail address and password) and thus, reach the main menu. From there, you can either try a very limited range of weapons on the shooting range or explore a few maps included in the game.

Please note that legacy content is heavily incomplete and will be expanded over time.