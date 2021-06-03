#!/usr/bin/env python3
import pathlib
import shutil
import sys
from typing import List, Union

IGNORED_DLLS = [
    "System.Runtime.InteropServices.WindowsRuntime.dll"
]


def help(error: Union[None, str] = None):
    print("Copies all .dll files from one directory to another")
    print()
    print("\tcopy_dll.py [source directory] [destination directory]")

    if error:
        print("Script failed:")
        print("\t" + error)


def audit_hook(a, b):
    if a == "shutil.copyfile":
        src, dest = b
        print("- " + str(src))
        print("\t-> " + dest)


def main(args: List[str]):
    sys.addaudithook(audit_hook)

    # Enforce arguments

    if "--help" in args or "-h" in args:
        help()
        return 0

    if len(args) != 2:
        help("Requires 2 arguments")
        return 1

    src, dest = pathlib.Path(args[0]), pathlib.Path(args[1])

    if not src.is_dir():
        help("Source directory does not exist")
        return 1

    if not dest.is_dir():
        help("Destination directory does not exist")
        return 1

    # Copy over new .dlls
    for f in src.iterdir():
        if f.is_file() and f.suffix == ".dll" and f.name not in IGNORED_DLLS:
            shutil.copy2(f, dest)

    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
