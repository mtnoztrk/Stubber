using Microsoft.Extensions.Options;
using StubberProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StubberProject.Attributes;

namespace StubberProject.Helpers
{
    /// <summary>
    /// Coordinates <see cref="StubberAttribute"/> and <see cref="StubberTargetAttribute"/>.
    /// </summary>
    interface IStubberManager : IProcessor
    {
        bool IsRecording { get; set; }
        void StartRecording(string methodUnderTest);
        void StopRecording();
        void AddToStubValues(string methodName, Dictionary<string, object> localResults);
        void AddToSnippetValues(string snippet);
    }

    /// <summary>
    /// Coordinates <see cref="StubberAttribute"/> and <see cref="StubberTargetAttribute"/>.
    /// </summary>
    internal class StubberManager : IStubberManager
    {
        private readonly IProcessor _processor;
        private readonly IOutputter _outputter;
        private string _outputName { get; set; }
        private StubberOption _config { get; set; }
        public bool IsRecording { get; set; }


        /// <summary>
        /// used for printing values that are used in Moq methods
        /// </summary>
        private Dictionary<string, Dictionary<string, object>> _stubValues = new Dictionary<string, Dictionary<string, object>>();
        /// <summary>
        /// used for printing methods for Moq
        /// </summary>
        private List<string> _snippetValues = new List<string>();

        public StubberManager(IOptions<StubberOption> options, IProcessor processor, IOutputter outputter)
        {
            _config = options.Value;
            IsRecording = false;
            _processor = processor;
            _outputter = outputter;
        }

        public void StartRecording(string methodUnderTest)
        {
            if (IsRecording)
                throw new InvalidOperationException("You are trying to test two methods at once please dont :(");
            _outputName = $"{methodUnderTest}_{DateTime.Now.ToString("dd_MMM_HH_mm_ss")}";
            _stubValues.Clear();
            _snippetValues.Clear();
            IsRecording = true;
        }

        public void StopRecording()
        {
            IsRecording = false;
            _outputter.OutputStubs(_outputName, _stubValues);
            _outputter.OutputSnippets(_outputName, _snippetValues);
        }

        public Dictionary<string, object> ProcessArguments(MethodBase methodMetadata, object[] args)
        {
            return _processor.ProcessArguments(methodMetadata, args);
        }

        public Dictionary<string, object> ProcessResult(MethodBase methodMetadata, object[] args, object result)
        {
            return _processor.ProcessResult(methodMetadata, args, result);
        }

        public string ProcessSnippet(MethodBase methodMetadata, string jsonAccessor)
        {
            return _processor.ProcessSnippet(methodMetadata, jsonAccessor);
        }

        public string GenerateMethodEntry(MethodBase methodBase, string jsonAccessor, string outputName = null)
        {
            return _processor.GenerateMethodEntry(methodBase, jsonAccessor, _outputName);
        }

        public void AddToStubValues(string methodName, Dictionary<string, object> results)
        {
            if (_stubValues.ContainsKey(methodName))
            {
                // might throw eksepsiyon :(
                var combinedDictionary = _stubValues[methodName].Union(results).ToDictionary(k => k.Key, v => v.Value);
                _stubValues[methodName] = combinedDictionary;
            }
            else
            {
                _stubValues.Add(methodName, results);
            }
        }

        public void AddToSnippetValues(string snippet)
        {
            _snippetValues.Add(snippet);
        }
    }
}
