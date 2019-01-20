package httpserver;

import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.ObjectInputStream;
import java.io.ObjectOutputStream;
import java.io.OutputStream;
import java.io.PrintWriter;
import java.math.BigInteger;
import java.net.Socket;
import java.nio.channels.Channels;
import java.nio.channels.Pipe;
import java.nio.channels.SelectionKey;
import java.nio.channels.Selector;
import java.nio.channels.SocketChannel;
import java.security.NoSuchAlgorithmException;
import java.util.HashMap;
import java.util.Iterator;

public class WebSocket{
	private SocketChannel connection;
	Server server;
	Room room;
	Pipe pipe;
	String username;
	public WebSocket(SocketChannel connection, HashMap<String,String> requestheader, String hash, Server server) throws IOException, NoSuchAlgorithmException {
		this.connection = connection;
		this.server = server;
		DataInputStream in = new DataInputStream(connection.socket().getInputStream());
		//check if handshake was successful first
		if (this.handshake(connection.socket(), requestheader, hash)) {
			System.out.println("handshake was successful");
			//if handshake was successful, first message in should be "join roomname username"
			String[] joinmsg = receiveMessage(in).split(" ");
			String message = joinmsg[1];
			username = joinmsg[2];
			if (!joinmsg[0].equals("join")) {
				System.out.println("Invalid join message: ");
			}
			this.room  = server.getRoom(message);
			System.out.println("Your room: " + room);
			System.out.println("Joining " + room + " as " + username);
			//initialize pipe here so not null pointer exception
			pipe = Pipe.open();
			//add new ws connection user to chatroom
			this.room.addUser(this);
			MessageHandler(connection.socket(), connection);
		}
		else {System.out.println("handshake failed");}
	}

	public void MessageHandler(Socket connection, SocketChannel sc) throws IOException {
		
		Selector select = Selector.open(); //will tell us when there is data to read
		sc.configureBlocking(false); 
		pipe.source().configureBlocking(false);
		sc.register(select, SelectionKey.OP_READ); //register socket channel with selector
		pipe.source().register(select, SelectionKey.OP_READ); //register pipe with selector
		while (!connection.isClosed()) { 
			try {
				select.select();
				Iterator<SelectionKey> selectedKeys = select.selectedKeys().iterator();
				while (selectedKeys.hasNext()) {
					SelectionKey key = (SelectionKey) selectedKeys.next();
					selectedKeys.remove();
					if (key.isReadable() && key.channel() == sc) {
						key.cancel();
						DataInputStream in = new DataInputStream(connection.getInputStream());
						sc.keyFor(select).cancel();
						sc.configureBlocking(true);
						Messages message = new Messages(receiveMessage(in));
						this.room.postMessage(message);
						sc.configureBlocking(false);
						select.selectNow();
						sc.register(select, SelectionKey.OP_READ);
					}
					if (key.isReadable() && key.channel() == pipe.source()) { // if pipe woke you up, have to block sc & pipe
						key.cancel();
						sc.keyFor(select).cancel();
						sc.configureBlocking(true);
						pipe.source().configureBlocking(true);
						ObjectInputStream pipeout = new ObjectInputStream(Channels.newInputStream(pipe.source()));

						Messages msg = (Messages) pipeout.readObject();
						String JSONmessage = msg.toJSON();
						System.out.println("user: " + msg.getUser() + " message: " + msg.getMessage());
						sendMessage(JSONmessage);
						pipe.source().configureBlocking(false);
						select.selectNow();
						pipe.source().register(select, SelectionKey.OP_READ);
						sc.configureBlocking(false);
						select.selectNow();
						sc.register(select, SelectionKey.OP_READ);
					}
				}
			} catch (Exception e) {
				e.printStackTrace();
			}
		}
	}
	public String receiveMessage(DataInputStream in) throws IOException {
		//initialize variables
		int payloadlength = 0; //aka b3 and b4
		int op;
		int maskbit = 0;
		long messagelengthbytes = 0;
		byte[] b1b2 = new byte[2];
		byte[] decodedmessage = null;
		String decoded = "";
		byte[] mask = new byte[4];
		byte[] actualmessage = null;
		byte[] payloaddata = new byte[0];
		//read in first 2 bytes
		in.readFully(b1b2);
		byte b1 = b1b2[0];
		byte b2 = b1b2[1]; 
		//b1 should be FIN(1), RSV(3), and OPCODE(4)
		if((b1>>4  & 0xF)!=8) {
			System.out.println("Error: unexpected bit for standard text frame");
		}
		if ((b1 &0x0F)!=1) {
			System.out.println("Opcode is not 0x1; not a message");
			op=0;
		}
		else {op = 1;}
		//b2 should be Maskbit(1) plus payload length (if <126)
		if (op ==1) {	
		
			if ((b2>>7 &0x1)==1) {
				maskbit = 1;
				//System.out.println("Message received");
			}
			else {
				//System.out.println("Message sent");
				maskbit = 0;
			}
		}
		//if maskbit is 0, dont accept message
		if (maskbit == 1) {	

			if ((b2 & 0x7F) < 126) {//payload is less than 126 bytes
				payloadlength = (b2 & 0x7F);// this var is referring to any extended payload not included in b2
				messagelengthbytes= (b2 & 0x7F);
				//System.out.println("no extra payload length");
				//System.out.println("Message length: " + messagelengthbytes);
			}

			else if ((b2 & 0x7F)== 126) {//payload is 126 bytes , allocate 2 more bytes for length
				payloadlength = (b2 & 0x7F);
				payloaddata = new byte[2];
				in.readFully(payloaddata);
				messagelengthbytes= new BigInteger(payloaddata).intValue();
				//System.out.println("Total Payload including 2bytes  extended payload" + payloadlength);
			}
			else { //8 more bytes for length
				payloadlength = (b2 & 0x7F);
				messagelengthbytes = in.readLong();
				System.out.println("Total Payload including extra 8 bytes extended payload" + payloadlength);
			}
			in.readFully(mask);
			actualmessage= new byte[(int)messagelengthbytes];
			in.readFully(actualmessage);
			decodedmessage = new byte [(int)messagelengthbytes];
			for (int i = 0; i < actualmessage.length; i++) {
				decodedmessage[i] = (actualmessage[i] ^= mask[i%4]);
			}
			decoded = new String(decodedmessage);
		}
		return decoded;
	}
	
