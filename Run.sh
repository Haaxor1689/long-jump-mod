#!/usr/bin/env bash
# use this script in case you're using CLI to test the mod
# -m:1 keeps the target in the entry MSBuild node so the game inherits the real console
dotnet build . -t:RunAllumeriaWithMod -v:d -tl:off