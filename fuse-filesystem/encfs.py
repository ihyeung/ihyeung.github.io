"""A naive passthrough FUSE filesystem.
"""

from __future__ import with_statement

from functools import wraps
import os
import sys
import errno
import logging

from fuse import FUSE, FuseOSError, Operations
import inspect

# modules/packages I added in
import getpass
import base64
from cryptography.fernet import Fernet
from cryptography.hazmat.backends import default_backend
from cryptography.hazmat.primitives import hashes
from cryptography.hazmat.primitives.kdf.pbkdf2 import PBKDF2HMAC



log = logging.getLogger(__name__)


def logged(f):
    @wraps(f)
    def wrapped(*args, **kwargs):
        log.info('%s(%s)', f.__name__, ','.join([str(item) for item in args[1:]]))
        return f(*args, **kwargs)
    return wrapped


class EncFS(Operations):

    def __init__(self, root):
        self.root = root
        self.usr_pass = getpass.getpass(prompt = 'Password: ', stream = None)
        self.salt = os.urandom(16)
        self.nextFD = 3
        self.in_mem_files = dict() # mapping of fd's to unencrypted file bytes stored in memory

    def destroy(self, path):
        pass

    def _full_path(self, partial):
        if partial.startswith("/"):
            partial = partial[1:]
        path = os.path.join(self.root, partial)
        return path

    # @logged
    def access(self, path, mode):
        full_path = self._full_path(path)
        if not os.access(full_path, mode):
            raise FuseOSError(errno.EACCES)

    # @logged
    def chmod(self, path, mode):
        full_path = self._full_path(path)
        return os.chmod(full_path, mode)

    # @logged
    def chown(self, path, uid, gid):
        full_path = self._full_path(path)
        return os.chown(full_path, uid, gid)

    # @logged
    def getattr(self, path, fh=None):
        full_path = self._full_path(path)
        st = os.lstat(full_path)
        return dict((key, getattr(st, key)) for key in ('st_atime', 'st_ctime',
               'st_gid', 'st_mode', 'st_mtime', 'st_nlink', 'st_size', 'st_uid'))

    # @logged
    def readdir(self, path, fh):
        full_path = self._full_path(path)

        dirents = ['.', '..']
        if os.path.isdir(full_path):
            dirents.extend(os.listdir(full_path))
        for r in dirents:
            yield r

    # @logged
    def readlink(self, path):
        pathname = os.readlink(self._full_path(path))
        if pathname.startswith("/"):
            # Path name is absolute, sanitize it.
            return os.path.relpath(pathname, self.root)
        else:
            return pathname

        #don't allow mknod
    '''@logged
    def mknod(self, path, mode, dev):
        """Make a special file.

        Make a special (device) file, FIFO, or socket. See mknod(2) for
        details. This function is rarely needed, since it's uncommon to make
        these objects inside special-purpose filesystems.

        """
        return os.mknod(self._full_path(path), mode, dev)
    '''
    # @logged
    def rmdir(self, path):
        full_path = self._full_path(path)
        return os.rmdir(full_path)

    # @logged
    def mkdir(self, path, mode):
        return os.mkdir(self._full_path(path), mode)

    # @logged
    def statfs(self, path):
        full_path = self._full_path(path)
        stv = os.statvfs(full_path)
        return dict((key, getattr(stv, key)) for key in ('f_bavail', 'f_bfree',
          'f_blocks', 'f_bsize', 'f_favail', 'f_ffree', 'f_files', 'f_flag',
          'f_frsize', 'f_namemax'))

    # @logged
    def unlink(self, path):
        return os.unlink(self._full_path(path))

    #no symlinks
    '''    @logged
    def symlink(self, target, name):
        """Create a symbolic link.

        Create a symbolic link named "from" which, when evaluated, will lead to
        "to". Not required if you don't support symbolic links. NOTE:
        Symbolic-link support requires only readlink and symlink. FUSE itself
        will take care of tracking symbolic links in paths, so your
        path-evaluation code doesn't need to worry about it.

        """
        return os.symlink(self._full_path(target), self._full_path(name))
    '''
    # @logged
    def rename(self, old, new):
        return os.rename(self._full_path(old), self._full_path(new))
