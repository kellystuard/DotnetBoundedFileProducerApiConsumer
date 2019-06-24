using System;
using CommandLine;

namespace Upload
{
	public sealed class Options
	{
		[Value(0, MetaName = "file-name", HelpText = "CSV file source for API calls.")]
		public string Filename { get; internal set; }
		[Option('b', "buffer-count", HelpText = "Number of file lines to buffer to the API.")]
		public int BufferCount { get; internal set; } = 100;
		[Option('p', "parallel-count", HelpText = "Number of simultaneous requests to make to the API. Defaults to logical processor count.")]
		public int ParallelCount { get; internal set; } = Environment.ProcessorCount;
	}
}