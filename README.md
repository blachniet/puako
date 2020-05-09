# Puako

Download the latest versions of applications not hosted in package repositories.

## Contributing

```sh
# Build
cd src/Puako
dotnet build

# Publish
cd src/Puako
for runtime in "linux-x64" "rhel-x64" "osx-x64" "win-x64"; do
    dotnet publish -c Release -r "${runtime}" -p:PublishSingleFile=true
done
```
