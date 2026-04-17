# flowx (fork)

sharex command palette for flow launcher

## features

- run sharex commands with the prefix `shx`
- presets toggle, ofc you can pick your own commands as well :)
- custom sharex installation path

## build

```powershell
.\build.ps1
```

expected output: `release/flowx.zip`, `publish/`; `package/flowx/`

either copy the generated files from `publish/`, or extract the archive `release/flowx.zip` into your plugins directory (e.g. `Plugins/flowx/`)

intermediates can be found in `.artifacts/`
