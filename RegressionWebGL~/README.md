# Jest Unity SDK Regression WebGL Project

This Unity project builds a WebGL regression fixture against the SDK package in this repository.

Run from the repository root:

```bash
Unity.exe -batchmode -nographics -quit -projectPath RegressionWebGL~ -buildTarget WebGL -executeMethod com.jest.sdk.regression.Editor.JestSdkRegressionBuild.BuildWebGL -outputPath <output-dir>
```

The project imports the SDK with `file:../../`, so the build uses the current checkout rather than an installed sample copy.
