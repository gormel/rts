rm -rf ./dst/*
mkdir dst
ls src/*.proto | xargs -I {} wine ./tools/protoc.exe -Isrc {} --csharp_out=./dst --plugin=protoc-gen-grpc=./tools/grpc_csharp_plugin.exe --grpc_out=./dst