#no hard links either

    # @logged
    def link(self, target, name):
        """Create a hard link.

        Create a hard link between "from" and "to". Hard links aren't required
        for a working filesystem, and many successful filesystems don't support
        them. If you do implement hard links, be aware that they have an effect
        on how unlink works. See link(2) for details.

        """
        return os.link(self._full_path(target), self._full_path(name))

    # @logged
    def utimens(self, path, times=None):
        return os.utime(self._full_path(path), times)

    """
    Opens a file.

    If the path is invalid, an error is thrown.
    If the user doesn't have adequate access permissions, or
    the file is already open (i.e, it is already loaded into memory), returns -1.
    Otherwise, the file is opened and the encrypted bytes of the file are read.
    The file bytes are decrypted using a symmetric key generated from
    the user password and stored in the in-memory dictionary.
    Only the correct user password will decrypt the file bytes correctly.
    Returns the associated file descriptor for that file.
    """

    @logged
    def open(self, path, flags):
        full_path = self._full_path(path)
        print('OPENING THE FILE "' + full_path + '"')
        if not os.path.exists(full_path):
            print('file does not exist')
            # raise FuseOSError(errno.ENOENT)
            return -1
        if not os.access(full_path, flags): #if access returns False, access unsuccessful
            print('You do not have permission to open this file.')
            return -1
         #file access success
        if full_path in self.in_mem_files.keys():
            print('File with filename' + full_path + 'is already open and in memory.')
            return -1
        fd = os.open(full_path, flags) #returns file object
        if fd == -1:
            raise FuseOSError(errno.EBADF) #fd already contained in in memory file dict

        # otherwise file exists and hasnt been opened yet
        file_size = os.path.getsize(full_path)
        print('Total number of bytes contained in file: ' + str(file_size))
        saltbytes= os.read(fd, 16) # first 16 bytes contain salt
        print('salt bytes read from file:' + str(saltbytes))
        filecontents = bytearray(b'')
        while True:
            chunk =  os.read(fd, 1024)
            if not chunk:
                print('Bytes read reached eof.\n')
                break
            filecontents.extend(chunk)
        print('file bytes: ' + filecontents.decode('ASCII'))
        os.close(fd)
        key = PBKDF2HMAC( # run password through key derivation function
        algorithm=hashes.SHA256(),
        salt= saltbytes,
        length = 32,
        iterations=100000,
        backend=default_backend()
        )
        keydec = base64.urlsafe_b64encode(key.derive(self.usr_pass.encode('ASCII')))
        f = Fernet(keydec)
        print('Decrypted file bytes: \n')
        token = bytes(filecontents)
        plaintext = f.decrypt(token)
        self.in_mem_files[full_path] = plaintext
        print('Decrypted text: ' + plaintext.decode('ASCII'))
        self.nextFD += 1
        return self.nextFD

    """
    Creates a new file.
    Creates a new empty file named path and adds empty entry
    to the dictionary of open files.
    Increments the fd counter if the file was created successfully.
    Returns the fd value of the created file.
    """
    @logged
    def create(self, path, mode, fi=None):
        full_path = self._full_path(path)
        if os.path.exists(full_path):
            print('create() error: file already exists\n')
            raise FuseOSError(errno.ENOENT)
        # try:
        fd = os.open(full_path, os.O_RDWR | os.O_CREAT, mode)
        # fd = os.open(full_path, os.O_CREAT, mode)
        # except IOError as e:
        #     print('Create function Error: Error opening file\n')
        #     raise e
        # if fd == -1:
        #     print('CREATE ERROR: bad file descriptor!\n')
        #     return -1
        new_file = ''
        self.in_mem_files[full_path] = new_file.encode('ASCII') #convert string to bytes
        print('Now printing all of the files stored in virtual memory system... ')
        for k in self.in_mem_files.keys():
            print('filename: ' + k)
        self.nextFD += 1
        os.close(fd) # close encrypted file
        return self.nextFD

    """
    Read from a file.

    Reads length bytes from the given file in the in-memory dictionary, beginning
    at offset bytes.
    Returns an array-like type containing the appropriate bytes of data.
    If the file is not loaded into the in-memory dictionary, returns -1.
    """
    @logged
    def read(self, path, length, offset, fh):
        full_path = self._full_path(path)
        if full_path in self.in_mem_files.keys():
            print ('file is loaded into memory \n')
            file = self.in_mem_files[full_path]
            return file[offset: offset + length]
        print('Read error: File not loaded into memory.\n')
        return -1

    """
    Write to a file.
    If the file is already loaded into memory, existing bytes in in-memory
    dictionary will be overwritten.
    Returns the number of bytes written.
    """
    @logged
    def write(self, path, buf, offset, fh):
        full_path = self._full_path(path)
        if full_path in self.in_mem_files.keys():
            print('Write function: file is already loaded in memory, file bytes will be overwritten\n')
            file = bytearray(self.in_mem_files[full_path])
            file[offset:offset+len(buf)] = bytearray(buf)
        else:
            print('Writing bytes to a file that has not already been loaded into memory')
            self.in_mem_files[full_path] = buf
        return len(buf)

    """
    Truncates a file.
    Truncates or lengthens a given file so it is precisely length bytes long.
    """
    @logged
    def truncate(self, path, length, fh=None):
        full_path = self._full_path(path)
        file_size = os.path.getsize(full_path)
        if file_size == length:
            print('no truncate needed, returning from function\n')
            return 0
        if full_path not in self.in_mem_files.keys():
            print('truncate error file not in memory\n\n\n\n\n\n')
            return -1
        else:
            print ('truncating decrypted file bytes...\n')
            file = self.in_mem_files[full_path]
            if length > len(file):
                print('truncate function: adding zero bytes to end\n')
                curr_file = self.in_mem_files[full_path]
                new_file = bytearray(b'')
                new_file.append(curr_file)
                while len(new_file) < length:
                    new_file.append(0)
                print('new padded file bytes:\n')
                print(new_file)
                self.in_mem_files[full_path] = new_file
                return 0
            elif length < len(file):
                print('truncate function: truncating in-memory file bytes\n')
                trunc_file = file[:length]
                self.in_mem_files[full_path] = trunc_file
                return 0
    #skip
    '''
    # @logged
    def flush(self, path, fh):
        """Flush buffered information.

        Called on each close so that the filesystem has a chance to report
        delayed errors. Important: there may be more than one flush call for
        each open. Note: There is no guarantee that flush will ever be called
        at all!

        """
        return os.fsync(fh)
   '''
   # """
   # Releases a file when the last open copy of a file is closed, generally called when a user calls close().
   # This method generates a new random 16 byte salt, encrypts the contents of
   # the file from accessing the current file bytes in the in-memory dictionary,
   # then writes the salt + newly encrypted file bytes to the physical filesystem.
   # If the file already exists, the existing file bytes on disk will be
   # overwritten.
   # """
    @logged
    def release(self, path, fh):
        newsalt = os.urandom(16) # new saltbytes
        full_path = self._full_path(path)
        if full_path not in self.in_mem_files.keys():
            print('error: trying to call release on a function not in memory')
            return
        file = self.in_mem_files[full_path]
        print('this is the file being relesased..........')
        print(file.decode('ASCII'))
        print('full path: ' + full_path)
        key = PBKDF2HMAC( # run password through key derivation function
            algorithm=hashes.SHA256(),
            salt= newsalt,
            length = 32,
            iterations=100000,
            backend=default_backend()
            )
        keyenc = base64.urlsafe_b64encode(key.derive(self.usr_pass.encode('ASCII')))
        f = Fernet(keyenc)
        token = f.encrypt(file) #token now has encrypted file bytes
        # del file
        self.nextFD -= 1
        del self.in_mem_files[full_path]
        encoded = bytearray(b'')
        encoded.extend(newsalt)
        encoded.extend(token)
        print('Newly re-encrypted file:')
        print(encoded)
        fd = os.open(full_path, os.O_WRONLY)
        os.write(fd, encoded)
        os.close(fd)
        return 0

'''    @logged
    def fsync(self, path, fdatasync, fh):
        """Flush any dirty information to disk.

        Flush any dirty information about the file to disk. If isdatasync is
        nonzero, only data, not metadata, needs to be flushed. When this call
        returns, all file data should be on stable storage. Many filesystems
        leave this call unimplemented, although technically that's a Bad Thing
        since it risks losing data. If you store your filesystem inside a plain
        file on another filesystem, you can implement this by calling fsync(2)
        on that file, which will flush too much data (slowing performance) but
        achieve the desired guarantee.

        """
        return self.flush(path, fh)
'''
if __name__ == '__main__':
    from sys import argv
    if len(argv) != 3:
        print('usage: %s <encrypted folder> <mountpoint>' % argv[0])
        exit(1)
    logging.basicConfig(level=logging.DEBUG)
    #create our virtual filesystem using argv[1] as the physical filesystem
    #and argv[2] as the virtual filesystem
    fuse = FUSE(EncFS(argv[1]), argv[2], foreground=True)
