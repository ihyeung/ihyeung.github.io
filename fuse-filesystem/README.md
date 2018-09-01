# FUSE Encryption Filesystem

A simple pass through FUSE filesystem written in Python, that utilizes
Fernet symmetric encryption
(https://cryptography.io/en/latest/fernet/).

This implementation is built on top of the FUSE library example code.

``` $ python3 encfs.py test/ mountpoint ```