	public boolean handshake(Socket connection, HashMap<String,String> requestheader, String hash) throws IOException {
		boolean handshakesuccess = false;
		PrintWriter output = new PrintWriter(connection.getOutputStream());

		if (!requestheader.containsKey("Sec-WebSocket-Key")) { //if no Sec-websocket-key field, not a websocket request
			output.print("HTTP/1.1 400 Bad Request\r\n");
			output.print("Sec-WebSocket-Version\r\n");
			output.print("\r\n");
			output.flush();
		}
//		else if (!requestheader.containsKey("Origin")) { //if no origin field, return 403 forbidden per documentation
//			output.print("HTTP/1.1 403 Forbidden \r\n");
//			output.print("\r\n");
//			output.flush();
//		}
		else {
			output.print("HTTP/1.1 101 Switching Protocols\r\n"); //else return websocket response header
			output.print("Upgrade: websocket\r\n");
			output.print("Connection: Upgrade\r\n");
			output.print("Sec-WebSocket-Accept: " + hash + "\r\n");
			output.print("\r\n");
			output.flush();
			handshakesuccess=true;
		}
		return handshakesuccess;	
	}

		public void sendMessage(String decoded) throws IOException {
			DataOutputStream  out = new DataOutputStream(this.connection.socket().getOutputStream());
			byte[] extendedpayload= new byte[0];
			byte[] header = new byte[2];
			header[0] = (byte) 0x81;
			if (decoded.length() <126) {
				header[1] = (byte) decoded.length();
			}
			else if (decoded.length() ==126) {
				header[1] = 126;
				extendedpayload = new byte[2];
			}
			else if (decoded.length() == 127) {
				header[1] = 127;
				extendedpayload = new byte[8];
			}
			System.out.println(decoded);
				out.write(header);
				out.write(extendedpayload);
				out.write(decoded.getBytes());
				out.flush();
		}
	public void writeToPipe(Messages message) throws IOException {
		(new ObjectOutputStream(Channels.newOutputStream(this.pipe.sink()))).writeObject(message);
	}
	public String getUser() {
		return this.username;
	}
}