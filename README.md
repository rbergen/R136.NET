# R136.Web
This is a web version of a text adventure a friend and I created all the way back in 1998. More information about the game and its history can be found in the README of the ["console version" GitHub repo](https://github.com/rbergen/R136).

Since its resurrection, R136 has become a willing vehicle for me to try new things. This single-pager "static" web application marks my first dabble with developing for/in/using:
* [.NET 5.0](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to?pivots=dotnet-5-0)
* Version 8.0 of C#. For one, I've quickly developed a liking for [switch expressions](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/switch-expression) and [range operators](https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/ranges-indexes).
* Web development using [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor). The embedding of HTML in code did bring back some memories of classic ASP and even ColdFusion, but my end conclusion is that the overall implementation is in fact a lot more elegant, if used right. Particularly the ability to back razor files with partial "pure C#" classes makes things a lot more manageable.
* [Blazor WebAssembly](https://docs.microsoft.com/en-gb/aspnet/core/blazor/?view=aspnetcore-5.0#blazor-webassembly), and by extension, [WebAssembly](https://webassembly.org/) in general. 
* [ASP.NET dependency injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-5.0).
* [Bootstrap 5.0](https://getbootstrap.com/docs/5.0/getting-started/introduction/).
* Browser local storage, via [Blazored LocalStorage](https://github.com/Blazored/LocalStorage).

The game itself has been converted from a purely C-like procedural implementation to an object-oriented one. Furthermore, much of it (including all visual texts) can now be configured through JSON files. (Amongst others, this means that a next step in the game's development could be an English version. Maybe I'll develop that itch at some point in the future.) 
