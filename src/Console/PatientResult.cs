using System;

namespace Upload
{
	public sealed class PatientResult : Patient
	{
		public PatientResult(Patient patient)
		{
			FirstName = patient.FirstName;
		}

		public DateTime BeginTime { get; set; }
		public DateTime EndTime { get; set; }
		public bool Failed { get => Exception != null; }
		public Exception Exception { get; set; }
	}
}