

Get-Process | Where-Object { $_.name -eq ‘AlephNote.App’ } | Stop-Process

