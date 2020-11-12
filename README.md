# Upload Transaction
Upload CSV and XML transaction files.

# Requirement
- Dotnet Core 3.1
- Visual 2019 For Development 

# Projects
- Trasaction.API
- Transaction.Entity
- Transaction.Web

# Development
- Run Multi startup projects from Solution properties with - Transaction.API, Transaction.Web 
- First Run, "Transaction" Database will be auto created in mssql local DB, if not created yet.  
- If you have other sql server, you can change connection string in Transaction.API\appsettings.Deveopment.json(appsettings.json) file.

# Assumption
- DateTime in Uploaded CSV and XML files are in UTC. (Sample CSV & XML files in Transaction.API\Data folder)

# TODO
- Pagination (API / UI)
- XML Documentation
- Prettify UI
