if (Test-Path -Path .\dst) {
    rm -Recurse -Force dst\*
}
New-Item -Path . -Name "dst" -ItemType "directory"
ls src/*.proto | foreach { .\tools\protoc.exe -I .\src $_.Name --csharp_out=.\dst --plugin=protoc-gen-grpc=.\tools\grpc_csharp_plugin.exe --grpc_out=.\dst }