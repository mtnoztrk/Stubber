﻿using StubberProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StubberProject
{
    public interface IOutputter
    {
        Task OutputStubs(string outputName, Dictionary<string, Dictionary<string, object>> StubValues);
        Task OutputSnippets(string outputName, Dictionary<string, StubSnippet> SnippetValues);
    }
}
