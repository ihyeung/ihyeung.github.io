package httpserver;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.PrintWriter;
import java.net.MalformedURLException;
import java.net.ProtocolException;
import java.net.Socket;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.util.Base64;
import java.util.HashMap;
import java.util.Map;
import java.util.NoSuchElementException;
import java.util.Scanner;

public class HTTPRequest {
	private String path;
	private  File file;
	public Socket clientsocket;
	public HashMap<String,String> requestmap;
	private String handshakehash;
	private boolean iswebsocket;
	public HTTPRequest(Socket connection) throws IOException, BadRequestException, NoSuchAlgorithmException{
		this.iswebsocket = false;
		this.handshakehash = null;
		this.requestmap = new HashMap<String,String>();
		this.clientsocket = connection;
		try {	
			Scanner scanner = new Scanner(connection.getInputStream());
			if (!scanner.hasNextLine()) {
				throw new BadRequestException("Error: End of HTTP Request Reached");
				
			}
			else {
				String r = scanner.nextLine();
				String [] requestheader = r.split(" ");

				if (requestheader.length!= 3) {
					throw new BadRequestException("Invalid HTTP Request Format");
				}
				this.path =requestheader[1];
				String requesttype = requestheader[0];
				String httpprotocol = requestheader[2];
				if (!requesttype.equals("GET") || !httpprotocol.equals("HTTP/1.1")){
					throw new BadRequestException("Invalid HTTP Request Type/Protocol");
				}
				this.file = new File("resources/" + this.path);
				requestmap.put(requestheader[0],requestheader[1]);
				while (true) { 
					String line = scanner.nextLine();
					if (line==null || line.isEmpty()){
						break;
					}
					else {
						String[] header = line.split(": ");
						requestmap.put(header[0], header[1]);
						if (requestmap.containsKey("Sec-WebSocket-Key")) { 
							String socketkey = requestmap.get("Sec-WebSocket-Key");
							this.handshakehash = Base64.getEncoder().encodeToString(MessageDigest.getInstance("SHA-1").digest((socketkey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11").getBytes()));
							this.iswebsocket = true; //then request is websocket
						}
					}
				}

			}
			
		}
			catch(IOException exception) {
				System.out.println("I/O Error");
			}
		}
		public File getFile() {
			return this.file;
		}
		public HashMap<String,String> getRequest(){
			return this.requestmap;
		}
		public String getHash() {
			return handshakehash;
		}
		public boolean iswebsocket() {
			return this.iswebsocket;
		}
	}

