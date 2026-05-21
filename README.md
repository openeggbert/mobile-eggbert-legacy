# Mobile Eggbert Legacy

Mobile Eggbert is a modified version of **Speedy Blupi**, originally developed for Windows Phone and released in 2013.

This repository contains a legacy codebase that went through several transformation steps:

1. The original Windows Phone/XNA version was decompiled with **ILSpy** into C# source code.
2. The recovered C# project was migrated from **XNA 4.0** to **MonoGame**.
3. Several related Git repositories were merged into this repository using **git subtree**, so the project history and related code can be kept together in one place.

## Purpose of this repository

The goal of this repository is preservation, experimentation, and further development of the Mobile Eggbert / Speedy Blupi codebase.

This repository may be useful for:

- studying the original Windows Phone game structure,
- preserving legacy XNA/MonoGame code,
- experimenting with ports to newer platforms,
- comparing different stages of the migration,
- keeping related repositories together in one historical repository.

## Project status

This is a legacy project.

The code may require additional fixes, older dependencies, or platform-specific adjustments before it builds or runs correctly on a modern system.

The project should be treated as a historical and experimental codebase rather than a clean modern rewrite.

## Technology

The project is based mainly on:

- C#
- XNA 4.0 original API structure
- MonoGame
- Windows Phone game code
- ILSpy-decompiled source code
- Git subtree repository merging

## Repository history

The current repository was created by merging several related repositories into `mobile-eggbert-legacy` using `git subtree`.

This makes it possible to keep multiple related parts of the project together while still preserving their historical origin.

## Building

Build steps may depend on the exact project version and installed MonoGame/.NET environment.

Typical steps are:

```bash
git clone https://github.com/robertvokac/mobile-eggbert-legacy.git
cd mobile-eggbert-legacy
dotnet restore
dotnet build
````

If the project uses an older MonoGame or .NET Framework setup, it may need to be opened and built from an IDE such as Visual Studio or JetBrains Rider.

## Notes

This repository is not a clean-room rewrite. It is based on decompiled and migrated legacy code.

Because of that, the code may contain:

* decompiler artifacts,
* old XNA naming and structure,
* platform-specific Windows Phone assumptions,
* incomplete or imperfect MonoGame migration code,
* historical code from multiple merged repositories.

## Legal notice

This is an unofficial preservation and development repository.

All original game names, characters, graphics, sounds, music, and other assets belong to their respective owners.

This repository is intended for educational, archival, and experimental purposes. Do not use original proprietary assets commercially unless you have the legal right to do so.

## License

No license is currently specified.

Until a license is added, all rights are reserved by their respective owners.
