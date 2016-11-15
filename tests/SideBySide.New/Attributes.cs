﻿using System;
using Xunit;

namespace SideBySide.New
{
	public class CachedProcedureTheoryAttribute : TheoryAttribute
	{
		public CachedProcedureTheoryAttribute()
		{
			if(!AppConfig.SupportsCachedProcedures)
				Skip = "No Cached Procedure Support";
		}
	}

	public class JsonTheoryAttribute : TheoryAttribute
	{
		public JsonTheoryAttribute()
		{
			if(!AppConfig.SupportsJson)
				Skip = "No JSON Support";
		}
	}

	public class PasswordlessUserFactAttribute : FactAttribute
	{
		public PasswordlessUserFactAttribute()
		{
			if(string.IsNullOrWhiteSpace(AppConfig.PasswordlessUser))
				Skip = "No passwordless user";
		}

	}

	public class TcpConnectionFactAttribute : FactAttribute
	{
		public TcpConnectionFactAttribute()
		{
			var csb = AppConfig.CreateConnectionStringBuilder();
			if(csb.Server.StartsWith("/", StringComparison.Ordinal) || csb.Server.StartsWith("./", StringComparison.Ordinal))
				Skip = "Not a TCP Connection";
		}
	}
}
