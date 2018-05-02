A quick example of how to pull AD group membership using C#

Using ObjectGUIDs to locate group in AD. This way if an admin changes the DN values of a group by moving it or renaming an OU in the path, the code won't need to be updated. 

To quickly find the ObjectGUID value for your group run this PowerShell command:

Get-ADGroup -Identity <Name of Your Group>