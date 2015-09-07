# noisebox

Welcome to noisebox!

Noisebox is a command line tool that helps you generate random noise, written in C&sharp; by [Starbeamrainbowlabs](https://starbeamrainbowlabs.com/).

The following noise types are currently supported:

 - [White](https://en.wikipedia.org/wiki/White_noise) by the random number generator and Starbeamrainbowlabs
 - [Perlin](https://en.wikipedia.org/wiki/Perlin_noise) by Ken Perlin, implemented by Tom Nuydens, converted to C&sharp; by Mattias Fagerlund.
 - [Simplex](https://en.wikipedia.org/wiki/Simplex_noise) by Ken Perlin, implemented by Heikki Törmälä.
 - [OpenSimplex](https://en.wikipedia.org/wiki/OpenSimplex_noise) by KdotJPG, ported from Java and refactored by digitalshadow to improve performance.
 - [Worley](https://en.wikipedia.org/wiki/Worley_noise) by Steven Worley, implemented by Starbeamrainbowlabs

Additional licensing and credit information can be found at the bottom of this file.

## Download
Binaries are available for Windows (compiled against .NET 4.5.1). In theory these binaries should work with Mono on other platforms.

Name    | Description   | Link
--------|---------------|-------------
Debug   | The very latest development build. May be unstable. | [noisebox.exe](https://raw.githubusercontent.com/sbrl/noisebox/master/noisebox/bin/Debug/noisebox.exe)
Release | The latest stable version. Might not contain all the latest features. | [noisebox.exe](https://raw.githubusercontent.com/sbrl/noisebox/master/noisebox/bin/Release/noisebox.exe)

### Compiling from source
To compile from source, simply clone this repository, open the Visual Studio 2013 solution, and hit CTRL + SHIFT + B (or whatever keyboard shortcut you assigned to building a solution).

## Command Line Arguments
A complete list can be obtained by typing `noisebox.exe --help`.

Option			| Default Value	| Example					|  Meaning
----------------|---------------|---------------------------|---------------------------
`--size`		| `1024x1024`	| `--size 640x480`		| Sets the size of the resulting image(s).
`--type`		| `White`		| `--type worley`			| Sets the type of noise to generate. For a full list, see above.
`--distfunc`	| `Eucliedean`	| `--distfunc Manhattan`	| Set the distance function when working with worley noise. Currently supports `Euclidean`, `Manhatttan`, and `Chebyshev`, but only `Euclidean` functions correctly.
`--colour`		| `#ffffff`	| `--colour #ff3300`		| The colour of the resulting image. Takes in a hex code. Currently has no efffect on worley noise due to optimisations that had to be made.
`--frames`		| `1`			| `--frames 15`			| Causes noisebox to render a number of frames in sequence. Replaces the hash sign in the filename with the frame number. Adding more hash signs causes the frame number to be padded with zeros. Useful if you are going to stitch them togethr with `ffmpeg` or `avconv` later.
`--offset`		| `0`			| `--offset 16`			| Allows you to specify the offset to apply to the frame number. Example uses include picking up rendering where you left off.
`--threads`	| `# of cpus - 1` | `--threads 4 `		| Sets the number of threads to use when rendering multiple frames. Each thread will work on one frame at a time. If not specified, the number of cpus you have minus one will be used as the thread count. At least one thread will always be used.
`--verbose`	| _n/a_			| `--verbose`				| Causes noisebox to become rather chatty, and log a lot of random stuff to the console. The effect this argument will have will vary over time, as I work on different areas of noisebox.

## Credits
The noise functions and the code used to generate them have come from many different places. I'll try my best to keep this section up to date. If you notice any mistakes, [please open an issue](https://github.com/sbrl/noisebox/issues/new).

If you are the owner (or implementor) of one of these noise algorithms and you want it to be removed, please [open an issue](https://github.com/sbrl/noisebox/issues/new) explaining why and I will deal with it as soon as I can manage.

Algorithm	| Inventor		| License	| Implementor(s)			| Link to source code
------------|---------------|-----------|---------------------------|----------------------------
[White](https://en.wikipedia.org/wiki/White_noise)		| Random number generator | Public Domain | [Starbeamrainbowlabs](https://github.com/sbrl/)	| _n/a_
[Perlin](https://en.wikipedia.org/wiki/Perlin_noise)		| [Ken Perlin](https://mrl.nyu.edu/~perlin/) | ?		| Tom Nuydens, ported from Delphi by [Mattias Fagerlund](https://lotsacode.wordpress.com/)	 | https://lotsacode.wordpress.com/2010/02/24/perlin-noise-in-c/
[Simplex](https://en.wikipedia.org/wiki/Simplex_noise)		| [Ken Perlin](https://mrl.nyu.edu/~perlin/) | ? (implementation under [Unlicense](http://unlicense.org/)		| Heikki Törmälä, loosly based on the [SimplexNoise1324](http://staffwww.itn.liu.se/~stegu/aqsis/aqsis-newnoise/simplexnoise1234.h) implementation by Stefan Gustavson | http://web.archive.org/web/20150618221040/https://code.google.com/p/simplexnoise/source/browse/trunk/SimplexNoise/Noise.cs
[OpenSimplex](http://uniblock.tumblr.com/post/97868843242/noise) | [KdotJPG](http://uniblock.tumblr.com/)	| [Unlicense](http://unlicense.org/)	| [KdotJPG](http://uniblock.tumblr.com/), ported and refactored to improve performance by [digitalshadow](https://github.com/digitalshadow) | https://gist.github.com/digitalshadow/134a3a02b67cecd72181/
[Worley](https://en.wikipedia.org/wiki/Worley_noise)		| Steven Worley	| ? (implementation under the same license as this repo)		| [Starbeamrainbowlabs](https://github.com/sbrl/) | https://raw.githubusercontent.com/sbrl/noisebox/master/noisebox/Worley.cs

## License
noisebox is licensed under the [Mozilla Public License 2.0](https://tldrlegal.com/license/mozilla-public-license-2.0-(mpl-2)). The full text can be [found here](https://raw.githubusercontent..com/sbrl/noisebox/master/LICENSE.txt).

The noise algorithms (and their implementations are not covered by this license. Please see the section above for more information.
