#include <stdio.h>
#include <unistd.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <linux/kernel.h>

int write_key(int fd, unsigned char* buf, size_t buflen) {
  int bytes_written;
  if ((bytes_written = write(fd, buf, buflen)) >= 0) {
    printf("\n%d bytes written\n", bytes_written);
  } else {
    printf("\nrc4 character device write failure\n");
  }
  return bytes_written;
}

int read_bytes(int fd, unsigned char* buf, size_t numbytes) {
  int bytesread;
  if ((bytesread = read(fd, buf, numbytes)) >= 0) {
    printf("\n%d bytes read\n", bytesread);
    for (int i = 0; i < bytesread; i++) {
      printf("rcbuf[%d] = %c\n", i, buf[i]);
    }
  } else {
    printf("\nrc4 character device read failure\n");
  }
  return bytesread;
}

int main(void) {
  int fd;
  static unsigned char rcbuf[5] = {'a', 'a', 'a', 'a', 'a'};
  static unsigned char rcbuf1[5] = {'x', 'y', 'z', 0, 4};
  static unsigned char emptybuf[10];
  static unsigned char setbuf[10] = {0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF};
  char * filepath = "/dev/myRand";
  printf("\nopening /dev/rand RC4 character device driver\n");
  fd = open(filepath, O_RDWR, S_IRWXU);
  if (fd < 0) {
    printf("Error opening driver\n");
    return 1;
  } else {
  printf("\nDriver opened successfully.\n");
  //TEST 1
  write_key(fd, rcbuf, sizeof(rcbuf));
  read_bytes(fd, rcbuf,sizeof(rcbuf));

  write_key(fd, rcbuf, sizeof(rcbuf));
  //Seeding rc4 with the same key should produce the same output values
  read_bytes(fd, rcbuf, sizeof(rcbuf));


  printf("\nNow seed rc4 with a different key and check whether it produces different output values\n");

  //TEST 2: Seeding rc4 generator with a different key should produce different output values

  write_key(fd, rcbuf, sizeof(rcbuf));
  read_bytes(fd, rcbuf, sizeof(rcbuf));
  write_key(fd, rcbuf1, sizeof(rcbuf1));
read_bytes(fd, rcbuf, sizeof(rcbuf));

  write_key(fd, emptybuf, sizeof(emptybuf));
  read_bytes(fd, emptybuf, sizeof(emptybuf));

  write_key(fd, setbuf, sizeof(setbuf));
  read_bytes(fd, setbuf, sizeof(setbuf));
  read_bytes(fd, emptybuf, sizeof(emptybuf));
  }
}
