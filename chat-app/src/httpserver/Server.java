package httpserver;


import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.PrintWriter;
import java.net.InetSocketAddress;
import java.net.ServerSocket;
import java.net.Socket;
import java.nio.channels.SelectionKey;
import java.nio.channels.Selector;
import java.nio.channels.ServerSocketChannel;
import java.nio.channels.SocketChannel;
import java.util.HashMap;
import java.util.Iterator;
import java.util.Scanner;
import java.util.Set;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.ThreadPoolExecutor;

public class Server {
	final int port;
	ServerSocketChannel ssc;
	Selector selector;
	SocketChannel connection;
	HashMap<String, Room> roomlist;
	final ExecutorService threadpool;
	public Server() throws IOException {
		roomlist = new HashMap<String, Room>();
		this.selector= Selector.open();
		this.port = 8080;
		this.ssc = ServerSocketChannel.open();
		ssc.configureBlocking(false);
		ssc.bind(new InetSocketAddress(port));
		ssc.register(selector, SelectionKey.OP_ACCEPT);
		connection = null;
		threadpool = Executors.newFixedThreadPool(100);
		//final ThreadPoolExecutor pool = new FixedThreadPool();
	}
	public void Serve() throws IOException, BadRequestException{
		try {	
			while (true) {
				selector.select();
				Set<SelectionKey> keys = selector.selectedKeys();
				Iterator<SelectionKey> keyiter= keys.iterator();
				while(keyiter.hasNext()) {
					SelectionKey selectkey = keyiter.next();
					if (selectkey.isAcceptable()) {
						keyiter.remove();
						SocketChannel sc = ssc.accept();
						threadpool.execute(new NewConnection(this, sc));
//						new Thread(new NewConnection(this, sc)).start();
					}
				}
			}
		}
		catch(IOException exception) {
			System.out.println("I/O Error:");
			System.out.println(exception);
		}
	}
	public Room getRoom (String room) {		
		Room newroom;
		if (roomlist.containsKey(room)){
			return roomlist.get(room);
		}
		else {
			newroom = new Room();
			roomlist.put(room, newroom);
			return newroom;
		}
	}
}
