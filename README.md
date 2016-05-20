# FbGrant

Grant or Revoke user permissons to all object in a Firebird database

## usage
    FbGrant.exe grant|revoke -d user:password@server:database -u user [-r role] [-a] [-x]

Options
```
  grant      Grant permissions
  revoke     Revoke permissions
  help       Display more information on a specific command.
  version    Display version information.
```
Arguments:
```
  -d, --database           Required. Database
  -u, --user               Required. db user
  -r, --role               db role
  -a, --WithAdminOption    grant WITH ADMIN OPTION.
  -x, --execute            Execute statements in the database
```
