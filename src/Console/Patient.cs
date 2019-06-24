using System;
using System.Threading.Tasks;

namespace Upload
{
	public class Patient
	{
		public static Patient Parse(string line)
		{
			//TODO: Use CSV parsing NuGet package to parse line into patient object
			return new Patient
			{
				FirstName = line,
			};
		}

		public string FirstName { get; set; }
		public string LastName { get; set; }
	}
}