package httpserver;

import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.PrintWriter;
import java.net.Socket;
import java.net.SocketException;
import java.nio.channels.SocketChannel;
import java.security.NoSuchAlgorithmException;
import java.util.HashMap;
import java.util.Scanner;

public class NewConnection implements Runnable {
	private SocketChannel connection;
	private Server server;
	public NewConnection(Server server, SocketChannel connection) {
		this.connection = connection;
		this.server = server;
	}
	@Override
	public void run() {
		File file = null;
		HTTPRequest request = null;
		HTTPResponse httpresponse = null;
		WebSocket websocket = null;
		String hash = null;
		HashMap<String,String> requestheader = null;
		// TODO Auto-generated method stub
		try {
			request = new HTTPRequest(this.connection.socket());
			file = request.getFile();
			hash = request.getHash();
			requestheader = request.getRequest();
			if (!request.iswebsocket()){
			httpresponse = new HTTPResponse(this.connection.socket(), file);
			}
			else if (request.iswebsocket()){
				websocket = new WebSocket(this.connection, requestheader, hash, server);
			}
		} catch (BadRequestException e) {
			// TODO Auto-generated catch block
			System.out.println("Error: Bad Request exception");
			e.printStackTrace();
		} catch(SocketException e) {
			System.out.println("Exception message: " + e.getMessage());
			System.out.println(e.getStackTrace());
			} catch (NoSuchAlgorithmException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (InterruptedException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
	}
		
	}

}
