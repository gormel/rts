rm -Recurse -Force dst\*
ls src/*.proto | foreach { .\tools\protoc.exe -I .\src $_.Name --csharp_out=.\dst --plugin=protoc-gen-grpc=.\tools\grpc_csharp_plugin.exe --grpc_out=.\dst }