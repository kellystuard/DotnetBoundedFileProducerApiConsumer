using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CommandLine;

namespace Upload
{
	// Look at TODO's for where work is being simulated
	public static class Program
	{
		/// <summary>
		/// Given a file name, loads as CSV and posts them to to the consumer.
		/// </summary>
		/// <remarks>
		/// When the consumer's buffer is full, this method waits for more space.
		/// </remarks>
		/// <param name="fileName">Name of file to read.</param>
		/// <param name="consumer">Consumer that has each line of the file posted to it. The consumer is then completed.</param>
		/// <returns>Task that can be waited on to signal that all lines have been posted to the consumer.</returns>
		private static async Task ProduceLinesFromFile(string fileName, ITargetBlock<string> consumer)
		{
			for (int i = 0; i < 150; i++)
			{
				//TODO: replace with actual disk read; make sure your read call does not block (use await)
				// simulating how long a disk read could take
				await Task.Delay(new Random().Next(1, 100));

				Console.Write(".");
				await consumer.SendAsync($"Line {i + 1}");
			}
			consumer.Complete();
		}

		/// <summary>
		/// Given a patient, async-writes to the API and returns the results.
		/// </summary>
		/// <param name="patient">Patient to write to the API.</param>
		/// <returns>Result of the API call.</returns>
		private static async Task<PatientResult> WritePatientToApi(Patient patient)
		{
			//TODO: If you decide to have multiple patient result records per API call, this signature could change to return Task<IEnumerable<PatientResult>
			// and the next data flow block take in the enumerable or would be TransformManyBlock instead of TransformBlock

			var result = new PatientResult(patient);
			try
			{
				result.BeginTime = DateTime.UtcNow;

				//TODO: replace with actual API POST; make sure your API call does not block (use await)
				// simulating 10% failure rate of API call
				if (new Random().Next(10) == 0)
					throw new Exception();
				// simulating how long an API call could take
				await Task.Delay(new Random().Next(1000, 5000));

				Console.Write("*");

			}
			catch (Exception ex)
			{
				result.Exception = ex;
				Console.Write("X");
			}
			finally
			{
				result.EndTime = DateTime.UtcNow;
			}
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="patientResult"></param>
		/// <returns></returns>
		private static async Task WritePatientLog(PatientResult patientResult)
		{
			//TODO: Change method signature to take in requirements shared over invocations, such as file stream
			//TODO: Here you would async log to file the begin/end UTC timestamps for the api call (part of PatientResult); use await
		}

		private static async Task WritePatientDetails(PatientResult patientResult)
		{
			//TODO: Change method signature to take in requirements shared over invocations, such as file stream
			//TODO: Here you would async write to file the PatientResult; use await
		}

		static int Main(string[] args)
		{
			var result = Parser.Default.ParseArguments<Options>(args)
				.WithParsed(RunUpload)
				.WithNotParsed(RunError)
				.Tag;

			return (int)result;
		}

		private static void RunUpload(Options o)
		{
			Console.WriteLine("Uploading from file to API. File: . API success: * API fail: X");

			// single-threaded producer with a configurable buffer
			var producerDataOptions = new ExecutionDataflowBlockOptions
			{
				BoundedCapacity = o.BufferCount,
				MaxDegreeOfParallelism = 1,
				EnsureOrdered = false,
			};
			// multi-threaded consumer with a configurable threading
			var consumerDataOptions = new ExecutionDataflowBlockOptions
			{
				BoundedCapacity = o.ParallelCount,
				MaxDegreeOfParallelism = o.ParallelCount,
				EnsureOrdered = false,
			};

			var lineToPatient = new TransformBlock<string, Patient>(source => Patient.Parse(source), producerDataOptions);
			var patientToApiResult = new TransformBlock<Patient, PatientResult>(source => WritePatientToApi(source), consumerDataOptions);
			var logApiResult = new ActionBlock<PatientResult>(source => WritePatientLog(source), producerDataOptions);
			var writePatientToFile = new ActionBlock<PatientResult>(source => WritePatientDetails(source), producerDataOptions);

			var linkOptions = new DataflowLinkOptions()
			{
				PropagateCompletion = true,
			};
			lineToPatient.LinkTo(patientToApiResult, linkOptions);
			patientToApiResult.LinkTo(logApiResult, linkOptions);
			patientToApiResult.LinkTo(writePatientToFile, linkOptions);

			var producer = ProduceLinesFromFile(o.Filename, lineToPatient);

			// wait for the producer and beginning/end of the consumer pipeline to be complete
			Task.WaitAll(
				producer,
				lineToPatient.Completion,
				logApiResult.Completion,
				writePatientToFile.Completion
			);
			Console.WriteLine();
			Console.WriteLine("Done.");
		}

		private static void RunError(IEnumerable<Error> errors)
		{
			throw new NotImplementedException();
		}
	}
}
