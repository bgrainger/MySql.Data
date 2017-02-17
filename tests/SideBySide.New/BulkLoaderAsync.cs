﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Xunit;
using Dapper;

namespace SideBySide
{
	[Collection("BulkLoaderCollection")]
	public class BulkLoaderAsync : IClassFixture<DatabaseFixture>
	{
		public BulkLoaderAsync(DatabaseFixture database)
		{
			m_database = database;
			//xUnit runs tests in different classes in parallel, so use different table names for the different test classes
			string testClient;
#if BASELINE
			testClient = "Baseline";
#else
			testClient = "New";
#endif
			m_testTable = "test.BulkLoaderAsyncTest" + testClient;

			var initializeTable = @"
				create schema if not exists test;
				drop table if exists " + m_testTable + @";
				CREATE TABLE " + m_testTable + @"
				(
					one int primary key
					, ignore_one int
					, two varchar(200)
					, ignore_two varchar(200)
					, three varchar(200)
					, four datetime
					, five blob
				) CHARACTER SET = UTF8;";
			m_database.Connection.Execute(initializeTable);

			m_memoryStreamBytes = System.Text.Encoding.UTF8.GetBytes(@"1,'two-1','three-1'
2,'two-2','three-2'
3,'two-3','three-3'
4,'two-4','three-4'
5,'two-5','three-5'
");
		}

		[BulkLoaderTsvFileFact]
		public async Task BulkLoadTsvFile()
		{
			using (MySqlConnection connection = new MySqlConnection(AppConfig.ConnectionString))
			{
				MySqlBulkLoader bl = new MySqlBulkLoader(connection);
				bl.FileName = AppConfig.MySqlBulkLoaderTsvFile;
				bl.TableName = m_testTable;
				bl.Columns.AddRange(new string[] { "one", "two", "three", "four", "five" });
				bl.NumberOfLinesToSkip = 1;
				bl.Expressions.Add("five = UNHEX(five)");
				bl.Local = false;
				int rowCount = await bl.LoadAsync();
				Assert.Equal(20, rowCount);
			}
		}

		[BulkLoaderLocalTsvFileFact]
		public async Task BulkLoadLocalTsvFile()
		{
			using (MySqlConnection connection = new MySqlConnection(AppConfig.ConnectionString))
			{
				MySqlBulkLoader bl = new MySqlBulkLoader(connection);
				bl.FileName = AppConfig.MySqlBulkLoaderLocalTsvFile;
				bl.TableName = m_testTable;
				bl.Columns.AddRange(new string[] { "one", "two", "three", "four", "five" });
				bl.NumberOfLinesToSkip = 1;
				bl.Expressions.Add("five = UNHEX(five)");
				bl.Local = true;
				int rowCount = await bl.LoadAsync();
				Assert.Equal(20, rowCount);
			}
		}

		[BulkLoaderLocalTsvFileFact]
		public async Task BulkLoadLocalTsvFileDoubleEscapedTerminators()
		{
			using (MySqlConnection connection = new MySqlConnection(AppConfig.ConnectionString))
			{
				MySqlBulkLoader bl = new MySqlBulkLoader(connection);
				bl.FileName = AppConfig.MySqlBulkLoaderLocalTsvFile;
				bl.TableName = m_testTable;
				bl.Columns.AddRange(new string[] { "one", "two", "three", "four", "five" });
				bl.NumberOfLinesToSkip = 1;
				bl.Expressions.Add("five = UNHEX(five)");
				bl.LineTerminator = "\\n";
				bl.FieldTerminator = "\\t";
				bl.Local = true;
				int rowCount = await bl.LoadAsync();
				Assert.Equal(20, rowCount);
			}
		}

		[BulkLoaderCsvFileFact]
		public async Task BulkLoadCsvFile()
		{
			using (MySqlConnection connection = new MySqlConnection(AppConfig.ConnectionString))
			{
				MySqlBulkLoader bl = new MySqlBulkLoader(connection);
				bl.FileName = AppConfig.MySqlBulkLoaderCsvFile;
				bl.TableName = m_testTable;
				bl.Columns.AddRange(new string[] { "one", "two", "three", "four", "five" });
				bl.NumberOfLinesToSkip = 1;
				bl.FieldTerminator = ",";
				bl.FieldQuotationCharacter = '"';
				bl.FieldQuotationOptional = true;
				bl.Expressions.Add("five = UNHEX(five)");
				bl.Local = false;
				int rowCount = await bl.LoadAsync();
				Assert.Equal(20, rowCount);
			}
		}

		[BulkLoaderLocalCsvFileFact]
		public async Task BulkLoadLocalCsvFile()
		{
			MySqlBulkLoader bl = new MySqlBulkLoader(m_database.Connection);
			bl.FileName = AppConfig.MySqlBulkLoaderLocalCsvFile;
			bl.TableName = m_testTable;
			bl.CharacterSet = "UTF8";
			bl.Columns.AddRange(new string[] { "one", "two", "three", "four", "five" });
			bl.NumberOfLinesToSkip = 1;
			bl.FieldTerminator = ",";
			bl.FieldQuotationCharacter = '"';
			bl.FieldQuotationOptional = true;
			bl.Expressions.Add("five = UNHEX(five)");
			bl.Local = true;
			int rowCount = await bl.LoadAsync();
			Assert.Equal(20, rowCount);
		}

		[Fact]
		public async Task BulkLoadCsvFileNotFound()
		{
			var secureFilePath = m_database.Connection.Query<string>(@"select @@global.secure_file_priv;").FirstOrDefault() ?? "";

			MySqlBulkLoader bl = new MySqlBulkLoader(m_database.Connection);
			bl.FileName = Path.Combine(secureFilePath, AppConfig.MySqlBulkLoaderCsvFile + "-junk");
			bl.TableName = m_testTable;
			bl.CharacterSet = "UTF8";
			bl.Columns.AddRange(new string[] { "one", "two", "three", "four", "five" });
			bl.NumberOfLinesToSkip = 1;
			bl.FieldTerminator = ",";
			bl.FieldQuotationCharacter = '"';
			bl.FieldQuotationOptional = true;
			bl.Expressions.Add("five = UNHEX(five)");
			bl.Local = false;
			try
			{
				int rowCount = await bl.LoadAsync();
			}
			catch (MySqlException mySqlException)
			{
				if (mySqlException.InnerException != null)
				{
					Assert.IsType(typeof(System.IO.FileNotFoundException), mySqlException.InnerException);
				}
				else
				{
					Assert.Contains("Errcode: 2 - No such file or directory", mySqlException.Message);
				}
			}
		}

		[Fact]
		public async Task BulkLoadLocalCsvFileNotFound()
		{
			MySqlBulkLoader bl = new MySqlBulkLoader(m_database.Connection);
			bl.Timeout = 3; //Set a short timeout for this test because the file not found exception takes a long time otherwise, the timeout does not change the result
			bl.FileName = AppConfig.MySqlBulkLoaderLocalCsvFile + "-junk";
			bl.TableName = m_testTable;
			bl.CharacterSet = "UTF8";
			bl.Columns.AddRange(new string[] { "one", "two", "three", "four", "five" });
			bl.NumberOfLinesToSkip = 1;
			bl.FieldTerminator = ",";
			bl.FieldQuotationCharacter = '"';
			bl.FieldQuotationOptional = true;
			bl.Expressions.Add("five = UNHEX(five)");
			bl.Local = true;
			try
			{
				int rowCount = await bl.LoadAsync();
			}
			catch (MySqlException mySqlException)
			{
				while (mySqlException.InnerException != null)
				{
					if (mySqlException.InnerException.GetType() == typeof(MySqlException))
					{
						mySqlException = (MySqlException)mySqlException.InnerException;
					}
					else
					{
						Assert.IsType(typeof(System.IO.FileNotFoundException), mySqlException.InnerException);
						break;
					}
				}
				if (mySqlException.InnerException == null)
				{
					Assert.IsType(typeof(System.IO.FileNotFoundException), mySqlException);
				}
			}
			catch (Exception exception)
			{
				//We know that the exception is not a MySqlException, just use the assertion to fail the test
				Assert.IsType(typeof(MySqlException), exception);
			};
		}

		[Fact]
		public async Task BulkLoadMissingFileName()
		{
			MySqlBulkLoader bl = new MySqlBulkLoader(m_database.Connection);
			bl.TableName = m_testTable;
			bl.Columns.AddRange(new string[] { "one", "two", "three", "four", "five" });
			bl.NumberOfLinesToSkip = 1;
			bl.FieldTerminator = ",";
			bl.FieldQuotationCharacter = '"';
			bl.FieldQuotationOptional = true;
			bl.Expressions.Add("five = UNHEX(five)");
			bl.Local = false;
#if BASELINE
			await Assert.ThrowsAsync<System.NullReferenceException>(async () =>
			{
				int rowCount = await bl.LoadAsync();
			});
#else
			await Assert.ThrowsAsync<System.InvalidOperationException>(async () =>
			{
				int rowCount = await bl.LoadAsync();
			});
#endif
		}

		[BulkLoaderLocalCsvFileFact]
		public async Task BulkLoadMissingTableName()
		{
			MySqlBulkLoader bl = new MySqlBulkLoader(m_database.Connection);
			bl.FileName = AppConfig.MySqlBulkLoaderLocalCsvFile;
			bl.Columns.AddRange(new string[] { "one", "two", "three", "four", "five" });
			bl.NumberOfLinesToSkip = 1;
			bl.FieldTerminator = ",";
			bl.FieldQuotationCharacter = '"';
			bl.FieldQuotationOptional = true;
			bl.Expressions.Add("five = UNHEX(five)");
			bl.Local = false;
#if BASELINE
			await Assert.ThrowsAsync<MySqlException>(async () =>
			{
				int rowCount = await bl.LoadAsync();
			});
#else
			await Assert.ThrowsAsync<System.InvalidOperationException>(async () =>
			{
				int rowCount = await bl.LoadAsync();
			});
#endif
		}

#if BASELINE
		[Fact(Skip = "InfileStream not implemented")]
		public void BulkLoadFileStreamInvalidOperation() {}
#else
		[BulkLoaderLocalCsvFileFact]
		public async Task BulkLoadFileStreamInvalidOperation()
		{
			using (MySqlConnection connection = new MySqlConnection(AppConfig.ConnectionString))
			{
				MySqlBulkLoader bl = new MySqlBulkLoader(connection);
				using (var fileStream = new FileStream(AppConfig.MySqlBulkLoaderLocalCsvFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
				{
					bl.InfileStream = fileStream;
					bl.TableName = m_testTable;
					bl.Columns.AddRange(new string[] { "one", "two", "three", "four", "five" });
					bl.NumberOfLinesToSkip = 1;
					bl.FieldTerminator = ",";
					bl.FieldQuotationCharacter = '"';
					bl.FieldQuotationOptional = true;
					bl.Expressions.Add("five = UNHEX(five)");
					bl.Local = false;
					await Assert.ThrowsAsync<System.InvalidOperationException>(async () =>
					{
						int rowCount = await bl.LoadAsync();
					});
				}
			}
		}
#endif

#if BASELINE
		[Fact(Skip = "InfileStream not implemented")]
		public void BulkLoadLocalFileStream() {}
#else
		[BulkLoaderLocalCsvFileFact]
		public async Task BulkLoadLocalFileStream()
		{
			MySqlBulkLoader bl = new MySqlBulkLoader(m_database.Connection);
			using (var fileStream = new FileStream(AppConfig.MySqlBulkLoaderLocalCsvFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
			{
				bl.InfileStream = fileStream;
				bl.TableName = m_testTable;
				bl.Columns.AddRange(new string[] { "one", "two", "three", "four", "five" });
				bl.NumberOfLinesToSkip = 1;
				bl.FieldTerminator = ",";
				bl.FieldQuotationCharacter = '"';
				bl.FieldQuotationOptional = true;
				bl.Expressions.Add("five = UNHEX(five)");
				bl.Local = true;
				int rowCount = await bl.LoadAsync();
				Assert.Equal(20, rowCount);
			}
		}
#endif

#if BASELINE
		[Fact(Skip = "InfileStream not implemented")]
		public void BulkLoadMemoryStreamInvalidOperation() {}
#else
		[Fact]
		public async Task BulkLoadMemoryStreamInvalidOperation()
		{
			MySqlBulkLoader bl = new MySqlBulkLoader(m_database.Connection);
			using (var memoryStream = new MemoryStream(m_memoryStreamBytes, false))
			{
				bl.InfileStream = memoryStream;
				bl.TableName = m_testTable;
				bl.Columns.AddRange(new string[] { "one", "two", "three" });
				bl.NumberOfLinesToSkip = 0;
				bl.FieldTerminator = ",";
				bl.FieldQuotationCharacter = '"';
				bl.FieldQuotationOptional = true;
				bl.Local = false;
				await Assert.ThrowsAsync<System.InvalidOperationException>(async () =>
				{
					int rowCount = await bl.LoadAsync();
				});
			}
		}
#endif

#if BASELINE
		[Fact(Skip = "InfileStream not implemented")]
		public void BulkLoadLocalMemoryStream() {}
#else
		[Fact]
		public async Task BulkLoadLocalMemoryStream()
		{
			MySqlBulkLoader bl = new MySqlBulkLoader(m_database.Connection);
			using (var memoryStream = new MemoryStream(m_memoryStreamBytes, false))
			{
				bl.InfileStream = memoryStream;
				bl.TableName = m_testTable;
				bl.Columns.AddRange(new string[] { "one", "two", "three" });
				bl.NumberOfLinesToSkip = 0;
				bl.FieldTerminator = ",";
				bl.FieldQuotationCharacter = '"';
				bl.FieldQuotationOptional = true;
				bl.Local = true;
				int rowCount = await bl.LoadAsync();
				Assert.Equal(5, rowCount);
			}
		}
#endif

		readonly DatabaseFixture m_database;
		readonly string m_testTable;
		readonly byte[] m_memoryStreamBytes;
	}
}