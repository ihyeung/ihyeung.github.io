"use strict";
var userlist = [];
var chatrooms = [];
window.onload = function(){
    let url = "ws://" + location.host;
    var chatSocket = new WebSocket(url);
    chatSocket.onopen = function(){
        console.log("Socket open!");
        console.log("Chat Socket readystate: " + chatSocket.readyState);
    };
    //DOM ELEMENTS HERE
    let loginbutton = document.getElementById("login");
    let enterbutton =  document.getElementById("enter");
    let postbutton = document.getElementById("post");
    let username = document.getElementById("user");
    let password = document.getElementById("pass");
    let loginstatus = document.getElementById("loginstatus");
    let loginwindow = document.getElementById("loginwindow");
    let messagefield = document.getElementById("newmessage");
    let enterchatroomwindow = document.getElementById("enterchatroom");
    let room = document.getElementById("room");
   
    let messagedisplay = document.getElementById("messagedisplay");
    let leftpanel =  document.getElementById("leftpanel"); // this is the left panel with active chatrooms and userlist
    let chatroomdiv = document.getElementById("chatroom"); //this is the div that houses the message panel    
    let messagediv = document.getElementById("inputmessage"); //this is the div that houses the message input field and button
   
    //Button actions here
    enterbutton.addEventListener("click", function(){
        enterchatroomwindow.style.visibility = "hidden";
        chatroomdiv.style.visibility = "visible";
        messagedisplay.style.visibility = "visible";
        messagediv.style.visibility = "visible";
        leftpanel.style.visibility = "visible";
        chatSocket.send( "join " + room.value + " " + username.value); 
    });

   loginbutton.onclick = join;
   postbutton.onclick = function(){
        var message = username.value + " " + messagefield.value;
        chatSocket.send(message);
    };
    //chatsocket events
    chatSocket.onerror = errorHandler;
    chatSocket.onclose = function(event){
       console.log("Socket closed :(");
       console.log(event);
   };
    chatSocket.onmessage = function(event){
         var message = JSON.parse(event.data);
        var time = new Date();
        var t = time.toUTCString();
        var user = document.createTextNode(message.user);
        var msg = document.createTextNode(message.message);
        var timestamp = document.createTextNode("Message sent at: " + t);
        var m = document.createElement("span");
       var entiremessage = document.createElement("p");
        m.appendChild(user);
        m.id = "username";
        var x = document.createElement("span");
        x.appendChild(msg);
        x.id = "usermessage";
        var t = document.createElement("span");
        t.appendChild(timestamp);
        t.id = "timestamp";
        let paragraph = document.createElement("p");
        paragraph.appendChild(m);
        paragraph.appendChild(x);
        paragraph.appendChild(t);
        paragraph.id = "singlemessage";
       let div = document.createElement("div").appendChild(paragraph);
        div.id = "messages";
       messagedisplay.appendChild(div);
       messagedisplay.scrollTo(0,document.body.scrollHeight);
 };

};

function errorHandler(error) {
    console.log("Socket Connection Error:" + error);
}

function join(){
    let validlogin = false;
    let user = document.getElementById("user").value;
    let password = document.getElementById("pass").value;
    let loginstatus = document.getElementById("loginstatus");
    let loginwindow = document.getElementById("loginwindow");
    let enterchatroomwindow = document.getElementById("enterchatroom");
    if (!user.length || !password.length){
        loginstatus.style.visibility = "visible";
        loginstatus.textContent = "Error: Invalid Username or Password";
        loginwindow.style.visibility = "visible";
        enterchatroomwindow.style.visibility = "hidden";

    }
    else if(user.length && password.length){
        loginstatus.textContent = "Login Success. Please wait...";
        loginstatus.style.visibility = "visible";
        validlogin = true;
    }
    
    if (validlogin){
        loginstatus.style.visibility = "visible";
        loginstatus.textContent = "Login Success. Please wait...";
        loginstatus.style.visibility="hidden";
        loginwindow.style.visibility="hidden";
        enterchatroomwindow.style.visibility = "visible";
    
        }
    }
