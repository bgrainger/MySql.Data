﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using MySql.Data.MySqlClient;

namespace MySql.Data.Serialization
{
	internal sealed class ConnectionSettings
	{
		public ConnectionSettings(MySqlConnectionStringBuilder csb)
		{
			ConnectionString = csb.ConnectionString;

			// Base Options
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && (csb.Server.StartsWith("/", StringComparison.Ordinal) || csb.Server.StartsWith("./", StringComparison.Ordinal)))
			{
				if (!File.Exists(csb.Server))
					throw new MySqlException("Cannot find Unix Socket at " + csb.Server);
				ConnectionType = ConnectionType.Unix;
				UnixSocket = Path.GetFullPath(csb.Server);
			}
			else
			{
				ConnectionType = ConnectionType.Tcp;
				Hostnames = csb.Server.Split(',');
				Port = (int) csb.Port;
			}
			UserID = csb.UserID;
			Password = csb.Password;
			Database = csb.Database;

			// SSL/TLS Options
			SslMode = csb.SslMode;
			CertificateFile = csb.CertificateFile;
			CertificatePassword = csb.CertificatePassword;

			// Connection Pooling Options
			Pooling = csb.Pooling;
			ConnectionReset = csb.ConnectionReset;
			if (csb.MinimumPoolSize > csb.MaximumPoolSize)
				throw new MySqlException("MaximumPoolSize must be greater than or equal to MinimumPoolSize");
			MinimumPoolSize = (int)csb.MinimumPoolSize;
			MaximumPoolSize = (int)csb.MaximumPoolSize;

			// Other Options
			AllowUserVariables = csb.AllowUserVariables;
			ConnectionTimeout = (int)csb.ConnectionTimeout;
			ConvertZeroDateTime = csb.ConvertZeroDateTime;
			ForceSynchronous = csb.ForceSynchronous;
			OldGuids = csb.OldGuids;
			PersistSecurityInfo = csb.PersistSecurityInfo;
			UseAffectedRows = csb.UseAffectedRows;
			UseCompression = csb.UseCompression;
		}

		public ConnectionSettings WithUseCompression(bool useCompression) => new ConnectionSettings(this, useCompression);

		private ConnectionSettings(ConnectionSettings other, bool? useCompression)
		{
			// Base Options
			ConnectionString = other.ConnectionString;
			ConnectionType = other.ConnectionType;
			Hostnames = other.Hostnames;
			Port = other.Port;
			UnixSocket = other.UnixSocket;
			UserID = other.UserID;
			Password = other.Password;
			Database = other.Database;

			// SSL/TLS Options
			SslMode = other.SslMode;
			CertificateFile = other.CertificateFile;
			CertificatePassword = other.CertificatePassword;

			// Connection Pooling Options
			Pooling = other.Pooling;
			ConnectionReset = other.ConnectionReset;
			MinimumPoolSize = other.MinimumPoolSize;
			MaximumPoolSize = other.MaximumPoolSize;

			// Other Options
			AllowUserVariables = other.AllowUserVariables;
			ConnectionTimeout = other.ConnectionTimeout;
			ConvertZeroDateTime = other.ConvertZeroDateTime;
			ForceSynchronous = other.ForceSynchronous;
			OldGuids = other.OldGuids;
			PersistSecurityInfo = other.PersistSecurityInfo;
			UseAffectedRows = other.UseAffectedRows;
			UseCompression = useCompression ?? other.UseCompression;
		}

		// Base Options
		internal readonly string ConnectionString;
		internal readonly ConnectionType ConnectionType;
		internal readonly IEnumerable<string> Hostnames;
		internal readonly int Port;
		internal readonly string UnixSocket;
		internal readonly string UserID;
		internal readonly string Password;
		internal readonly string Database;

		// SSL/TLS Options
		internal readonly MySqlSslMode SslMode;
		internal readonly string CertificateFile;
		internal readonly string CertificatePassword;

		// Connection Pooling Options
		internal readonly bool Pooling;
		internal readonly bool ConnectionReset;
		internal readonly int MinimumPoolSize;
		internal readonly int MaximumPoolSize;

		// Other Options
		internal readonly bool AllowUserVariables;
		internal readonly int ConnectionTimeout;
		internal readonly bool ConvertZeroDateTime;
		internal readonly bool ForceSynchronous;
		internal readonly bool OldGuids;
		internal readonly bool PersistSecurityInfo;
		internal readonly bool UseAffectedRows;
		internal readonly bool UseCompression;
	}

}

