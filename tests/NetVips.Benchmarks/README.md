# NetVips benchmarks

The goal of this project is to demonstrate the performance of the NetVips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/libvips/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 23/05/19 with libvips 8.8.0-rc3, Magick.NET 7.13.1, ImageSharp 1.0.0-beta0006, SkiaSharp 1.68.0 and System.Drawing.Common 4.5.1.

``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.17763.529 (1809/October2018Update/Redstone5)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET Core SDK=2.2.300
  [Host]     : .NET Core 2.2.5 (CoreCLR 4.6.27617.05, CoreFX 4.6.27618.01), 64bit RyuJIT
  Job-PEGGXL : .NET Core 2.2.5 (CoreCLR 4.6.27617.05, CoreFX 4.6.27618.01), 64bit RyuJIT

Toolchain=.NET Core 2.2.0  

```
|         Method | input | output |      Mean |     Error |    StdDev | Ratio | RatioSD |
|--------------- |------ |------- |----------:|----------:|----------:|------:|--------:|
|        **NetVips** | **t.jpg** | **t2.jpg** |  **29.05 ms** | **0.2566 ms** | **0.2274 ms** |  **1.00** |    **0.00** |
|     Magick.NET | t.jpg | t2.jpg | 305.85 ms | 1.0218 ms | 0.8533 ms | 10.53 |    0.08 |
|     ImageSharp¹ | t.jpg | t2.jpg | 176.37 ms | 0.5108 ms | 0.4778 ms |  6.07 |    0.05 |
|      SkiaSharp¹ | t.jpg | t2.jpg | 235.74 ms | 0.1413 ms | 0.1322 ms |  8.12 |    0.07 |
| System.Drawing² | t.jpg | t2.jpg | 273.12 ms | 0.5481 ms | 0.4577 ms |  9.40 |    0.08 |
|                |       |        |           |           |           |       |         |
|        **NetVips** | **t.tif** | **t2.tif** |  **19.26 ms** | **0.2896 ms** | **0.2709 ms** |  **1.00** |    **0.00** |
|     Magick.NET | t.tif | t2.tif | 287.90 ms | 0.5872 ms | 0.5206 ms | 14.98 |    0.19 |
| System.Drawing | t.tif | t2.tif | 282.01 ms | 1.1995 ms | 1.1220 ms | 14.65 |    0.25 |

¹ ImageSharp and SkiaSharp does not have TIFF support, so I only tested with JPEG files.  
² System.Drawing does not have a sharpening or convolution operation, so I skipped that part of the benchmark.

## Performance test design

The project contains a `Benchmark.cs` file with specific scripts 
using various libraries available on .NET.

Each script is coded to execute the same scenario (see Scenario section).

See "Do it yourself" section for how to run benchmark scenario.

## Scenario

Test scenario was taken from [Speed and Memory
use](https://github.com/libvips/libvips/wiki/Speed-and-memory-use)
page from libvips [Home
page](https://libvips.github.io/libvips/).

## Do it yourself

```bash
git clone https://github.com/kleisauke/net-vips

cd net-vips/tests/NetVips.Benchmarks

dotnet run -c Release
```