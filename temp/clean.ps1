gci . -Recurse | foreach {
   if( $_.psiscontainer -eq $true){
      if((gci $_.FullName) -eq $null){
         Write-Output "Removing Empty folder $($_.FullName)"
         $_.FullName | Remove-Item -Force
      }
}
}