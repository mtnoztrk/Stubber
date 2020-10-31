using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Options;
using StubberProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace StubberProject.Helpers
{
    interface IOutputManager : IProcessor
    {
        bool IsRecording { get; set; }
        void StartRecording(string methodUnderTest);
        void StopRecording();
        void AddToStubValues(string methodName, Dictionary<string, object> localResults);
        void AddToMethodSignatures(string methodName, StubSnippet snippet);
        void OutputStubs();
        void OutputSnippets();
    }

    internal class OutputManager : IOutputManager
    {
        private readonly IProcessor _processor;
        private readonly IOutputter _outputter;
        private string _outputFileName { get; set; }
        private StubberOption _config { get; set; }
        public bool IsRecording { get; set; }


        /// <summary>
        /// used for printing values used in Moq methods
        /// </summary>
        internal Dictionary<string, Dictionary<string, object>> StubValues = new Dictionary<string, Dictionary<string, object>>();
        /// <summary>
        /// used for printing methods for Moq
        /// </summary>
        internal Dictionary<string, StubSnippet> SnippetValues = new Dictionary<string, StubSnippet>();

        public OutputManager(IOptions<StubberOption> options, IProcessor processor, IOutputter outputter)
        {
            _config = options.Value;
            IsRecording = false;
            _processor = processor;
            _outputter = outputter;
        }

        public void StartRecording(string methodUnderTest)
        {
            _outputFileName = $"{methodUnderTest}_{DateTime.Now}";
            IsRecording = true;
        }

        public void StopRecording()
        {
            IsRecording = false;
        }

        public Dictionary<string, object> ProcessArguments(MethodBase methodMetadata, object[] args)
        {
            return _processor.ProcessArguments(methodMetadata, args);
        }

        public Dictionary<string, object> ProcessResult(MethodBase methodMetadata, object[] args, object result)
        {
            return _processor.ProcessResult(methodMetadata, args, result);
        }

        public void AddToStubValues(string methodName, Dictionary<string, object> results)
        {
            if (StubValues.ContainsKey(methodName))
            {
                // might throw eksepsiyon :(
                var combinedDictionary = StubValues[methodName].Union(results).ToDictionary(k => k.Key, v => v.Value);
                StubValues[methodName] = combinedDictionary;
            }
            else
            {
                StubValues.Add(methodName, results);
            }
        }

        public void AddToMethodSignatures(string methodName, StubSnippet snippet)
        {
            SnippetValues.Add(methodName, snippet);
        }

        public void OutputStubs()
        {
            _outputter.OutputStubs(_outputFileName, StubValues);
        }

        public void OutputSnippets()
        {
            _outputter.OutputSnippets(_outputFileName, SnippetValues);
        }
    }
}
