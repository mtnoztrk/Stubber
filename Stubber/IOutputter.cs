using StubberProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StubberProject
{
    public interface IOutputter
    {
        Task OutputStubs(string methodName, Dictionary<string, Dictionary<string, object>> StubValues);
        Task OutputSnippets(string methodName, Dictionary<string, StubSnippet> MethodSignatures);
    }
}
