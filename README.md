# R136.Web
This is a web version of a text adventure a friend and I created all the way back in 1998. 
More information about the game and its history can be found in the README of the 
["console version" GitHub repo](https://github.com/rbergen/R136).

Since its resurrection, R136 has become a willing vehicle for me to try new things. This 
single-pager "static" web application marks my first dabble with developing for/in/using:
* [.NET 5.0](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to?pivots=dotnet-5-0)
* Version 9.0 of C#. For one, I've quickly developed a liking for 
[switch expressions](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/switch-expression) 
and [range operators](https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/ranges-indexes).
* Web development using [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor). The embedding of HTML 
in code did bring back some memories of classic ASP and even ColdFusion, but my end conclusion is that the overall 
implementation is in fact a lot more elegant, if used right. Particularly the ability to back razor files with 
partial "pure C#" classes makes things a lot more manageable.
* [Blazor WebAssembly](https://docs.microsoft.com/en-gb/aspnet/core/blazor/?view=aspnetcore-5.0#blazor-webassembly), 
and by extension, [WebAssembly](https://webassembly.org/) in general. 
* .NET [configuration](https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration-providers) 
and [logging](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging) extensions.
* [Serilog](https://serilog.net/)
* [ASP.NET dependency injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-5.0).
* [Bootstrap 5.0](https://getbootstrap.com/docs/5.0/getting-started/introduction/).
* Browser local storage, via [Blazored LocalStorage](https://github.com/Blazored/LocalStorage).
* Real-time language switching in the context described by the previous points.

Concerning the the game itself:
* It has been converted from a purely C-like procedural implementation to an object-oriented one.
* I've added a console ("shell") front-end to the game engine used by the web front-end, just for good measure 
(and yes, it does come with the ASCII art animation of the original C(++) console game!)
* Much of it (including all visible texts) can now be configured through JSON files. 
* It's now available in English, too!
