package httpserver;

import java.io.Serializable;

public class Messages implements Serializable {
	private String user;
	private String message;
	public Messages(String clientmessage) {
		String[] msg = clientmessage.split(" ",2);
		user = msg[0];
		message = msg[1];
	}
	public String toJSON() {
		String toJSON= "{\"user\":\"" + this.user + "\", \"message\":\"" + this.message + "\"}";
		System.out.println(toJSON);
		return toJSON;
	}
	public String getUser() {
		return this.user;
	}
	public String getMessage() {
		return this.message;
	}
	
}
