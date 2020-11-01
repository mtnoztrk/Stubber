# Stubber
Helper library for writing WebAPI tests while mocking DB calls etc. Captures required data in run time and saves it as json file to desired location. Generates code for setting up mocked services with **Moq** with the data captured in run time.
# Motivation
Writing tests for complex methods takes a lot of time especially the project has no prior tests. **Stubber** automatically generates some code to jump start testing.
# Installation
Stubber is activated only in DEBUG mode. Using Stubber should not affect performance in production environment. 

`appsettings.json`
```json
{
  "Stubber": {
    "CodeFilePathPrefix": "C:\\ProjectPath\\Tests\\Documents", 
    "StubFilePathPrefix": "C:\\ProjectPath\\Tests\\DataSources"
  }
}
```
`Startup.cs`
```csharp
using StubberProject.Extensions;

public class Startup
{
    public IConfiguration Configuration { get; }
    
    public Startup(IConfiguration configuration) 
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services) 
    {
        ...
        service.UseStubber(Configuration);
        ...
    }
    
    public void Configure(IApplicationBuilder app) 
    {
        ...
        app.UseStubber();
        ...
    }
}
```
# Usage
Detailed example can be found in examples folder. (WIP)

Classes needs to be recorded should be marked with an attribute.
```csharp
using StubberProject.Attributes;

[Stubber]
public class UserService : IUserService 
{
    ...
}
```
Target method under test should be marked with an attribute. Recording session will start when this method is triggered and end after it is done. Every method's and propertyGetter's data is saved to the json file (do not forget to put JsonIgnore to sensitive fields). Every method and propertyGetter used in recorded session will be generated as **Moq** setup code under `CodeFilePathPrefix` folder. 
```csharp
using StubberProject.Attributes;

public class BlogService : IBlogService
{
    public BlogService (IUserService userService)
    {
        ...
    }

    [StubberTarget]
    public int GetUsersBlogCount(Guid userId)
    {
        var user = _userService.GetUser(userId);
        ...
    }
}
```
# Lisense
MIT License

Copyright (c) 2020 Metin ÖZTÜRK

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.