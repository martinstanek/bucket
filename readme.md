## The Bucket Project

 *... a simple docker compose wrapper/cli tool/library for creating transferable single file bundles*

![logo](https://github.com/martinstanek/bucket/blob/develop/misc/logo.png?raw=true)

[![Build status](https://awitec.visualstudio.com/Awitec/_apis/build/status/bucket)](https://awitec.visualstudio.com/Awitec/_build/latest?definitionId=51)

### CLI

```
Arguments:
    -h, --help : Show this help
    -b, --bundle : Bundle given manifest
        Either the manifest path is provided, or a valid manifest is searched in the current directory
    -i, --install : Install given bundle
        The path to the bundle is required
    -r, --remove : Uninstall and remove given bundle
        The path to the bundle folder is required
    -s, --start : Start given bundle
        The path to the bundle folder is required
    -t, --stop : Stop given bundle
        The path to the bundle folder is required
    -o, --output : Path to the output file or directory
    -w, --workdir : Path to the working directory during bundling
        If no directory provided, the current executable directory will be used
    -v, --verbose : Turn on internal logs
```
### How to use

Imagine we have a folder with following structure representing our simple demo docker based application:

```
/my-app
    -- /backend
        -- docker-compose.yml        
    -- /proxy
        -- /config
            -- api-gateway.comf
        -- docker-compose.yml
 -- manifest.json 
```
This app is just an echo server with a nginx proxy in front of it.\
Our compose files could look like this:

backend: [docker-compose.yml](https://github.com/martinstanek/bucket/blob/develop/tst/Bucket.Tests/Bundle/backend/docker-compose.yml)\
proxy: [docker-compose.yml](https://github.com/martinstanek/bucket/blob/develop/tst/Bucket.Tests/Bundle/proxy/docker-compose.yml)\

And the manifest.json:

```json
{
  "Info": {
    "Name": "bucket-test-bundle",
    "Description": "Simple .DAP definition example",
    "Version": "0.1.0"
  },
  "Configuration": {
    "FetchImages": true
  },
  "Images": [
    { "Alias": "backend", "FullName": "docker.io/hashicorp/http-echo:1.0" },
    { "Alias": "proxy", "FullName": "docker.io/nginx:1.23.4" }
  ],
  "Stacks": [
    "backend",
    "proxy"
  ]
}
```

Let's either move our bucket binary next to the manifest.json file, or use absolute file paths.

Then by hitting: 

```
% ./bucket --bundle ./bundle.json
```

We'll generate **bucket-test-bundle.dap.tar.gz**\
Followed on the target machine:

```
% ./bucket --install  ./bucket-test-bundle.dap.tar.gz --output ./my-app-folder
```

Should result in two happy compose stacks up&running.

Happy Bundling,\
Martin