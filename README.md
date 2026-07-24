#

`dotnet build -t:Coverage`

#

`dotnet msbuild targets/CreateArchive.targets -t:Archive`

# to execute for a folder of .log files  
`for %f in (C:\folder_path\*) do C:\U...e\GertToUTW\bin\Release\net10.0\GertToUTW.exe "%f" out_dir`
