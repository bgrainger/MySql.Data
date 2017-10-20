---
lastmod: 2016-10-16
date: 2016-10-16
menu:
  main:
    parent: tutorials
title: Migrating from Connector/NET
weight: 20
---

Migrating from Connector/NET
============================

### Connection String Differences

MySqlConnector has some different default connection string options:

<table class="table table-striped table-hover">
  <thead>
    <th style="width:20%">Option</th>
    <th style="width:20%">MySqlConnector</th>
    <th style="width:20%">Oracle's Connector/NET</th>
    <th style="width:40%">Notes</th>
  </thead>
  <tr>
    <td><code>AllowPublicKeyRetrieval</code></td>
    <td>Default is <code>false</code></td>
    <td>(not configurable)</td>
    <td>When using <code>sha256_password</code> authentication, this option allows the RSA public key to be retrieved from the server
    (when not using a secure connection). It's <code>false</code> by default to avoid disclosing the password to a malicious proxy.</td>
  </tr>
  <tr>
    <td><code>ConnectionReset</code></td>
    <td>Default is <code>true</code></td>
    <td>Default is <code>false</code></td>
    <td>MySqlConnector takes an extra command to reset pooled connections by default so that the connection is always in a known state</td>
  </tr>
  <tr>
    <td><code>ServerRSAPublicKeyFile</code></td>
    <td>(no default)</td>
    <td>(not configurable)</td>
    <td>Specify the server's RSA public key to allow <code>sha256_password</code> authentication over an insecure connection.</td>
  </tr>
  <tr>
    <td><code>UseAffectedRows</code></td>
    <td>Default is <code>true</code></td>
    <td>Default is <code>false</code></td>
    <td>This also affects the behavior of the <code>ROW_COUNT</code> function.  <code>UseAffectedRows=true</code> is the default in most other languages (C++, PHP, others)</td>
  </tr>
</table>

Some command line options that are supported in Connector/NET are not supported in MySqlConnector.  For a full list of options that are
supported in MySqlConnector, see the [Connection Options](connection-options).

### TransactionScope

MySqlConnector adds full distributed transaction support (for client code using [`TransactionScope`](https://msdn.microsoft.com/en-us/library/system.transactions.transactionscope.aspx)),
while Connector/NET uses regular database transactions. As a result, code that uses `TransactionScope`
may execute differently with MySqlConnector. To get Connector/NET-compatible behavior, remove
`TransactionScope` and use `BeginTransaction`/`Commit` directly.

### MySqlConnection

Connector/NET allows a `MySqlConnection` object to be reused after it has been disposed. MySqlConnector requires a new `MySqlConnection`
object to be created. See [#331](https://github.com/mysql-net/MySqlConnector/issues/331) for more details.

### MySqlCommand

Connector/NET allows a command to be executed even when `MySqlCommand.Transaction` references a commited, rolled back, or
disposed `MySqlTransaction`. MySqlConnector will throw an `InvalidOperationException` if the `MySqlCommand.Transaction`
property doesn't reference the active transaction. See [#333](https://github.com/mysql-net/MySqlConnector/issues/333) for more details.

### Bugs present in Connector/NET that are fixed in MySqlConnector

* [#37283](https://bugs.mysql.com/bug.php?id=37283), [#70587](https://bugs.mysql.com/bug.php?id=70587): Distributed transactions are not supported
* [#50773](https://bugs.mysql.com/bug.php?id=50773): Can't use multiple connections within one TransactionScope
* [#61477](https://bugs.mysql.com/bug.php?id=61477): `ColumnOrdinal` in schema table is 1-based
* [#66476](https://bugs.mysql.com/bug.php?id=66476): Connection pool uses queue instead of stack
* [#70111](https://bugs.mysql.com/bug.php?id=70111): `Async` methods execute synchronously
* [#70686](https://bugs.mysql.com/bug.php?id=70686): `TIME(3)` and `TIME(6)` fields serialize milliseconds incorrectly
* [#72494](https://bugs.mysql.com/bug.php?id=72494), [#83330](https://bugs.mysql.com/bug.php?id=83330): EndOfStreamException inserting large blob with UseCompression=True
* [#73610](https://bugs.mysql.com/bug.php?id=73610): Invalid password exception has wrong number
* [#73788](https://bugs.mysql.com/bug.php?id=73788): Can't use `DateTimeOffset`
* [#75917](https://bugs.mysql.com/bug.php?id=75917), [#76597](https://bugs.mysql.com/bug.php?id=76597), [#77691](https://bugs.mysql.com/bug.php?id=77691), [#78650](https://bugs.mysql.com/bug.php?id=78650), [#78919](https://bugs.mysql.com/bug.php?id=78919), [#80921](https://bugs.mysql.com/bug.php?id=80921), [#82136](https://bugs.mysql.com/bug.php?id=82136): "Reading from the stream has failed" when connecting to a server
* [#77421](https://bugs.mysql.com/bug.php?id=77421): Connection is not reset when pulled from the connection pool
* [#78426](https://bugs.mysql.com/bug.php?id=78426): Unknown database exception has wrong number
* [#78760](https://bugs.mysql.com/bug.php?id=78760): Error when using tabs and newlines in SQL statements
* [#78917](https://bugs.mysql.com/bug.php?id=78917), [#79196](https://bugs.mysql.com/bug.php?id=79196), [#82292](https://bugs.mysql.com/bug.php?id=82292): `TINYINT(1)` values start being returned as `sbyte` after `NULL`
* [#80030](https://bugs.mysql.com/bug.php?id=80030): Slow to connect with pooling disabled
* [#81650](https://bugs.mysql.com/bug.php?id=81650): `Server` connection string option may now contain multiple, comma separated hosts that will be tried in order until a connection succeeds
* [#83229](https://bugs.mysql.com/bug.php?id=83329): "Unknown command" exception inserting large blob with UseCompression=True
* [#84220](https://bugs.mysql.com/bug.php?id=84220): Cannot call a stored procedure with `.` in its name
* [#84701](https://bugs.mysql.com/bug.php?id=84701): Can't create a paramter using a 64-bit enum with a value greater than int.MaxValue
* [#85185](https://bugs.mysql.com/bug.php?id=85185): `ConnectionReset=True` does not preserve connection charset
* [#86263](https://bugs.mysql.com/bug.php?id=86263): Transaction isolation level affects all transactions in session
* [#87307](https://bugs.mysql.com/bug.php?id=87307): NextResult hangs instead of timing out
* [#87316](https://bugs.mysql.com/bug.php?id=87316): MySqlCommand.CommandTimeout can be set to a negative value
* [#87868](https://bugs.mysql.com/bug.php?id=87868): `ColumnSize` in schema table is incorrect for `CHAR(36)` and `BLOB` columns
* [#87876](https://bugs.mysql.com/bug.php?id=87876): `IsLong` is schema table is incorrect for `LONGTEXT` and `LONGBLOB` columns
* [#88058](https://bugs.mysql.com/bug.php?id=88058): `decimal(n, 0)` has wrong `NumericPrecision`
* [#88124](https://bugs.mysql.com/bug.php?id=88124): CommandTimeout isn't reset when calling Read/NextResult
