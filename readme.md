## The Bucket Project

 *... a something somewhere between DOCKER RUN and CNAB.*

![logo](https://github.com/martinstanek/bucket/blob/develop/misc/logo.png?raw=true)

Utilise your docker-compose files to create transferable Docker bundles.

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

```
% ./bucket --bundle ./bundle.json
% ./bucket --install ./gfms-machinemonitor.dap.tar.gz --output machinemonitor
```

### Sample Manifest

```json
{
  "Info": {
    "Name": "TestBundle",
    "Description": "This a first *.dap definition ever ...",
    "Version": "0.1.0"
  },
  "Configuration": {
    "FetchImages": true
  },
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
    "./test"
  ]
}
```


