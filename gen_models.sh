
#!/bin/sh

V1="https://willcas.github.io/gpig/reference/GPIG.v1.json"

docker run --rm -v "${PWD}:/local" swaggerapi/swagger-codegen-cli-v3 generate \
    -i "$V1" \
    -l csharp-dotnet2 \
    -o /local/tmp
