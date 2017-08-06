using NUnitLite;

namespace NHibernate.Test
{
	class Program
	{
		/// <summary>
		/// Takes arguments like NUnit Config - https://github.com/nunit/docs/wiki/Console-Command-Line#options
		/// </summary>
		static int Main(string[] args)
		{
			return new AutoRun(typeof(Program).Assembly).Execute(args);
		}
	}
}
