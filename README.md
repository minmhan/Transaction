# Upload Transaction
Upload CSV and XML transaction files.

# Projects
- Trasaction.API
- Transaction.Entity
- Transaction.Web

First Run, "Transaction" Database will be auto created in mssql local DB, if not created yet.  
If you have other sql server, you can change connection string in Transaction.API\appsettings.Deveopment.json(appsettings.json) file.

# Assumption
- DateTime in Uploaded CSV and XML files are in UTC.

# TODO
- Pagination (API / UI)
- XML Documentation
- Prettify UI
