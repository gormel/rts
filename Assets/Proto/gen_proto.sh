rm -rf ./dst
mkdir dst
ls src/*.proto | xargs -I {} protoc -Isrc {} --csharp_out=./dst --grpc_out=./dst --plugin=protoc-gen-grpc=/usr/bin/grpc_csharp_plugin
