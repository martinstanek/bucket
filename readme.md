## The Bucket Project

 *... a something somewhere between DOCKER RUN and CNAB.*

![logo](https://github.com/martinstanek/bucket/blob/develop/misc/logo.png?raw=true)

Utilise your docker-compose files to create transferable Docker bundles.

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

