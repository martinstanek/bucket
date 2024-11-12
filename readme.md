## The Bucket Project

 *... a something somewhere between DOCKER RUN and CNAB.*

![logo](https://github.com/martinstanek/bucket/blob/develop/misc/logo.png?raw=true)

Utilise your docker-compose files to create transferable Docker bundles.

### CLI

```
Arguments:
    -h, --help : Show this help
    -b, --bundle : Bundle given manifest
        Either the manifest path is provided, or a valid manifest is searched in the current directory
    -i, --install : Install given bundle
        The path to the bundle is required
    -u, --uninstall : Uninstall given bundle
        The path to the bundle folder is required
    -s, --start : Start given bundle
        The path to the bundle folder is required
    -t, --stop : Stop given bundle
        The path to the bundle folder is required
    -o, --output : Path to the output file or directory
```

### Sample Manifest

```
{
  "Info": {
    "Name": "TestBundle",
    "Description": "This a first *.dap definition ever ...",
    "Version": "0.1.0"
  },
  "Configuration": {
    "FetchImages": true
  },
  "Parameters": [
    {
      "Name": "test_parameter",
      "Description": "Please provide the test parameter ...."
    }
  ],
  "Registries": [
    {
      "Name": "test1",
      "Server": "testserver1.io",
      "User": "user",
      "Password": "password"
    },
    {
      "Name": "test2",
      "Server": "testserver2.io",
      "User": "user",
      "Password": "password"
    }
  ],
  "Images": [
    {
      "Alias": "image1",
      "FullName": "registry/image1:tag"
    },
    {
      "Alias": "image2",
      "FullName": "registry/image2:tag"
    }
  ],
  "Stacks": [
    "./test/docker-compose.yml"
  ]
}
```


