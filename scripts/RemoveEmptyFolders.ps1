gci . -Recurse | foreach {
    if($_.Length -eq 0){
       Write-Output "Removing Empty File $($_.FullName)"
       $_.FullName | Remove-Item -Force
    }
    if( $_.psiscontainer -eq $true){
       if((gci $_.FullName) -eq $null){
          Write-Output "Removing Empty folder $($_.FullName)"
          $_.FullName | Remove-Item -Force
       }
 }
}