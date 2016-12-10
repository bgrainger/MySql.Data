﻿using System;
using System.IO;
using System.Text;
using MySql.Data.Serialization;

namespace MySql.Data.MySqlClient
{
	internal sealed class MySqlStatementPreparer
	{
		public MySqlStatementPreparer(string commandText, MySqlParameterCollection parameters, StatementPreparerOptions options)
		{
			m_commandText = commandText;
			m_parameters = parameters;
			m_options = options;
		}

		public ArraySegment<byte> ParseAndBindParameters()
		{
			using (var stream = new MemoryStream(m_commandText.Length))
			using (var writer = new BinaryWriter(stream, Encoding.UTF8))
			{
				writer.Write((byte) CommandKind.Query);

				if (!string.IsNullOrWhiteSpace(m_commandText))
				{
					var parser = new ParameterSqlParser(this, writer);
					parser.Parse(m_commandText);
				}

#if NETSTANDARD1_3
				var array = stream.ToArray();
#else
				var array = stream.GetBuffer();
#endif
				return new ArraySegment<byte>(array, 0, checked((int) stream.Length));
			}
		}

		private sealed class ParameterSqlParser : MySqlParser
		{
			public ParameterSqlParser(MySqlStatementPreparer preparer, BinaryWriter writer)
			{
				m_preparer = preparer;
				m_writer = writer;
			}

			protected override void OnBeforeParse(string sql)
			{
			}

			protected override void OnNamedParameter(int index, int length)
			{
				var parameterName = m_preparer.m_commandText.Substring(index, length);
				var parameterIndex = m_preparer.m_parameters.FlexibleIndexOf(parameterName);
				if (parameterIndex != -1)
					DoAppendParameter(parameterIndex, index, length);
				else if ((m_preparer.m_options & StatementPreparerOptions.AllowUserVariables) == 0)
					throw new MySqlException("Parameter '{0}' must be defined.".FormatInvariant(parameterName));
			}

			protected override void OnPositionalParameter(int index)
			{
				DoAppendParameter(m_currentParameterIndex, index, 1);
				m_currentParameterIndex++;
			}

			private void DoAppendParameter(int parameterIndex, int textIndex, int textLength)
			{
				AppendString(m_preparer.m_commandText, m_lastIndex, textIndex - m_lastIndex);
				m_preparer.m_parameters[parameterIndex].AppendSqlString(m_writer, m_preparer.m_options);
				m_lastIndex = textIndex + textLength;
			}

			protected override void OnParsed()
			{
				AppendString(m_preparer.m_commandText, m_lastIndex, m_preparer.m_commandText.Length - m_lastIndex);
			}

			private void AppendString(string value, int offset, int length)
			{
				m_writer.WriteUtf8(value, offset, length);
			}

			readonly MySqlStatementPreparer m_preparer;
			readonly BinaryWriter m_writer;
			int m_currentParameterIndex;
			int m_lastIndex;
		}

		readonly string m_commandText;
		readonly MySqlParameterCollection m_parameters;
		readonly StatementPreparerOptions m_options;
	}
}
