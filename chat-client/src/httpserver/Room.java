package httpserver;

import java.io.IOException;
import java.nio.channels.Pipe;
import java.nio.channels.SocketChannel;
import java.util.ArrayList;
import java.util.HashMap;

public class Room {
	ArrayList<Messages> messagelist;
	ArrayList<WebSocket> wslist;
	ArrayList<String> userlist;
	public Room() {
		wslist = new ArrayList<WebSocket>();
		messagelist = new ArrayList<Messages>();
		userlist = new ArrayList<String>();
	}
	public synchronized void postMessage(Messages message) throws IOException {
		messagelist.add(message);
		for (WebSocket s: wslist) {
			s.writeToPipe(message);
		}
		System.out.println("Current users: ");
		for (String u: userlist) {
			System.out.println(u);
		}
	}
	public void addUser(WebSocket ws) throws IOException {
		wslist.add(ws);
		String user = ws.getUser();
		if (!userlist.contains(user)){
			userlist.add(user);
		}
		for (Messages m: messagelist) {
			ws.writeToPipe(m);
		}
//		for (WebSocket s: wslist) {
//			s.writeToPipe(message);
//		}
	}
	public void removeUser(WebSocket ws) {
		wslist.remove(ws);
		String user = ws.getUser();
		if (userlist.contains(user)){
			userlist.remove(user);
		}
		
	}
}
