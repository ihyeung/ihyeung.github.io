package httpserver;

import java.io.DataOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.io.PrintWriter;
import java.net.Socket;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.util.Base64;
import java.util.Scanner;

public class HTTPResponse {
	public  File file;
	public Socket connection;
	public HTTPResponse (Socket connection, File file) throws IOException, InterruptedException, BadRequestException, NoSuchAlgorithmException{
			this.connection = connection;
			this.file = file;
			try {
			OutputStream out = connection.getOutputStream();
			PrintWriter output = new PrintWriter(out);
			FileInputStream ins = new FileInputStream(file);
			
			if (!this.file.exists()) {    //HTTP request contains invalid file path
				output.print("HTTP/1.1 404 NOT FOUND\r\n");
				output.print("Content-Length: 0 \r\n");
				output.print("Content-Type: \r\n");
				output.print("\r\n");
				output.flush();
				output.close();
			}
			else {
				output.print("HTTP/1.1 200 OK \r\n");
				output.print("Content-Length: " + file.length() + "\r\n");
				output.print("Content-Type: \r\n");
				output.print("\r\n");
				output.flush();
				
				byte[] buffer = new byte[1024];
				int buffersize = 0;
				do {
					buffersize = ins.read(buffer);
					this.connection.getOutputStream().write(buffer);
//					connection.getOutputStream().flush();
//					Thread.sleep(50);
				}
				while (buffersize > 0);
			}
				output.flush();
				output.close();
				out.close();
				ins.close();
				
			}
			catch(IOException exception) {
				System.out.println(exception);
			}
	}
}
